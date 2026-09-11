# Est Backlog

## Current Release

# v0.1.0 - First Light

Objective:

Build the smallest complete vertical foundation for Est: one persistent planet, a functioning simulation clock, one causal planetary system, and a browser-visible globe.

## Phase 0 - Foundation

### Project Setup

- [x] Initialize Git repository
- [x] Pin .NET SDK 10.0.301
- [x] Create Est.slnx
- [x] Create Est.Simulation
- [x] Create Est.Simulation.Tests
- [x] Add project references
- [x] Verify clean test run
- [x] Establish README
- [x] Establish project charter
- [x] Establish backlog
- [x] Establish handoff document

### Architecture Foundation

- [ ] Define simulation-domain boundaries
- [x] Define world identity model
- [x] Define immutable simulation-time representation
- [x] Define simulation clock
- [x] Define world state
- [x] Define planet state
- [x] Define simulation step/tick contract
- [ ] Define event/history model
- [ ] Define intervention model
- [ ] Define provenance concept without overbuilding it
- [ ] Record initial architecture decisions

## Phase 1 - Time Exists

Goal:

A world can exist and advance through simulation time without any user interface.

Acceptance criteria:

- [x] World has stable identity
- [x] World has current simulation time
- [x] Time can advance deterministically
- [x] Simulation speed is separate from simulation state
- [x] Clock can pause
- [x] Clock supports explicit advancement
- [x] Tests prove deterministic advancement
- [x] No rendering dependency exists in Est.Simulation

## Phase 2 - First Planet

Goal:

A world owns one coherent planet state.

Initial candidate properties:

- [x] Radius
- [x] Surface gravity
- [x] Mean surface temperature
- [x] Atmospheric pressure
- [x] Atmospheric composition
- [x] Surface water fraction
- [x] Ice coverage
- [x] Habitability assessment foundation
  - [x] Define profile-specific assessment contract
  - [x] Preserve uncertainty and missing-factor reporting
  - [x] Establish Earth-like surface life reference profile
  - [x] Avoid universal uninhabitability claims from limited surface data
  - [ ] Implement scientifically calibrated biological profiles when supporting environmental models are available

Acceptance criteria:

- [x] Planet can be created from explicit initial conditions
- [x] Planet state is serializable
  - [x] Add separate Est.Persistence project
  - [x] Implement versioned JSON snapshot DTOs
  - [x] Reconstruct through validating domain constructors
  - [x] Verify basic round trip and invalid-domain rejection
  - [x] Complete malformed-snapshot and multi-planet coverage
  - [x] Establish serialization checkpoint
- [x] Planet state can be cloned or forked safely
  - [x] Copy preserves world identity and state
  - [x] Simple fork creates a new world identity
  - [x] Fork preserves planet identities and starting state
  - [x] Immutable operations allow independent divergence
  - [ ] Historical timeline branching remains Phase 4
- [x] Establish defined simulation-operation boundary
  - [x] Define ISimulationOperation and SimulationOperationExecutor
  - [x] Implement time advancement and planet-environment replacement operations
  - [x] Route SimulationClock through the operation boundary
  - [x] Verify immutable state transitions and target identity preservation
  - [ ] Revisit enforcement when causal systems require stronger execution guarantees

## Phase 3 - First Causal System

Goal:

Introduce one deliberately constrained planetary feedback model.

Candidate first system:

Temperature and ice-albedo feedback.

Acceptance criteria:

- [x] System consumes defined inputs
- [x] System produces deterministic outputs where expected
- [x] Advancing time changes planetary state
- [x] Cause of change can be represented in history
- [x] Tests cover stable, warming, and cooling cases
- [x] Model assumptions are documented
- [x] Model is clearly identified as simplified, not a real Earth climate model

## Phase 4 - Timeline and Branching

Goal:

World history becomes inspectable and forkable.

