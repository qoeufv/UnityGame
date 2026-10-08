# Changelog

All notable player-facing changes are recorded here. The project follows semantic versioning once standalone builds begin.

## Unreleased

### Added

- Third-person free-flight prototype with multiple selectable destinations.
- Azure World landing and first-person island exploration.
- Structure scanning, discovery progress and mission completion feedback.
- Docked-ship return-to-orbit interaction.
- Surface jumping, shoreline treatment, animated observation water and scan markers.
- Project design, technical and production documentation system.

### Changed

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

