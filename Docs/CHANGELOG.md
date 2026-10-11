# Changelog

All notable player-facing changes are recorded here. The project follows semantic versioning once standalone builds begin.

## Unreleased

### Added

- Shared pause overlay for the flight and planet-surface scenes.
- Persistent play-session discovery and Azure World survey completion state.
- A dedicated mission-complete panel that directs the player back to the docked ship.
- One-click macOS and Windows development build commands under `Tools/Build Game Frontier`.
- A verified macOS `0.1.0` standalone development build.
- A keyboard-first start menu with New Game, Continue, Settings and Quit actions.
- Versioned local save data for discoveries and Azure World survey completion.
- Functional fullscreen and audio controls in the start-menu settings page.
- An in-flight destination log that records discoveries and Azure World survey status.
- Third-person free-flight prototype with multiple selectable destinations.
- Azure World landing and first-person island exploration.
- Structure scanning, discovery progress and mission completion feedback.
- Docked-ship return-to-orbit interaction.
- Surface jumping, shoreline treatment, animated observation water and scan markers.
- Project design, technical and production documentation system.

### Changed

- The standalone build now starts in `SpaceFlightPrototype`; inactive HexMap work is excluded from builds.
- Product metadata now identifies the application as Game Frontier `0.1.0` with bundle ID `com.gamefrontier.prototype`.
- Completed scan markers disappear and remain completed when Azure World is revisited, including after restarting the application.
- Replaced the original hex/tile-map direction with direct space-flight exploration as the active product path.
- Replaced the temporary floating Azure World platform with an irregular island blockout.
- Returning from a surface scene now restores a near-orbit destination context.

### Fixed

- Corrected imported building orientation and scale handling.
- Removed oversized building collision volumes that blocked traversal.
- Corrected island mesh winding so its upper surface renders correctly.

### Deferred

- Rogue-map and fleet-tactics prototypes remain available as inactive reference work.
- Swimming and underwater exploration are deferred until the surface loop is stable.
