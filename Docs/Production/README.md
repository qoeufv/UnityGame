# Production Documentation

This directory defines the working agreements for Game Frontier. It is intended for a small distributed team using Unity, GitHub, and parallel AI-assisted development. The rules are deliberately lightweight: every rule exists to keep the project buildable, reviewable, and recoverable.

## Document map

| Document | Purpose | Owner |
| --- | --- | --- |
| [Branching and Git](BranchingAndGit.md) | Branch, commit, pull request, and merge rules | Engineering lead |
| [Agent Collaboration](AgentCollaboration.md) | Task decomposition, agent roles, handoffs, and integration | Project manager |
| [Unity QA](UnityQA.md) | Editor checks, Play Mode validation, regression checklist, and evidence | QA lead |
| [Release Process](ReleaseProcess.md) | Versioning, build profiles, release candidates, and GitHub Releases | Release owner |
| [Asset and License Management](AssetManagement.md) | Provenance, attribution, import rules, and large-file policy | Art/tech art lead |
| [Weekly Report Template](WeeklyReportTemplate.md) | Consistent weekly progress reporting | Project manager |

## Current project baseline

- Engine: Unity `6000.6.0f1`.
- Target: Windows and macOS desktop.
- Primary current slice: manual space flight and Azure World surface exploration.
- Prototype scenes and scripts are described in `Docs/SpaceFlightPrototype.md`, while older Rogue, fleet tactics, and tile-map prototypes remain reference material unless a milestone explicitly reactivates them.
- The repository may contain work in progress and generated Unity assets. A clean release must be built from a reviewed commit, not from an arbitrary local editor state.

## Source of truth

Code and Unity assets are authoritative in Git. Product decisions, acceptance criteria, architecture boundaries, and known limitations are authoritative in `Docs/`. A change is complete only when the implementation, relevant documentation, and validation evidence agree.

If an existing document conflicts with a newer approved decision, update the older document in the same change or mark it as historical. Do not silently maintain two contradictory rules.

## Definition of Done

A task is done when:

1. Its acceptance criteria are written and demonstrably satisfied.
2. Code/assets and any required Unity metadata are included in a reviewable branch.
3. The appropriate Unity validation and regression checks pass.
4. Documentation, screenshots, known limitations, and setup notes are updated.
5. The change is merged through a pull request and the resulting commit is identifiable.

## Change control

Large architectural, input, save-data, scene-flow, or package changes require a short design note before implementation. Small tuning changes can use the weekly report or pull request description. When uncertain, record the decision; written context is cheaper than rediscovering intent later.
