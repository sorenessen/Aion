# Aion Development Guide

Canonical operational reference for local development and repository recovery. Distinguish verified commands from procedures not yet exercised on the user's machine.

## Local Environment

Primary environment: macOS Apple Silicon. Repository: `~/Projects/Aion`. Solution: `Aion.slnx`. Target framework: `net10.0`. Recorded SDK: `10.0.301`.

## Build and Test

From the repository root:

```bash
dotnet test Aion.slnx
```

Latest verified result at this update: 155 passed, 0 failed.

## Repository Inspection

```bash
git status --short
git branch --show-current
git log -1 --oneline
```

Do not assume the worktree is clean or that all current work is pushed.

## Recovery ZIP

From the repository root, run:

```bash
python3 scripts/create-recovery-zip.py
```

The script captures tracked and untracked, non-ignored files, including uncommitted source and documentation, and writes `Aion-recovery.zip` beside the repository. It includes `Aion/RECOVERY_STATUS.txt` with branch, commit, and worktree status. It excludes Git history, ignored build output, dependency caches, IDE state, and known ignored secrets. It skips symlinks rather than following them.

Review the ZIP before sharing it. Ignore rules cannot guarantee that every untracked file is safe. The ZIP is a source snapshot, not a Git backup. Preserve the original repository. This recovery procedure was validated successfully on macOS Apple Silicon and produced an integrity-tested recovery ZIP.

## Run and Debug

Aion does not yet have a user-facing executable, API, or browser client. Add actual run, debug, configuration, and local-service commands when validated.

## Release Procedures

Create `docs/RELEASE.md` when a real packaging and distribution workflow exists. Preserve validated versioning, packaging, signing, distribution, and release-validation procedures there.
