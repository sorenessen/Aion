using Aion.Simulation.Worlds;

namespace Aion.Simulation.Causality;

public sealed record SimulationStepResult(
    WorldState World,
    SimulationChange Change);
