# Character data and animation integration

Scene layout, cameras, anchors, Animator assets and existing prefabs are unchanged.

## Ownership

- `CharacterData`: shared authoring asset with Attack, Defense, HP, Dodge, Speed and CritRate (default 0.03), starting equipment, learned skills and an optional future battle prefab reference.
- `EquipmentData`: one of Helmet, Bracer, ChestArmor, Weapon, Ring or Necklace, additive stat bonuses and optional visual prefab. Replacing a slot replaces its bonus. Missing weapon defaults to Fist. Weapon enum values are preserved from existing animations (None is the unrestricted requirement sentinel).
- `SkillData`: MartialArt, InnerSkill or Qinggong; weapon requirement, damage multiplier, accuracy, status definitions, trigger, optional VFX and impact timing. Former code references to MartialArts now use MartialArt, retaining serialized value 0.
- `CombatantStats`: per-battle HP, equipment selection, computed stats and status durations. Shared assets never hold current HP or remaining turns. Equipment changes clamp current HP to the new maximum but do not heal or resurrect. Duplicate starting slots are rejected.
- `SkillResolver`: deterministic gameplay resolution. Caller supplies two rolls in [0, 1). Hit chance = accuracy × (1 − target dodge). Damage = round(max(0, attack × multiplier − defense) × crit multiplier). Critical multiplier is 2. Zero-damage hits can apply statuses; misses and defeated targets cannot.
- `BattleCharacterController`: presentation facade around the existing `BattleCharacterVisual`, plus a single-impact clock. It does not validate equipment, spend turns or apply damage.
- `BattleManager`: validates weapon requirements before spending the turn; subscribes before playback; resolves gameplay when impact arrives; updates reactions, VFX, UI and turn order afterward.

Speed is available to a future initiative system; the current player-first turn order is preserved. Character battle prefab, learned skills and non-weapon equipment visuals are authoring hooks, not automatic scene replacement, a skill-selection UI or an armor attachment system. Existing prototype buttons retain their balance and behavior unless a matching Default Skill is assigned; basic attack crit chance now uses the combatant's 3% base rate. The new resolver governs the data-driven skill path.

## Configure without changing the layout

1. Create assets with **Assets → Create → Strategy RPG → Character / Equipment / Skill**. Assign equipment and skills to each character asset. Percentages use fractions: 3% is 0.03; equipment CritRate +0.02 adds two percentage points.
2. On the existing BattleManager, optionally assign Player Data and Enemy Data. Leave them empty to retain the prototype HP defaults. Assign **FlameSunPalm** to Default Skill to route MartialArt buttons through authored resolution, or call `PlayerSkill(skill)` from your skill-selection UI. These API calls intentionally do not enforce a learned-skill inventory yet.
3. Existing BattleArenaController adds/reuses a BattleCharacterController on each BattleCharacterVisual at runtime. For standalone character prefabs, add the controller yourself and assign the existing visual. Keep its Animator reference pointed at your animated model.
4. Existing Animator parameters remain supported: Attack, Skill, Hit, Die and Reset triggers; Defend bool; SkillType and WeaponType ints. Flame Sun Palm uses `Skill` and MartialArt (0) so the existing Animator works. For a unique animation, add a trigger/transition in your Animator and enter its exact name in the skill asset.
5. Optional VFX is spawned at the target's existing effect root on a successful impact. Give the VFX prefab its own lifetime/despawn behavior. Its reference can remain empty.

## Impact timing

`PlayBasicAttack()`, `PlaySkill(SkillData)`, `PlayDefend()`, `PlayHit()`, `PlayDeath()` and `PlayIdle()` are presentation calls. Attack/skill playback raises `Impact(actionId, skill)` once; a basic attack supplies null skill. Subscribe before starting playback. A zero-second timed impact arrives on the next controller Update, allowing callers to capture CurrentActionId after starting.

- **Timed:** Impact Time is scaled seconds from PlaySkill, independent of clip normalized time/Animator speed. Adjust it if you change playback speed.
- **AnimationEvent:** add an Animation Event named `OnSkillImpact` at the strike frame on a base-layer clip. The controller installs a relay on the Animator GameObject, so nested model Animators work. Place the event after the entry transition and before the exit transition; events during transitions or from a different state are ignored. Reactions, death, idle, disabling or starting another action invalidate the old action. Custom integrations can call `NotifyAnimationImpact(capturedActionId)` to reject stale callbacks.
- This is a single-impact action protocol, not a multi-hit/combo scheduler. Finish/cancel an action before reusing its clip. Do not put impact events on idle/hit/death clips. BattleManager cancels an event-driven action with no callback after Animation Event Timeout (default 5 seconds), without applying damage, then advances the turn. Increase this for longer clips.

Flame Sun Palm: MartialArt, Fist, ×1.3 damage, 0.9 accuracy, Burning for 3 turns at 1% target max HP per turn. Burning ticks at the end of the affected combatant's turn, rounds up with a minimum of 1, and refreshes rather than stacks. Call `EndTurn()` exactly once per completed turn in other battle integrations.

## Validation

Run **Tools → Validate Battle Data and Timing** (Cmd+Option+B on macOS), in Edit Mode. The checks load the real Flame Sun Palm asset and use temporary data objects; they do not change the scene. Coverage includes equipment replacement, asset immutability, default crit, hit/miss/crit damage, weapon rejection, Burning ticks/expiry/refresh/reset, timed and event impacts, duplicate callbacks and cancellation.

For the existing prototype scene, enter Play Mode from the map and run **Tools → Validate Battle Skill Playback** (Cmd+Option+N). It starts a temporary battle, verifies the real Animator state and delayed damage/Burning, and logs its result. Exit Play Mode afterward to discard the test battle. It assumes the default Fist runtime combatant and the existing prototype Animator.
