# Unity QA and Regression Process

## Environment

Use Unity `6000.6.0f1`, the repository package lock, and a clean checkout when possible. Test the target platform relevant to the change. Editor-only validation is not a substitute for a standalone build check before release.

## Preflight

1. Confirm the branch and commit under test.
2. Confirm `ProjectVersion.txt` and package lock have no accidental changes.
3. Open the project and wait for asset import and script compilation to finish.
4. Check the Console for new errors and exceptions. Warnings require triage when they affect runtime behavior.
5. Confirm the expected scene is included in the active build profile.

## Current smoke test

For the space-flight and Azure World slice:

- Open `Assets/Scenes/SpaceFlightPrototype.unity` and enter Play Mode.
- Confirm the ship, destinations, camera, HUD, and stargate render.
- Select a destination and approach at a safe speed.
- Verify `F LAND` appears near a landable planet and the surface scene loads.
- Walk in each cardinal direction, look around, jump with `Space`, and verify the island boundary is intentional and traversable.
- Confirm the laboratory, habitat, communication tower, and landing platform can be scanned; progress reaches `4/4`.
- Confirm buildings do not trap the player with invisible colliders.
- Approach the docked ship, verify the return prompt, and return to near orbit with `F`; verify `ESC` remains a safe exit.
- Verify a second Play session starts without stale scan state unless persistence is intentionally implemented.

## Regression checklist

| Area | Check |
| --- | --- |
| Input | WASD/thrust, mouse look or orbit, brake, boost, jump, interact, and escape work in their intended mode |
| Camera | No inverted unexpected axis, clipping, frozen camera, or unusable near-field view |
| Transitions | Orbit → surface → orbit loads the intended scene and does not duplicate cameras or UI |
| Movement | Player remains on walkable terrain, cannot pass through the world boundary, and can reach all objectives |
| Interaction | Prompt appears only in range, line-of-sight rules are consistent, and progress updates exactly once |
| Visuals | Materials, lighting, imported textures, markers, and HUD are readable at the target resolution |
| Performance | No sustained frame-time spike, runaway object creation, or repeated console error during a five-minute pass |
| Data | No unexpected reset or mutation of unrelated progression, settings, or scene assets |
| Build | Active scenes and Resources references resolve in a standalone build |

## Evidence standard

Record Unity version, commit, platform, scene, test steps, result, and known failures. For visual work attach before/after screenshots or a short capture. For a failure, include the first relevant console error and reproduction steps; do not report only “does not work.”

## Exit criteria

A feature is accepted when its acceptance criteria pass twice: once in the editor and once in the intended runtime build when the feature touches scene loading, Resources, platform behavior, or packaging. Known limitations must be documented in the owning feature document and the weekly report.