- [x] Append-only event history
- [x] Simulation checkpoints
- [x] Reset to initial state
- [x] Fork timeline from checkpoint
- [x] Parent and child timeline identity
- [x] Preserve immutable history semantics

Principle:

History is immutable. Futures branch.

## Phase 5 - Persistence

Goal:

A world can leave memory and return unchanged.

- [x] Save world
- [x] Load world
- [x] Version serialized state
- [x] Detect incompatible or corrupt state
- [x] Round-trip tests
- [x] Preserve timeline and history
- [x] Preserve provenance metadata

## Phase 6 - Est API

Goal:

Expose simulation operations without coupling the engine to presentation.

- [x] Create Est.Api
- [x] World creation endpoint
- [x] World-state endpoint
- [x] Advance-time endpoint
- [x] Pause and resume semantics
- [x] Intervention endpoint
- [x] Timeline endpoint
- [x] Persistence boundary
- [x] API integration tests

Acceptance: 202 tests passing. API creation, inspection, advancement,
intervention, and archive round trips are verified.

## Phase 7 - First Globe

Goal:

See Est.

Candidate client:

TypeScript web application with Babylon.js or another browser-native renderer selected after a focused technical spike.

- [x] Create Est.Web
- [x] Render large 3D planet
- [x] Orbit camera
- [x] Zoom controls
- [ ] Basic lighting
- [ ] Basic generated or placeholder surface
- [x] Fetch authoritative world state from API
- [x] Display core planetary values
- [ ] Pause
- [ ] 1x
- [ ] 10x
- [ ] 100x
- [ ] 1000x
- [ ] Visually reflect first causal-system changes

### Local development launcher

- [x] Add a Sparrow workspace with Play Est as the primary action.
- [x] Preserve independent Start API and Start Web development tasks.
- [x] Launch API and Web in separate iTerm windows and wait for health.
- [x] Reuse healthy Est-owned listeners without duplicate service windows.
- [x] Refuse automatic termination or restart of occupied ports.
- [x] Create a fresh Earth session and open Cesium from Play.
- [x] Validate cold-start Play from Sparrow and subsequent service reuse.
- [ ] Consider portable terminal integration if Est development expands beyond macOS.
- [ ] Revisit durable session selection/resumption when the product requires it.

## Phase 8 - First Intervention

Goal:

The user changes one thing and sees understandable consequences.

- [ ] Select one safe prototype intervention
- [ ] Apply intervention through simulation domain
- [ ] Record intervention in history
- [ ] Run simulation forward
- [ ] Compare before and after state
- [ ] Explain causal chain at prototype level

# Future Milestones

## Earth Observatory

Earth Observatory is intended to grow into a broad real-world systems simulation, not a simulator of any single domain.

Est should eventually represent as much of the physical, biological, environmental, economic, social, technological, and geopolitical world as can be modeled responsibly with available evidence, established domain models, transparent assumptions, and explicit uncertainty.

Candidate domains include, but are not limited to:

- atmosphere, weather, and climate
- oceans, hydrology, and cryosphere
- geology and natural hazards
- ecosystems and biodiversity
- agriculture and food systems
- natural resources and energy
- populations and demographics
- public health
- migration
- infrastructure and transportation
- housing and land use
- economies and macroeconomic indicators
- markets, commodities, prices, and supply chains
- finance, currencies, and trade
- industries, firms, labor, and employment
- governments, institutions, law, and public policy
- international relations and conflict
- science and technology
- education and human development
- culture and social change
- disasters and major real-world events
- interactions and causal effects among these systems

This breadth is a long-term design target, not a requirement to implement all domains at once.

- Real Earth canonical world
- Historical snapshots
- Data provenance
- Source attribution
- Current-condition ingestion
- News and event interpretation pipeline
- Scenario experiments
- Cross-domain causal simulation
- Economic, market, and commodity projections
- Uncertainty representation
- Model and version attribution
- Observed versus estimated versus simulated state

## Living Worlds

