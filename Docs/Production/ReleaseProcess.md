# Versioning and Release Process

## Version identity

Use `MAJOR.MINOR.PATCH` for playable releases:

- **MAJOR:** incompatible save, input, or product direction change.
- **MINOR:** new player-facing feature or explorable content.
- **PATCH:** bug fix, tuning, documentation, or packaging correction with compatible behavior.

Mark unfinished builds as `0.x.y`; the first public prototype can be `0.1.0`. Every release maps to one Git tag and one reviewed commit. The tag, Unity version, target platform, and build date belong in the release notes.

## Local development builds

Use the Unity editor menu commands:

- `Tools → Build Game Frontier → macOS Development`
- `Tools → Build Game Frontier → Windows Development`

The build script includes `SpaceFlightPrototype`, `Planet_AzureWorld` and `Planet_PaleMoon` in that order. Output is written below `Builds/`, which is ignored by Git. A successful build reports its output path and size in the Unity log.

Standalone builds open on a keyboard-first menu. Press `Enter` to begin or continue, `N` to clear progress and start a new expedition, `S` for fullscreen and audio settings, or `Q` to quit. Exploration progress is stored as versioned JSON in Unity's platform-specific persistent data directory.

For a command-line macOS build, run Unity with:

```text
-batchmode -quit -projectPath <project> -executeMethod StrategyRPG.Editor.PrototypeBuild.BuildMacDevelopment
```

## Release candidate

1. Freeze feature work on the candidate branch.
2. Run the full applicable checklist in [Unity QA](UnityQA.md).
3. Build Windows and macOS candidates from the same commit where practical.
4. Test a clean-machine or clean-folder install, first launch, scene transitions, and exit behavior.
5. Check that no development-only paths, absolute local paths, debug cheats, or missing Resources are required.
6. Record build hashes and known limitations.
7. Tag the approved commit, create a GitHub Release, and attach compressed build artifacts.

## Build artifacts

Use platform-specific archives such as `GameFrontier-0.1.0-Windows.zip` and `GameFrontier-0.1.0-macOS.zip`. Keep the Unity project source in Git and the standalone binaries in GitHub Releases or an approved artifact store. Do not commit `Library/`, `Temp/`, `Logs/`, `obj/`, or local `UserSettings/`.

## Release notes

Each release note should include:

- Player-visible changes.
- Controls and minimum setup.
- Known issues and workarounds.
- Supported platforms.
- Build commit/tag and Unity version.
- Asset/license notices when required.

If a release is withdrawn, keep the tag and explain why. Do not silently replace a published binary under the same version.
