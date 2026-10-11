# Game Frontier Documentation

This directory is the project knowledge base for Game Frontier. It records product intent, technical decisions, production practice and weekly progress so the project can be reviewed without relying on conversation history.

## Documentation areas

| Area | Purpose | Entry point |
| --- | --- | --- |
| Design | Product vision, player experience, scope, roadmap and planet-content principles | [Design README](Design/README.md) |
| Technical | Runtime architecture, scene/data flow, asset pipeline and technical debt | [Technical README](Technical/README.md) |
| Production | Git workflow, agent collaboration, QA, releases, asset records and weekly reports | [Production README](Production/README.md) |
| Progress | Week-by-week implementation history | [Week 1](Week1Progress.md), [Week 2](Week2Progress.md), [Week 3](Week3Progress.md), [Week 4](Week4Progress.md) |
| Changes | Player-facing additions, changes, fixes and deferred work | [Changelog](CHANGELOG.md) |

## Current project baseline

- Unity `6000.6.0f1`
- Windows and macOS desktop target
- Current playable focus: third-person space flight, Azure World landing and first-person island exploration
- Current prototype loop: approach planet → land → walk and scan structures → return to near orbit

## Documentation rules

Every meaningful system change should update the relevant design or technical document. Every weekly delivery should use the production report template and link to the commit that contains the work. Architectural changes should record the decision, its reason and its impact on future work.

The documentation describes the intended product and current implementation separately. Prototype limitations must remain explicit until the implementation is replaced or accepted as production behavior.
