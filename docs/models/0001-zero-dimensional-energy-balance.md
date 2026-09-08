# Model 0001: Zero-Dimensional Planetary Energy Balance

## Status

Initial causal model for Aion Phase 3.

## Purpose

This model exists to establish a scientifically grounded causal simulation loop in which elapsed time changes planetary environmental state.

It is deliberately constrained. It is not intended to reproduce Earth's climate or predict real-world climate outcomes.

## State Used

The model currently reads:

- mean surface temperature
- ice coverage fraction

It preserves:

- surface water fraction
- atmosphere

The atmosphere is not yet dynamically coupled to radiative transfer.

## External Model Parameters

The model requires explicit values for:

- stellar flux
- effective longwave emissivity
- effective heat capacity
- ice-free albedo
- ice albedo
- temperature associated with full ice coverage
- temperature associated with zero ice coverage
- ice response timescale

These are model assumptions, not universal planetary constants.

## Energy Balance

Globally averaged absorbed stellar flux is approximated as:

absorbed = S * (1 - albedo) / 4

where S is stellar flux at the planet.

Outgoing longwave radiation is approximated as:

outgoing = effective_emissivity * sigma * T^4

where sigma is the Stefan-Boltzmann constant.

Temperature evolves from the net radiative flux using a single effective heat capacity.

## Ice-Albedo Feedback

Planetary albedo is interpolated between an ice-free value and an ice-covered value using current ice coverage.

The model calculates a temperature-dependent target ice coverage and relaxes current ice coverage toward that target over an explicit response timescale.

This creates a positive feedback across simulation steps:

- cooling can increase ice
- increased ice raises albedo
- higher albedo reduces absorbed stellar energy
- reduced absorption can cause additional cooling

The inverse process occurs during warming.

## Important Limitations

This is a zero-dimensional model. The entire planet is represented by global mean quantities.

It does not currently model:

- latitude
- longitude
- seasons
- orbital variation
- day/night cycles
- clouds
- weather
- atmospheric layers
- spectral absorption
- greenhouse gas chemistry
- convection
- evaporation
- precipitation
- ocean circulation
- horizontal heat transport
- land-ocean differences
- sea ice versus land ice
- thermal stratification
- geothermal heating
- biological feedbacks
- carbon cycling

Effective longwave emissivity is an explicit simplification and must not be interpreted as a complete greenhouse model.

The explicit time integration can become invalid for excessively large time steps or extreme parameters. The system rejects resulting non-finite or sub-zero temperatures rather than silently repairing them.

## Architectural Role

The model implements ICausalSystem.

It evaluates authoritative world state over an explicit elapsed duration and returns an ISimulationOperation describing the proposed environmental change.

It does not advance authoritative simulation time itself. SimulationStepRunner applies the causal operation and then advances time separately.

This separation is intentional so future causal systems can operate at different characteristic timescales without owning the world clock.
