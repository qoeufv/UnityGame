# Agent Collaboration Workflow

## Roles

The manager owns priority, scope, integration order, and the final acceptance decision. Agents are assigned one responsibility at a time:

- **Design/progression:** player goals, flow, rules, and acceptance criteria.
- **Architecture/engineering:** system boundaries, APIs, data ownership, and implementation plan.
- **Environment/art direction:** visual language, composition, asset requirements, and performance budgets.
- **Implementation:** code or Unity asset changes within an agreed scope.
- **QA/validation:** reproducible checks, regression coverage, and evidence.
- **Documentation/release:** update the relevant design note, weekly report, and release notes.

One person or agent may fill multiple roles on a small task, but the responsibilities must still be explicit.

## Task packet

Every delegated task starts with a short packet:

```text
Goal:
Player-visible result:
In scope:
Out of scope:
Files or scene boundaries:
Acceptance criteria:
Validation expected:
Dependencies:
```

The agent returns changed files, decisions, validation performed, known limitations, and any work it deliberately did not touch. An agent must not silently change a shared scene, input scheme, save format, or package dependency outside its packet.

## Parallel work

Parallelize independent analysis and asset preparation. Serialize work that touches the same scene, public API, input map, save data, or generated runtime hierarchy. When two agents need the same file, the manager chooses an owner before edits begin.

Recommended sequence:

1. Design agent writes the acceptance criteria.
2. Architecture agent defines boundaries and risks.
3. Art/asset agent prepares references and provenance.
4. Implementation agent edits a focused branch.
5. QA agent validates the integrated result.
6. Documentation agent records behavior and limitations.

## Handoff protocol

An agent handoff must contain:

- Branch and commit, or explicit statement that the work is analysis only.
- Exact files changed.
- What was tested and how.
- Screenshots/logs when visual or runtime behavior matters.
- Remaining risks and the next recommended action.

The manager integrates only after checking the diff and resolving conflicts. If a task reveals a new design question, pause implementation at the boundary and write a decision note rather than letting agents invent incompatible behavior.

## Decision records

Record decisions that affect multiple systems: scene transitions, player state persistence, input ownership, data formats, asset budgets, and platform support. A decision record should state context, decision, alternatives considered, and consequences. Link it from the affected design or technical document.
