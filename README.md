# Aion

A living planetary simulation.

Aion is a simulation-first platform for modeling worlds as interconnected physical, environmental, biological, societal, and eventually individual systems that evolve through time.

The project begins with a single planet viewed from space and is designed to scale in resolution toward regions, settlements, communities, and individual agents, and eventually outward toward star systems and galaxies.

## Product Directions

### Earth Observatory

A read-only representation of the real Earth grounded in historical evidence, authoritative datasets, and current real-world information.

Earth Observatory may support future-facing scenario experiments, but observed reality, model assumptions, and simulated outcomes must remain clearly distinguishable.

### Living Worlds

The game side of Aion.

Players create or fork worlds, alter conditions, advance time, influence civilizations, and eventually interact with governments, settlements, and individual simulated inhabitants.

## Current Milestone

### Aion 0.1 - First Light

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

Aion should preserve architectural escape hatches. Existing implementation decisions are not sacred if they begin to materially constrain performance, maintainability, reliability, scientific integrity, or future product possibilities.
