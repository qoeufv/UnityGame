# Game Frontier

Game Frontier is an early-stage 2D turn-based strategy RPG built with Unity. It combines tile-based exploration, controlled-randomness combat, regional conquest, and lightweight settlement management.

> **Current status:** Pre-production. The initial concept and MVP scope are being defined; gameplay implementation has not started.

## Overview

The player controls a character or small party while exploring a world made of diamond-shaped tiles. Moving to a new tile may reveal an encounter, resource, merchant, ruin, NPC, special location, or regional boss.

The long-term goal is to connect RPG progression with a strategic world layer: players explore and fight to conquer regions, then develop those territories to support further expeditions into more dangerous lands.

## Game Vision

Game Frontier is intended to bring together three connected experiences:

- **Explore:** Reveal a varied world one tile at a time.
- **Fight:** Make tactical decisions in turn-based battles with controlled randomness.
- **Develop:** Turn conquered regions into useful settlements, trade networks, and sources of progression.

The first prototype will validate this core loop on a deliberately small scale. It is not intended to be a full city-building simulation.

## Core Gameplay Loop

```text
Explore the world
       ↓
Trigger events and encounters
       ↓
Fight enemies and gain resources or equipment
       ↓
Defeat the regional boss
       ↓
Conquer and develop the region
       ↓
Grow the economy, population, trade, and technology
       ↓
Strengthen the player and explore more dangerous regions
```

## Main Gameplay Systems

The planned game is built around several connected systems:

- Diamond-tile world exploration and gradual map discovery
- Tile-driven encounters and events
- Turn-based combat with weighted outcomes
- Characters, equipment, skills, items, and status effects
- Regions with distinct terrain, resources, and objectives
- Territory conquest through regional bosses or objectives
- High-level settlement and resource management
- Trade routes between controlled regions
- Technology that benefits both combat and settlements

These systems describe the direction of the project, not the scope of the first playable build.

## Combat System

Combat is turn-based. During a turn, the player may choose actions such as:

- Basic Attack
- Skill
- Heal
- Defend
- Use Item
- Select Target

Actions use **controlled randomness** instead of always producing an identical result. A skill may deal normal or critical damage, apply a status effect, trigger a special effect, heal, miss, or produce a rare outcome.

An eventual **Outcome Pool** system may define weighted results for each ability. For example:

| Fireball outcome | Base chance |
| --- | ---: |
| Normal damage | 55% |
| Critical damage | 20% |
| Burning effect | 15% |
| Explosion | 8% |
| Backfire | 2% |

Character stats, equipment, terrain, technology, and active effects may later modify these weights.

Planned status effects include Burning, Poison, Bleeding, Stun, Frozen, Fear, Confusion, Regeneration, Shield, Weakness, and Vulnerable. Future iterations may also support interactions such as Wet + Lightning, Oil + Fire, Frozen + Heavy Attack, and Bleeding + Poison.

## Exploration and Map System

The world map is divided into diamond-shaped tiles. The player moves only between valid neighboring tiles, and entering a destination tile resolves its interaction.

Possible tile contents include:

- Enemy encounters
- Random events
- Resource discoveries
- Merchants
- Ruins
- NPC encounters
- Special locations
- Regional bosses
- Empty or safe locations

Unexplored tiles should reveal their contents gradually, making exploration and route choice meaningful.

## Region and Territory System

Groups of tiles form larger regions. Each region may have its own terrain, resources, enemies, events, settlements, and regional objective or boss.

Completing that objective transfers control of the region to the player and unlocks its management layer. Different environments may provide distinct resources—for example, forests may offer wood and herbs, while mountains may offer iron and stone.

## Settlement and Economy System

Conquered regions can contain settlements that the player develops at a high level. The initial system will focus on a few clear decisions rather than detailed city simulation.

Potential settlement resources include Food, Gold, Materials, Population, Production, Security, Happiness, and Technology. Potential buildings include Farms, Markets, Blacksmiths, Barracks, Libraries, Hospitals, and Trading Posts.

Later versions may allow population assignment to roles such as farmers, workers, merchants, researchers, and soldiers. Trade routes may exchange resources, encourage population growth, spread technology, and generate events such as bandit attacks, shortages, disease, or war.

## Technology and Progression

Technology is planned to connect the RPG and strategy layers. Research should improve a region while also opening new combat options or strengthening the party.

Examples:

- **Metallurgy:** Increases iron production and unlocks better weapons.
- **Medicine:** Improves population growth, reduces disease, and strengthens healing abilities.

## Initial MVP Scope

The first playable prototype is limited to:

- Unity 2D project
- One diamond/isometric-style map with approximately 20–30 tiles
- Movement between valid neighboring tiles
- Tile interaction and event system
- Simple random battle encounters
- One playable character and a few enemy types
- Turn-based combat with Attack, Skill, Heal, and Defend actions
- Randomized combat outcomes and basic status effects
- One region, one regional boss, and a conquest state
- Simple post-conquest settlement management
- Three main resources: Food, Gold, and Population
- Three initial buildings: Farm, Market, and Blacksmith

Features outside this list belong to later milestones unless they are required to validate the core loop.

## Planned Technology Stack

- **Engine:** Unity
- **Language:** C#
- **Presentation:** 2D, using a diamond/isometric-style tile map
- **Version control:** Git and GitHub
- **Game data:** Unity ScriptableObjects may be introduced for configurable content

The codebase should remain modular enough to evolve, while avoiding unnecessary abstraction during prototyping.

## Tentative Project Structure

The project structure will evolve with the prototype. A lightweight starting point may look like this:

```text
game-frontier/
├── Assets/
│   ├── Audio/
│   ├── Materials/
│   ├── Prefabs/
│   ├── Scenes/
│   ├── Scripts/
│   │   ├── Combat/
│   │   ├── Exploration/
│   │   ├── Settlement/
│   │   └── Shared/
│   ├── UI/
│   └── VFX/
├── Packages/
├── ProjectSettings/
├── .gitignore
└── README.md
```

Unity-generated directories such as `Library/`, `Temp/`, `Logs/`, `obj/`, and local `UserSettings/` should not be committed.

## Development Roadmap

1. **Foundation** — Establish the repository, core project conventions, and a test scene.
2. **Exploration prototype** — Build the tile map, neighbor validation, movement, discovery, and tile events.
3. **Combat prototype** — Add turn flow, player actions, enemies, weighted outcomes, and basic status effects.
4. **Regional objective** — Add the first region, boss encounter, victory state, and conquest transition.
5. **Settlement prototype** — Add Food, Gold, Population, and the first three buildings.
6. **Core-loop validation** — Connect exploration, combat, conquest, and settlement progression; then playtest and refine.

## Current Project Status

Game Frontier is currently in **pre-production**. This README defines the initial vision and MVP boundaries. No gameplay systems have been implemented yet, and all designs are subject to change as prototypes are tested.

## License

No license has been selected yet. Until one is added, all rights are reserved by the project owner.
