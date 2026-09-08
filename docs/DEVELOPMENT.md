# Est Development Guide

Canonical operational reference for local development and repository recovery. Distinguish verified commands from procedures not yet exercised on the user's machine.

## Local Environment

Primary environment: macOS Apple Silicon. Repository: `~/Projects/Est`. Solution: `Est.slnx`. Target framework: `net10.0`. Recorded SDK: `10.0.301`.

## Build and Test

From the repository root:

```bash
dotnet test Est.slnx
```

Latest verified result at this update: 222 passed, 0 failed.

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

The script captures tracked and untracked, non-ignored files, including uncommitted source and documentation, and writes `Est-recovery.zip` beside the repository. It includes `Est/RECOVERY_STATUS.txt` with branch, commit, and worktree status. It excludes Git history, ignored build output, dependency caches, IDE state, and known ignored secrets. It skips symlinks rather than following them.

Review the ZIP before sharing it. Ignore rules cannot guarantee that every untracked file is safe. The ZIP is a source snapshot, not a Git backup. Preserve the original repository. This recovery procedure was validated successfully on macOS Apple Silicon and produced an integrity-tested recovery ZIP.

## Run and Debug

Est has a headless API project and an initial browser client in
`src/Est.Web`. The browser client currently contains the first interactive
globe rendering spike and is not yet integrated with the API. The API
provides explicit world/session operations and server-owned archive storage.
The archive directory defaults to the API content root's `archives` directory
and can be configured through `Est:ArchiveDirectory`.

The API has been exercised through integration tests. A standalone local
run/debug procedure has not yet been validated on the user's machine.
Add the actual launch command, address, and manual smoke-test procedure
after validating them. Do not treat an assumed port as a verified endpoint.

## Release Procedures

Create `docs/RELEASE.md` when a real packaging and distribution workflow exists. Preserve validated versioning, packaging, signing, distribution, and release-validation procedures there.
