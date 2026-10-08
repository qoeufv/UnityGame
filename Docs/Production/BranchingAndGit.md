# Branching and Git Workflow

## Repository rules

- Keep `main` releasable. Do not develop directly on it.
- Create one short-lived branch per coherent task. Use `codex/<area>-<short-name>` for agent work, for example `codex/surface-return-prompt`.
- A branch should have one purpose. Split unrelated fixes into separate branches so a review can be accepted or reverted independently.
- Never use `git reset --hard`, mass deletion, or force push to hide unrelated local work. If the worktree contains changes from another task, isolate the intended files and report the remaining state.
- Commit Unity `.meta` files together with their asset or folder. Never hand-edit generated metadata unless the task specifically requires it.

## Commit format

Use the Conventional Commits style:

```text
<type>(<scope>): <imperative summary>
```

Allowed types: `feat`, `fix`, `refactor`, `docs`, `test`, `art`, `build`, `chore`, `perf`.

Examples:

```text
feat(surface): add docked ship return interaction
fix(flight): preserve selected planet on orbit return
docs(production): add Unity regression checklist
```

Keep the subject under about 72 characters. Explain the reason and validation in the body when the change is non-trivial. Do not mix a report update with an unrelated code or asset change unless the report is documenting that exact change.

## Pull requests

Every merge to `main` should have a pull request, even for a solo project. The PR should include:

- Problem and resulting player-visible behavior.
- Scope and files/systems changed.
- Acceptance criteria and test evidence.
- Screenshots or a short capture for visual changes.
- Known limitations and follow-up work.
- Asset provenance or license notes when assets changed.

The author checks their own diff, runs the applicable validation, and resolves all review comments. A second agent or the manager reviews architecture, unintended scope, and documentation for changes that affect shared systems.

## Review and merge

Before merge:

1. Rebase or merge the current `main` into the branch if it has diverged materially.
2. Confirm Unity version and package state are unchanged or documented.
3. Run the Unity QA checks in [Unity QA](UnityQA.md).
4. Review `git diff --stat` and `git status` for accidental generated files, large binaries, or deletions.
5. Squash only when it preserves useful history; otherwise retain logical commits.

After merge, record the merge commit or release tag in the weekly report. Push tags only after the corresponding build has been verified.

## Handling the current worktree

The project has historically accumulated large asset imports and unrelated local changes. Treat those as separate work until explicitly reviewed. Before staging, use `git status --short`, `git diff --name-status`, and `git diff --stat`; stage paths deliberately rather than using `git add .`.
