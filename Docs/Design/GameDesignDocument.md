# Game Design Document

**Project:** Game Frontier  
**Status:** Living design baseline  
**Target:** Windows and macOS desktop  
**Engine:** Unity 6000.6.0f1

## High concept

Game Frontier is a science-fiction exploration game in which the player pilots a ship through a connected universe, approaches unfamiliar planets, lands at local sites, and investigates environments, structures and signs of civilization. The intended experience combines the scale of space travel with grounded first-person discovery.

## Player fantasy

The player is an independent explorer operating a personal spacecraft. They should feel that every destination is physically reachable, visually distinct and capable of revealing new information about the universe.

## Core loop

1. Review destinations and select a planet or point of interest.
2. Fly through local space and approach the target.
3. Meet the target's landing or interaction conditions.
4. Land and enter a first-person exploration area.
5. Discover structures, environmental clues and mission objectives.
6. Complete the local survey and return to the ship.
7. Return to orbit with discoveries and progression preserved.

## Current playable slice

The current vertical slice contains third-person free flight, destination selection, Azure World landing, first-person island traversal, structure scanning and return to near orbit. It is a prototype used to validate movement, scale, scene transitions and the exploration loop.

## Player controls

### Space flight

- Thrust, reverse, turn, pitch and roll.
- Boost and brake.
- Orbiting follow camera and zoom.
- Destination selection and contextual landing/inspection.

### Surface exploration

- First-person WASD movement and mouse look.
- Jump.
- Contextual structure scanning.
- Return to orbit from the docked ship or emergency shortcut.

## Progression direction

Progression should come from discoveries, completed surveys, access to new systems, ship capabilities and relationships with civilizations or factions. Combat, economy and survival systems remain optional layers until exploration proves enjoyable by itself.

## Content model

Each explorable planet should define:

- A strong environmental identity.
- An orbital silhouette and approach experience.
- At least one landing site.
- A readable local objective.
- Several points of interest.
- A discovery, mystery or decision that justifies visiting.
- A clear return path to the ship.

## Success and failure

The current prototype does not use a permanent failure state. A local mission succeeds when its required discoveries are completed and uploaded. Future hazards may create temporary failure, resource loss or forced extraction, but should not interrupt the exploration fantasy without clear warning.

## UI principles

- Show only information needed for the current navigation or exploration decision.
- Use consistent verbs: Approach, Land, Scan, Return.
- Keep mission progress visible without covering the environment.
- Separate prototype/debug information from final player-facing HUD elements.

## Audio direction

Space should feel quiet and mechanical, with ship feedback carrying most of the soundscape. Surface areas should use environmental layers, distant machinery and location-specific ambience. Audio implementation is planned after movement and visual scale are stable.

## Explicit non-goals for the current slice

- Seamless planet-to-surface streaming.
- Large procedural galaxies.
- Tactical combat.
- Full civilization simulation.
- Multiplayer networking.
- Final economy, crafting or survival systems.

## Acceptance criteria for the first vertical slice

- A new player can identify a planet, approach it, land, explore and return to orbit without developer explanation.
- Movement and camera behavior are stable on Windows and macOS.
- Azure World clearly reads as an island surrounded by ocean.
- Buildings have understandable scale and interaction markers.
- Mission completion and return-to-orbit feedback are visible.
- The complete loop can be built as a standalone executable.

