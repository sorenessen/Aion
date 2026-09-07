# Aion Backlog

## Current Release

# v0.1.0 - First Light

Objective:

Build the smallest complete vertical foundation for Aion: one persistent planet, a functioning simulation clock, one causal planetary system, and a browser-visible globe.

## Phase 0 - Foundation

### Project Setup

- [x] Initialize Git repository
- [x] Pin .NET SDK 10.0.301
- [x] Create Aion.slnx
- [x] Create Aion.Simulation
- [x] Create Aion.Simulation.Tests
- [x] Add project references
- [x] Verify clean test run
- [x] Establish README
- [x] Establish project charter
- [x] Establish backlog
- [x] Establish handoff document

### Architecture Foundation

- [ ] Define simulation-domain boundaries
- [ ] Define world identity model
- [ ] Define immutable simulation-time representation
- [ ] Define simulation clock
- [ ] Define world state
- [ ] Define planet state
- [ ] Define simulation step/tick contract
- [ ] Define event/history model
- [ ] Define intervention model
- [ ] Define provenance concept without overbuilding it
- [ ] Record initial architecture decisions

## Phase 1 - Time Exists

Goal:

A world can exist and advance through simulation time without any user interface.

Acceptance criteria:

- [ ] World has stable identity
- [ ] World has current simulation time
- [ ] Time can advance deterministically
- [ ] Simulation speed is separate from simulation state
- [ ] Clock can pause
- [ ] Clock supports explicit advancement
- [ ] Tests prove deterministic advancement
- [ ] No rendering dependency exists in Aion.Simulation

## Phase 2 - First Planet

Goal:

A world owns one coherent planet state.

Initial candidate properties:

- [ ] Radius
- [ ] Surface gravity
- [ ] Mean surface temperature
- [ ] Atmospheric pressure
- [ ] Atmospheric composition
- [ ] Surface water fraction
- [ ] Ice coverage
- [ ] Habitability indicator

Acceptance criteria:

- [ ] Planet can be created from explicit initial conditions
- [ ] Planet state is serializable
- [ ] Planet state can be cloned or forked safely
- [ ] Planet state changes only through defined simulation operations

## Phase 3 - First Causal System

Goal:

Introduce one deliberately constrained planetary feedback model.

Candidate first system:

Temperature and ice-albedo feedback.

Acceptance criteria:

- [ ] System consumes defined inputs
- [ ] System produces deterministic outputs where expected
- [ ] Advancing time changes planetary state
- [ ] Cause of change can be represented in history
- [ ] Tests cover stable, warming, and cooling cases
- [ ] Model assumptions are documented
- [ ] Model is clearly identified as simplified, not a real Earth climate model

## Phase 4 - Timeline and Branching

Goal:

World history becomes inspectable and forkable.

- [ ] Append-only event history
- [ ] Simulation checkpoints
- [ ] Reset to initial state
- [ ] Fork timeline from checkpoint
- [ ] Parent and child timeline identity
- [ ] Preserve immutable history semantics

Principle:

History is immutable. Futures branch.

## Phase 5 - Persistence

Goal:

A world can leave memory and return unchanged.

- [ ] Save world
- [ ] Load world
- [ ] Version serialized state
- [ ] Detect incompatible or corrupt state
- [ ] Round-trip tests
- [ ] Preserve timeline and history
- [ ] Preserve provenance metadata

## Phase 6 - Aion API

Goal:

Expose simulation operations without coupling the engine to presentation.

- [ ] Create Aion.Api
- [ ] World creation endpoint
- [ ] World-state endpoint
- [ ] Advance-time endpoint
- [ ] Pause and resume semantics
- [ ] Intervention endpoint
- [ ] Timeline endpoint
- [ ] Persistence boundary
- [ ] API integration tests

## Phase 7 - First Globe

Goal:

See Aion.

Candidate client:

TypeScript web application with Babylon.js or another browser-native renderer selected after a focused technical spike.

- [ ] Create Aion.Web
- [ ] Render large 3D planet
- [ ] Orbit camera
- [ ] Zoom controls
- [ ] Basic lighting
- [ ] Basic generated or placeholder surface
- [ ] Fetch authoritative world state from API
- [ ] Display core planetary values
- [ ] Pause
- [ ] 1x
- [ ] 10x
- [ ] 100x
- [ ] 1000x
- [ ] Visually reflect first causal-system changes

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

- Real Earth canonical world
- Historical snapshots
- Data provenance
- Source attribution
- Current-condition ingestion
- News and event interpretation pipeline
- Scenario experiments
- Uncertainty representation
- Model and version attribution
- Observed versus estimated versus simulated state

## Living Worlds

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
- Emergent events
- Player interventions

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
