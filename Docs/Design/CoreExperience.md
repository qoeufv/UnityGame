# Core Experience

## Primary player loop

```text
Observe the star sector
    -> select a destination
    -> fly and manage approach speed
    -> receive a landing prompt
    -> land near a defined site
    -> disembark into first-person exploration
    -> survey landmarks and complete a local objective
    -> return to the docked ship
    -> resume near orbit and choose the next destination
```

The loop is intentionally short in the first playable slice. Its purpose is to validate the feeling of travel and discovery before adding economy, combat, faction simulation or large-scale procedural generation.

## State model

| Player-facing state | Required feedback | Exit condition |
| --- | --- | --- |
| Near orbit | Target name, distance, speed and landing availability | Player reaches interaction range and slows down |
| Landing approach | Clear `LAND` action and destination identity | Player confirms landing |
| Surface arrival | Environment name, mission objective and movement controls | Player starts exploring |
| Surface exploration | Progress count, scan result and visible landmarks | Local objective complete or player chooses to leave |
| Ship return | `RETURN TO ORBIT` prompt near the ship | Player interacts with the docked ship |
| Near-orbit resume | Destination context and flight controls restored | Player selects a new destination |

## Azure World reference slice

Azure World is the first reference implementation: a civilized ocean world with a small island research outpost. The island should communicate a landmass surrounded by visible water, with a landing area, research laboratory, residential habitat and communication tower as readable landmarks. The current survey objective is to scan four marked structures and report their discoveries.

The ocean is currently observational. The player can see the water and shoreline but does not need to swim or dive for the first slice. This keeps the movement and landing loop testable while leaving underwater exploration as a future feature.

## Interaction principles

- Prompts appear because of spatial context: distance, facing and movement state, rather than because the player is standing on an arbitrary trigger.
- Scanning should require the player to approach and look at a landmark, so the action rewards observation.
- The same interaction key can be reused across orbit and surface when the prompt explains the action.
- The game should never require the player to guess how to return from a surface scene.
- The HUD should support orientation and objective clarity without becoming the primary way the player experiences the world.

## Completion definition for the first loop

The slice is complete when a new player can land on Azure World, understand the island layout, walk around the buildings and ship, scan the local landmarks, see progress update, and return to orbit. Visual polish, persistent survey history and multiple destination travel are follow-up layers.

