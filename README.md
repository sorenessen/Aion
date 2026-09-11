# Est

A living planetary simulation.

Est is a simulation-first platform for modeling worlds as interconnected physical, environmental, biological, societal, and eventually individual systems that evolve through time.

The project begins with a single planet viewed from space and is designed to scale in resolution toward regions, settlements, communities, and individual agents, and eventually outward toward star systems and galaxies.

## Product Directions

### Earth Observatory

A read-only representation of the real Earth grounded in historical evidence, authoritative datasets, and current real-world information.

Earth Observatory may support future-facing scenario experiments, but observed reality, model assumptions, and simulated outcomes must remain clearly distinguishable.

### Living Worlds

The game side of Est.

Players create or fork worlds, alter conditions, advance time, influence civilizations, and eventually interact with governments, settlements, and individual simulated inhabitants.

## Current Milestone

### Est 0.1 - First Light

Goal:

Create a planet. Make time move. Make the planet change. Make those changes understandable.

Initial scope:

- Headless simulation engine
- Persistent world state
- Simulation clock
- One planet
- Small set of planetary variables
- One causal simulation system
- Timeline and event history
- Save and load
- Browser-based 3D globe
- Pause and simulation-speed controls

Explicitly out of scope for First Light:

- AI agents
- Governments
- Civilizations
- Individual inhabitants
- Multiplayer
- News ingestion
- Real Earth synchronization
- Star systems
- Galaxies

## Development Environment

- .NET SDK 10.0.301
- Target framework: .NET 10
- Primary development platform: macOS Apple Silicon

## Architectural Principle

The simulation engine must remain independent of rendering.

A world must be able to exist, advance, save, load, and be tested without any graphical client.

Est should preserve architectural escape hatches. Existing implementation decisions are not sacred if they begin to materially constrain performance, maintainability, reliability, scientific integrity, or future product possibilities.

## Current Rendering Evaluation

Phase 7 is evaluating CesiumJS alongside the preserved Babylon prototype.
Cesium is a serious candidate, not a final renderer selection.

The browser evaluation now reads authoritative simulation state through
Est.Api and includes a real regional surface study derived from USGS Annual
NLCD Land Cover 2025. The validated semantic/evaluation pipeline is:

USGS categorical raster -> Est surface categories -> Est presentation
-> geographic TMS -> Cesium terrain draping.

The Olympia/Puget Sound/Mount Rainier evaluation confirms geographic
alignment, terrain relief, coastline placement, snow/ice coverage, and a
renderer-independent path from Est surface semantics to presentation assets.
It also established that direct categorical land-cover rendering is not the
intended final visual-surface strategy. Est surface semantics describe what is
at a location; a separate visual-surface composition path may combine
non-authoritative presentation inputs to determine how that location should
look. Cesium remains a consumer of those presentation assets rather than the
owner of semantic interpretation.

For local development, use Sparrow's Play Est task or run
`./scripts/dev/play.sh` from the repository root. Play starts or reuses
the API and web hosts, creates a fresh Earth session, and opens Cesium.
The individual Start API and Start Web tasks remain available for focused
development. The macOS launcher requires iTerm and the project toolchain.

See `docs/DEVELOPMENT.md` for development commands and
`docs/SURFACE_EVALUATION.md` for the data pipeline and next steps.
