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

### Command-block integrity

Every terminal instruction must be delivered as one complete, contiguous,
copy-pasteable code block. One command block at a time means the entire
runnable step, including heredocs and closing delimiters, must be contained
inside one rendered block.

Never place literal Markdown triple-backtick fences inside an outer command
block. When generating Markdown that contains fenced code blocks, construct
the fence at runtime (for example, `fence = chr(96) * 3`) or use another
technique that cannot terminate the outer block.

Before sending a command, verify that the entire script is contained in one
rendered block and that no embedded content can prematurely close it. Do not
split a runnable script across prose, multiple code blocks, or UI sections.
If a command cannot be safely represented as one block, use a different
delivery method rather than sending a partial script.

This is a mandatory workflow requirement, not a formatting preference.
Preserve it across handoffs and future development sessions.

## Run and Debug

Est has a headless API project and a browser client in `src/Est.Web`.
The Cesium rendering evaluation page has a verified read-only integration
with authoritative API session/world state. The API provides explicit
world/session operations and server-owned archive storage. The archive
directory defaults to the API content root's `archives` directory and can be
configured through `Est:ArchiveDirectory`.

The local development procedure was validated on macOS Apple Silicon on
September 8, 2026.

Run Est.Api in a dedicated terminal and leave that process running:

```bash
cd ~/Projects/Est
dotnet run --project src/Est.Api/Est.Api.csproj --launch-profile http
```

The verified HTTP endpoint is `http://localhost:5026`.

Run the web development host from a separate terminal:

```bash
cd ~/Projects/Est/src/Est.Web
npm run dev
```

The verified browser endpoint is `http://localhost:5173`. Vite proxies
browser requests under `/api` to Est.Api on port 5026.

Create a simulation session through `POST /sessions`, retain the returned
`sessionId`, and open:

```text
http://localhost:5173/cesium.html?session=<session-id>
```

A successful smoke test renders the Cesium globe and shows authoritative
session data in the evaluation panel, for example
`Earth · 288.15 K · t=0s`.

Simulation sessions are currently in-memory. Restarting Est.Api discards
existing sessions, so any browser URL containing an old session ID will need
a newly created session after the restart.

Keep the API host and Vite host as separate long-running processes during
manual browser testing. Reusing either host terminal for another command
terminates that foreground host unless it was deliberately started as a
detached process.

## Release Procedures

Create `docs/RELEASE.md` when a real packaging and distribution workflow exists. Preserve validated versioning, packaging, signing, distribution, and release-validation procedures there.
