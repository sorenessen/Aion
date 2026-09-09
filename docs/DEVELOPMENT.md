# Est Development Guide

Canonical operational reference for local development and repository recovery. Distinguish verified commands from procedures not yet exercised on the user's machine.

## Local Environment

Primary environment: macOS Apple Silicon. Repository: `~/Projects/Est`. Solution: `Est.slnx`. Target framework: `net10.0`. Recorded SDK: `10.0.301`.

## Build and Test

From the repository root:

```bash
dotnet test Est.slnx
```

Latest verified result: September 8, 2026, `dotnet test Est.slnx`: 222 passed, 0 failed, 0 skipped. The web production build also passed with `npm run build`.

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
The Cesium evaluation reads authoritative API session/world state. The API
provides explicit world/session operations and server-owned archive storage.
The archive directory defaults to the API content root's `archives` directory
and can be configured through `Est:ArchiveDirectory`.

### Normal startup

The macOS launcher was validated on September 8, 2026, including a true
cold start from Sparrow and subsequent reuse of both running services.

From Sparrow, open the Est workspace and select **Play Est**. Alternatively,
run the same launcher from a regular development terminal:

```bash
cd ~/Projects/Est
./scripts/dev/play.sh
```

Play starts or reuses Est.Api and Est.Web, waits for their health checks,
creates a fresh Earth session through `POST /sessions`, and opens Cesium
with the returned session ID. No manual API -> Web -> Play sequence is
required.

The Sparrow workspace configuration is `sparrow.toml`:

- **Play Est**: normal startup and fresh Earth session.
- **Start API**: start or reuse the API independently.
- **Start Web**: start or reuse the web renderer independently.

The launcher is currently macOS-specific and uses iTerm to open independent
service windows. It requires the .NET SDK, Node.js/npm, Python 3, curl,
lsof, zsh, and iTerm. The launcher preserves the invoking shell's PATH
when starting service windows. The browser requires the existing local
Cesium configuration, including the ion token where applicable.

The API listens on port 5026 and Vite on port 5173. The API health contract
is `GET /health`, returning HTTP 200 with service `Est.Api` and status
`healthy`. The web health check requests the Cesium HTML page using
`localhost`, which supports the observed IPv6-only Vite listener.

The scripts inspect existing listeners and verify project ownership before
reuse. They do not automatically kill or restart occupied ports. An
unrecognized or unhealthy listener produces a diagnostic and requires
manual investigation. The API script also preserves a recognized older
Est.Api host that predates `/health`, rather than discarding its sessions.

### Manual development

The individual scripts can be run from the repository root:

```bash
./scripts/dev/api.sh
./scripts/dev/web.sh
```

For direct debugging without the launcher, run Est.Api in a dedicated
terminal:

```bash
cd ~/Projects/Est
dotnet run --project src/Est.Api/Est.Api.csproj --launch-profile http
```

Run the web development host from a separate terminal:

```bash
cd ~/Projects/Est/src/Est.Web
npm run dev
```

The verified API endpoint is `http://localhost:5026`. The verified browser
endpoint is `http://localhost:5173`. Vite proxies browser requests under
`/api` to Est.Api on port 5026.

Create a simulation session through `POST /sessions`, retain the returned
`sessionId`, and open:

```text
http://localhost:5173/cesium.html?session=<session-id>
```

A successful smoke test renders the Cesium globe and shows authoritative
session data, for example `Earth · 288.15 K · t=0s`.

### Process and session safety

Simulation sessions are currently in-memory. Restarting Est.Api discards
existing sessions, so browser URLs containing old session IDs need a newly
created session after the restart. Play intentionally creates a fresh
session each time; it does not resume an existing session.

Keep API and Vite in separate long-running terminals. Stop a service
deliberately with Ctrl+C in its own window when necessary. Do not kill
unrelated listeners or restart the API merely to change browser rendering.
A listening port alone does not prove that a service is healthy.

## Release Procedures

Create `docs/RELEASE.md` when a real packaging and distribution workflow exists. Preserve validated versioning, packaging, signing, distribution, and release-validation procedures there.
