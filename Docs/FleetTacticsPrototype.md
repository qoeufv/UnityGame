# Fleet tactics prototype

Scene: `Assets/Scenes/FleetTacticsPrototype.unity`.
Open from **Tools → Open Fleet Tactics Prototype**, then press Play. This independent scene is not yet connected to the universe map or Rogue exploration. The existing duel scene and its code remain available for reference.

## Play

- Select a cyan ship; blue hexes show legal movement destinations. Click an enemy in range with clear line of fire to attack (its hex is highlighted red).
- Each surviving ship receives 2 AP at the start of its side's phase. One move or attack costs 1 AP. Ships have different movement, range, damage and hull values. Players may freely switch ships and may attack twice.
- Asteroids block movement and line of fire. Friendly ships block movement but not weapons. Attacks use a seeded accuracy roll: a miss is reported as `DODGED`, while a critical hit applies the attacker's critical multiplier. The default values are 85% hit chance, 10% critical chance and 1.5× critical damage.
- The cyan relay repairs 20 hull, capped at maximum HP, once per encounter. An injured allied ship must stand on or adjacent to it and spend 1 AP using **REPAIR +20**.
- **END TURN** allows all surviving enemies to move and fire. Enemy decisions are deterministic and prefer available firing positions.
- Destroy all enemies to win; lose all allies to lose. **RESTART** starts a clean encounter. Restart is disabled during enemy actions and movement/weapon presentation.

## Architecture and scope

`FleetBattleState` owns grid/pathfinding, occupation, AP, seeded attack resolution, line of sight, repair and outcomes. The last attack exposes a `Miss`, `Hit` or `Critical` result for presentation and future AI simulation. It has no UI or scene dependencies beyond Unity coordinate types. `FleetBattlePrototype` adapts input, UI and replaceable ship visuals to those rules. A scene-referenced material keeps the prototype shader available for builds.

The scene intentionally uses simple capsule/wing silhouettes, English prototype labels, an 8 × 6 axial hex board and a fixed 3 versus 3 encounter. It does not yet include selectable factions, loadouts, skills, persistent fleet damage or Rogue rewards. Disabling and re-enabling the prototype resets the encounter; it is not a suspension/save API.

## Validation and next milestone

**Tools → Validate Fleet Tactics Rules** covers invalid and legal moves, obstacle detours, occupancy, AP, phase ownership, range and sight, repair restrictions, death, victory/defeat, and a complete automated encounter. It passed in Unity. Manual scene verification includes ship selection/movement and turn handoff; final art and balance remain unvalidated.

Three agents independently reviewed rules, reviewed technical risks, and authored rule checks. The primary agent integrated the scene, corrected findings, and performed Unity validation.

Next milestone: playtest this encounter, then connect one generated Rogue route map to configurable tactical encounters and shared fleet hull state. The earlier direct star-map-to-duel bridge is provisional and must be replaced at that integration step. Only then expand factions and polish final models/materials.
