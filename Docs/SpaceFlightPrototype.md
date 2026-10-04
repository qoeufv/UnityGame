# Third-Person Space Flight Prototype

Updated: 2026-09-24. Target: Windows and macOS desktop.

## Try the scene

Open `Assets/Scenes/SpaceFlightPrototype.unity`, or choose **Tools → Open Space Flight Prototype**, then press Play and click the Game view. The scene builds its environment at runtime; the edit-mode hierarchy intentionally contains only the bootstrap object.

Fly toward the ancient gate, brake below 5 m/s within 22 metres of its centre, then press F. Select the planet, station, and moon to explore and scan them. Discoveries last for this Play session only.

| Input | Action |
| --- | --- |
| W / S | Forward thrust / reverse |
| A / D | Turn left / right |
| Up / Down | Pitch up / down |
| Q / E | Roll |
| Shift | Boost while thrusting |
| Space | Brake |
| Right mouse drag | Orbit the following camera |
| Mouse wheel | Change camera distance |
| 1 / 2 / 3 / 4 or Tab | Select gate / planet / station / moon |
| F | Inspect the selected destination when close and slow |
| R | Return to departure and clear trails |

## Implementation

- `SpaceFlightController`: input, acceleration, three-axis steering, roll stabilization, swept CharacterController collision and a bounded test sector. Cruise 18 m/s, boost 42 m/s, reverse 8.1 m/s, sector radius 450 m.
- `SpaceFollowCamera`: following view, mouse orbit, zoom and obstruction handling.
- `SpaceFlightEnvironment`: runtime planet, moon, rings, station, star background and the existing imported stargate. The gate opening is traversable; its rim has colliders.
- `SpaceFlightPrototype`: replaceable primitive ship visuals, destination selection, distance indicator, scan feedback and HUD. Serialized material templates keep shader references available to builds.
- `SpaceFlightSceneSetup`: creates the independent scene; opens an existing scene without rebuilding it.
- `PlanetData` and `PlanetSurfacePrototype`: data-driven landing content. Azure World is a civilized prototype; Pale Moon is a horror/barren prototype. Their generated terrain is intentionally replaceable.
- The landing scene now uses a first-person explorer with mouse look and WASD movement. Azure World is presented as an island research outpost: a broad deck, a landing pad, a visible docked player ship and low volcanic outcrops establish an uneven perimeter. Swimming is intentionally disabled for this slice; the ocean remains an observable surrounding body.
- The first Azure World asset kit is in `Assets/Art/Planets/AzureWorld/Imported/`: research laboratory, residential habitat, communication tower and landing platform. The setup tool binds each FBX and its generated textures and normalizes its runtime height for the prototype layout.
- Azure World exploration now has a scan loop: walk near a laboratory, habitat, communication tower or landing platform, look directly at it and press `F`. The HUD tracks discoveries, reports a short result for each structure, and shows a mission-complete message after all four scans. Raycast-all target matching prevents geometry from blocking a valid scan. Imported building colliders are removed in this slice so their invisible bounds do not trap the explorer; the island terrain remains the walkable collision boundary.
- The surface explorer can jump with `Space` while on land. Swimming remains disabled for the current ocean-observation prototype.
- Surface exit is now part of the loop: approach the docked ship and press `F` to return to the near-orbit flight scene; `Escape` remains an immediate return shortcut. The orbit HUD labels Azure World and Pale Moon as `LAND` destinations when the ship is close and slow enough to interact.
- Tall structures are normalized through quarter-turn orientation fitting, foot-to-deck alignment and player-relative target heights (laboratory 4.8 m, habitat 4.2 m, tower 7 m). This keeps the surface slice readable as a walkable game scene while the authored models are still replaceable.

Controls, environment and validation were handled by three collaborating agents; integration, navigation UI and Unity verification were completed by the manager agent.

## Verification

Unity 6000.6.0f1 compiled and ran the scene on macOS. **Tools → Validate Space Flight** passed in Play Mode: cruise/boost/reverse limits, braking, three-axis rotation, stabilization, disabled input, reset, thin-wall collision, boundary return and camera occlusion/recovery. Tests use temporary objects and clean up afterward.

The Game view was visually checked for ship/environment rendering and readable HUD placement. Destination selection and mouse-wheel zoom were exercised through the UI. The Pale Moon surface scene was opened and run; its terrain, signal beacon and description HUD rendered correctly. The Azure World scene now renders the four imported building assets with their texture maps. The three scenes are registered in the active macOS Build Profile so runtime scene loading can return to orbit. Extended manual flight feel, right-drag orbit feel, Windows runtime and full standalone builds remain to be evaluated.

## Scope and next steps

This is a small local flight playground. Interstellar jumps, seamless large-world streaming, persistent orbit position, combat and final planet materials are not implemented. The Rogue and fleet-tactics prototypes remain available, with development paused.

Next: make the landing transition preserve approach context → tune island traversal and camera scale → replace generated surface details with authored planet kits → add one fly-by transition to a second local sector. Supply a finished ship and planet model only after the movement and camera feel are accepted; current geometry is a placeholder.
