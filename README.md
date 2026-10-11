# Blade Racer

A modular anti-gravity racing/combat prototype scaffold for Unity 6.

## Start here

1. Add the [`Unity`](Unity) folder to Unity Hub using Unity `6000.0.32f1` (or let a newer Unity 6 editor upgrade it). Do not open the repository root as a Unity project.
2. Create an empty scene and choose **Blade Racer > Create Demo Track** to generate a chained showcase of straights, banks, climbs, twists, loops, and chicanes.
3. Enter Play Mode and drive with WASD/arrow keys or a gamepad. Boost with Space/gamepad South; reset with R/Select.
4. Add track pieces with **GameObject > Blade Racer > Track**, edit their procedural parameters in the Inspector, then use the `TrackTile` Inspector to snap one tile to another.
5. Run **Blade Racer > Apply Recommended Platform Settings** before creating Android or iOS builds.

## Repository layout

```text
blade-racer/
├── Docs/                   Project and authoring documentation
├── Unity/                  Unity project root—open this folder in Unity Hub
│   ├── Assets/
│   ├── Packages/
│   └── ProjectSettings/
├── .gitignore
└── README.md
```

## Architecture summary

- `TrackSpline`: lightweight local-space Catmull-Rom spline with position, tangent, and frame sampling.
- `TrackTile`: reusable prefab root containing a spline, entry/exit snap sockets, bounds, and optional gates/triggers.
- `ProceduralTrackPiece`: editable recipes for straight, curved, banked, climbing, chicane, twist, and loop geometry.
- `TrackSurface`: generated ribbon mesh, UVs, thickness, and collision sampled from any `TrackSpline`.
- `TrackAssembler`: validates tile connections and provides runtime track sampling.
- `GateDefinition`: ScriptableObject data for checkpoints, boosts, hazards, lap lines, and route switches.
- `TrackGate`: scene/prefab trigger that publishes strongly typed gate events.
- `RaceDirector`: validates checkpoint order and counts laps without coupling vehicle code to tile code.
- `HoverVehicleController`: deterministic kinematic arcade motor with speed curves, steering, drift, collision sweeps, boost, and recovery.
- `HoverMagnetController`: raycast-only sensor providing collider surface frames and deliberately limited spline guidance; it never applies forces.
- `VehicleInputSource`: merges keyboard, gamepad, and touch into one gameplay-facing input state.
- `PlatformBootstrap`: selects desktop/mobile frame pacing and toggles the safe-area-aware touch HUD.

See the [authoring guide](Docs/AUTHORING.md) and [architecture notes](Docs/ARCHITECTURE.md) for more detail.
