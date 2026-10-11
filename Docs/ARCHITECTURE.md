# Architecture

Blade Racer is split into small runtime components and editor-only authoring tools. The Unity project lives under `Unity/`; repository documentation and future automation remain outside Unity's asset import pipeline.

## Runtime layers

### Track

- `TrackSpline` supplies dependency-free Catmull-Rom sampling and approximate nearest-point queries.
- `ProceduralTrackPiece` creates spline recipes for straights, curves, climbs, banks, chicanes, twists, and loops.
- `TrackSurface` extrudes a visible ribbon and mesh collider from a spline.
- `TrackSocket` and `TrackTile` define the modular snapping contract.
- `TrackAssembler` owns an ordered tile collection and validates connections.

### Vehicle

- `VehicleInputSource` normalizes keyboard, gamepad, and touch commands.
- `HoverMagnetController` only senses track colliders with ray probes and returns a hover frame.
- `HoverVehicleController` is a kinematic arcade motor. It controls speed, steering, drift, track adhesion, limited racing-line assistance, and collision sweeps without force-based vehicle simulation.

### Race and gates

- `GateDefinition` contains reusable checkpoint, lap, boost, hazard, and route-switch data.
- `TrackGate` converts collider entry into a strongly typed event.
- `RaceDirector` consumes checkpoint and lap events without coupling race state to track prefabs or vehicles.

### Platform

- `PlatformBootstrap` applies desktop/mobile frame-pacing defaults and selects the touch interface.
- `SafeAreaFitter` keeps mobile controls clear of notches and system UI.

## Editor layer

Editor code is isolated in `BladeRacer.Editor`. It creates procedural tile presets, cross-platform demo controls, platform defaults, and tile snapping tools. Runtime builds reference only `BladeRacer.Runtime`.

## Dependency direction

```text
Input ──> Vehicle ──> Track sensing
                    └─> Gate triggers ──> RaceDirector

Editor tools ──> Runtime authoring components
Platform ──> presentation and device defaults
```

Track tiles do not know about racers, weapons, AI, or race state. Those systems communicate through spline sampling, collider queries, and gate events so tiles remain reusable and adaptable.
