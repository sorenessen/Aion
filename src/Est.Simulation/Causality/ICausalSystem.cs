using Est.Simulation.Worlds;

namespace Est.Simulation.Causality;

public interface ICausalSystem
{
    SimulationChange Evaluate(
        WorldState world,
        long elapsedSeconds);
}
