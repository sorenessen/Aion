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
- [x] No rendering dependency exists in Aion.Simulation

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
  - [x] Add separate Aion.Persistence project
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

Earth Observatory is intended to grow into a broad real-world systems simulation, not a simulator of any single domain.

Aion should eventually represent as much of the physical, biological, environmental, economic, social, technological, and geopolitical world as can be modeled responsibly with available evidence, established domain models, transparent assumptions, and explicit uncertainty.

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

Living Worlds is the long-term game side of Aion.

Aion is not intended to prescribe one permanent game loop. The distant goal is a general simulation platform capable of supporting many different kinds of experiences within the same coherent world model.

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
