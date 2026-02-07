# Git Workflow

## Branch-first rule
- Never work directly on `main`.
- Start each task on a feature branch, e.g. `agent/<topic>` or `feat/<topic>`.

## Session checklist
1. `git checkout main`
2. `git pull` (if remote exists)
3. `git checkout -b agent/<topic>`
4. Make changes and validate (`Release` + `Nomad` builds)
5. Commit with a focused message
6. Merge to `main` only after validation

## Merge reminder policy
- End each substantial task update with: `Reminder: merge <feature-branch> into main after validation.`
- If a branch is older than one session/day, rebase or merge `main` before continuing.

## Suggested branch names
- `agent/perf-<topic>`
- `feat/<topic>`
- `fix/<topic>`
