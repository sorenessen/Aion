using Aion.Simulation.Operations;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Causality;

public interface ICausalSystem
{
    ISimulationOperation Evaluate(
        WorldState world,
        long elapsedSeconds);
}
