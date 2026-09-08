# Aion Project Charter

## Vision

Aion is a living planetary simulation.

Aion models worlds as interconnected physical, environmental, biological, societal, and eventually individual systems that change through time.

The experience begins with a planet viewed from space. The user can observe its state, move through time, inspect causes and effects, and, in simulated worlds, intervene.

Aion is designed to grow in resolution rather than simply grow in size.

The long-term scale includes planets, regions, settlements, communities, and individual inhabitants. Much later, Aion may expand outward to star systems and galaxies.

The first implementation does not need those future scales, but foundational architecture should avoid preventing them.

## Earth Observatory

Earth Observatory represents the real Earth.

Historical state should be derived from historical evidence and authoritative datasets. Contemporary state should be updated from trustworthy real-world data sources and carefully processed current events.

Recorded reality must never silently become generated or simulated information.

Aion should distinguish among:

- observed measurements
- historical records
- estimates and reconstructed data
- current events
- model assumptions
- simulated projections

The canonical Earth is read-only.

Users may create scenario experiments from known Earth states.

Examples include changes to emissions, atmospheric composition, energy production, population, technology, and natural or cosmic events.

Scenario results are modeled possibilities, not claims of certain prediction.

The long-term goal is to support thoughtful experiments about what Earth could plausibly look like 10, 50, or 100 years into the future.

## Living Worlds

Living Worlds is Aion as a game.

Worlds may be procedurally generated or created from templates, including historical Earth states.

Unlike Earth Observatory, these worlds are editable.

Players may eventually alter planetary conditions, resources, ecosystems, civilizations, technologies, governments, events, and individual inhabitants.

The long-term objective is emergent history.

The player creates conditions.

The simulation creates consequences.

## Shared Principle

Earth Observatory and Living Worlds may share simulation components, visualization systems, physical models, and data structures.

They do not share epistemic status.

Observed, estimated, modeled, simulated, and generated information must remain distinguishable.

## Time

Time is a first-class component of Aion.

Worlds have persistent chronology.

Simulation may be paused or accelerated.

World state may be checkpointed.

A state may be forked into a new timeline without changing its parent.

History is immutable. Futures branch.

## Simulation Scale

Aion does not attempt to simulate every entity at maximum resolution simultaneously.

Simulation resolution changes with observational scale.

At planetary scale, populations may be aggregates.

At regional scale, they may become settlements and demographic groups.

At sufficiently close scale, important or representative inhabitants may become persistent individual agents.

Higher-resolution state must remain consistent with lower-resolution aggregate state.

## Scientific Integrity

Earth Observatory is intended to become a broad simulation of real-world systems rather than a model confined to one scientific or human domain.

Its long-term scope may include physical, biological, environmental, economic, social, technological, and geopolitical systems wherever they can be represented responsibly.

Aion should preserve the ability to model causal relationships across those domains, including effects involving resources, populations, economies, markets, commodities, trade, governments, technology, ecosystems, climate, and other measurable systems.

Earth simulation should prefer established domain models, authoritative datasets, and transparent assumptions over invented equations.

Aion should represent uncertainty, disagreement, missing knowledge, and model limitations explicitly rather than manufacture false precision.

Artificial intelligence is not the authority governing physical reality.

AI may eventually assist with agent decision-making, interpretation, narration, explanation, culture generation, diplomacy, dialogue, and summarization.

Deterministic or probabilistic simulation systems establish world state.

## Online Future

A world is a persistent data object independent of the device displaying it.

Initially, worlds may run locally.

Eventually, Aion may host worlds so users can sign in through a browser and continue observing or interacting with them from anywhere.

Worlds need not consume dedicated computing resources continuously. Checkpoints, elapsed simulation time, scheduled advancement, and event processing may be used to maintain persistent hosted worlds efficiently.

World owners may eventually allow others to observe, visit, or interact according to explicit permissions.

## First Principle

Aion earns complexity.

We will not build a galaxy before one planet is compelling.

We will not build cities before planetary systems work.

We will not build individuals before aggregate civilization behavior justifies them.

We will not build multiplayer before a persistent world is worth sharing.

First goal:

Create a planet. Make time move. Make the planet change. Make those changes understandable.