Living Worlds is the long-term game side of Est.

Est is not intended to prescribe one permanent game loop. The distant goal is a general simulation platform capable of supporting many different kinds of experiences within the same coherent world model.

A player may eventually choose to observe, manage, influence, inhabit, or directly participate in the simulation at different scales. Possible experiences may include planetary stewardship, ecosystem management, civilization building, economics, markets, government, warfare, exploration, business, city building, family or individual life, scientific experimentation, alternate history, spaceflight, or other forms of play that emerge from the simulated systems.

Different experiences should share authoritative underlying world state rather than becoming disconnected games with incompatible versions of reality.

This is a design horizon, not a requirement to build every possible game mode now.

- Procedural planets
- Biomes
- Resources
- Biosphere
- Populations
- Civilizations
- Technology
- Governments
- Culture
- Trade
- Conflict
- Migration
- Economies
- Markets
- Organizations and firms
- Households
- Persistent individual agents
- Exploration
- Spaceflight
- Emergent events
- Player interventions
- Multiple styles of play over shared simulation state

## Increased Resolution

- Regions
- Settlements
- Cities
- Communities
- Buildings
- Persistent individual agents
- Ground-level visualization
- Aggregate-to-individual reconciliation

## Online and Hosted Worlds

- Accounts
- Cloud persistence
- Hosted simulation workers
- Browser access
- Efficient catch-up simulation
- World permissions
- Public and private worlds
- Visiting worlds
- Shared interactions

## Beyond One Planet

- Moons
- Orbital relationships
- Star-system simulation
- Interplanetary travel
- Terraforming
- Colonization
- Known-system templates
- Procedural systems
- Galaxies

# Explicitly Deferred Ideas

These are intentionally not rejected. They are simply not allowed to distort First Light.

- LLM-driven civilizations
- Individual AI inhabitants
- Real-time multiplayer
- MMO architecture
- Photorealistic rendering
- Full physical climate modeling
- Full economic modeling
- Every person on Earth as an agent
- Detailed spacecraft
- Interstellar travel
- Galaxy generation
- Cross-universe travel

### Olympia land-cover vertical slice

- [x] Retrieve the 2025 Annual NLCD GeoTIFF from MRLC request `5a3e2c72-778a-4f3b-9c57-be4e7b1fef82`.
- [x] Inspect CRS, bounds, resolution, NoData, categorical values, legend, and source provenance.
- [x] Build a reproducible regional conversion pipeline without committing large raw datasets by default.
- [x] Define Est-owned surface semantics independently of NLCD codes and Cesium materials.
- [x] Render a coherent Olympia/Puget Sound/Mount Rainier slice using real terrain and land-cover semantics.
- [x] Verify geographic alignment, coastline placement, terrain relief, and snow/ice coverage through close-range browser inspection.
- [x] Preserve the working satellite baseline, terrain study, and USGS WMS comparison; keep renderer selection open.
- [ ] Evaluate full-resolution regional rendering and an appropriate level-of-detail strategy.
- [ ] Improve regional coverage boundaries and fallback surface treatment.
- [ ] Develop Est-owned natural surface materials without treating the diagnostic palette as final art direction.
- [ ] Evaluate water treatment, close-range quality, performance, and transitions at multiple scales.

## Regional surface TMS follow-up

- [x] Generate and validate a full-resolution regional geographic TMS pyramid.
- [x] Integrate the pyramid as a fifth Cesium comparison mode.
- [x] Confirm improved close-range lake/shoreline detail in the browser.
- [x] Fix launcher ownership detection for differently capitalized macOS paths.
- [x] Add renderer-only daylight/real-lighting evaluation control.
- [x] Preserve camera position during surface-mode A/B switching.
- [x] Validate coverage edges, transparency, seams, and fallback behavior.
- [ ] Evaluate natural surface materials and close-range performance.
- [ ] Harden and test TMS publication rollback behavior.
