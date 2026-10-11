# Blade Racer authoring guide

## Tile contract

Each reusable track prefab has one `TrackTile`, one `TrackSpline`, and entry/exit `TrackSocket` children. Socket forward follows race direction, so the snap operation aligns both position and travel orientation. Keep gameplay objects (gates, hazards, boost strips) beneath the tile so prefab variants remain self-contained.

Create procedural pieces from **GameObject > Blade Racer > Track**. Available presets are straight, curve, banked turn, climb, chicane, twist, and loop. `ProceduralTrackPiece` exposes length, width, radius, turn/bank angles, elevation, twist count, and control-point density. Its generated `TrackSurface` supplies both the visible ribbon and mesh collision. Use **Rebuild Spline** or **Rebuild Surface** from the component context menus after scripted changes; Inspector changes rebuild automatically.

Socket `profile` values provide a cheap compatibility rule (`standard`, `wide`, `tube`, etc.). Extend this into a ScriptableObject profile when art production needs shared dimensions/material rules.

## Adaptive logic

Create gate data via **Assets > Create > Blade Racer > Gate Definition**. Put a trigger collider plus `TrackGate` in a tile prefab and assign the asset. `TrackGate.Triggered` is the decoupled integration seam: race flow, audio, VFX, AI, route switching, and combat systems can subscribe without modifying the tile.

## Recommended next slices

1. Mesh extrusion and banking preview from `TrackSpline`.
2. Input Actions asset and player/device pairing.
3. Weapon hardpoints, damage interfaces, and pooled projectiles.
4. Nearest-point spline queries for AI and recovery.
5. Addressable tile catalog plus seeded procedural assembly.
6. EditMode tests for snapping and PlayMode tests for checkpoint order.

The demo supports keyboard, gamepad, and multi-touch through `VehicleInputSource`. `PlatformBootstrap` hides touch controls on desktop and applies conservative mobile frame-pacing and sleep settings. The touch canvas uses `SafeAreaFitter`, so controls avoid notches and home indicators in either orientation.

## Hover and magnetic assist

`HoverMagnetController` uses four configurable ray probes against the generated track collider, but applies no forces. It reports an averaged surface point/normal and nearby spline direction to `HoverVehicleController`. The arcade motor directly smooths its kinematic pose toward that hover frame, giving predictable banks, twists, walls, and loops on desktop and mobile. A low `trackFollowBias` adds centering and heading nudges; set it to zero for manual steering or raise it for stronger racing-line assistance.

For production, replace the local-device polling with an Input Actions asset when adding rebinding, local multiplayer, or device pairing. Keep `VehicleInputSource` as the vehicle-facing boundary so physics does not become coupled to a particular input system.

Use **Blade Racer > Apply Recommended Platform Settings** once to select landscape autorotation, Android API 26+, iOS 15+, and IL2CPP for mobile builds. Bundle identifiers, signing teams, icons, and store metadata remain project-specific and are intentionally not guessed by the scaffold.
