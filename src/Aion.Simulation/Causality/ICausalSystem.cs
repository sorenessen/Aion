using Aion.Simulation.Worlds;

namespace Aion.Simulation.Causality;

public interface ICausalSystem
{
    SimulationChange Evaluate(
        WorldState world,
        long elapsedSeconds);
}
