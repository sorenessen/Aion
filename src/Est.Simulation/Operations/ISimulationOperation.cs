using Est.Simulation.Worlds;

namespace Est.Simulation.Operations;

public interface ISimulationOperation
{
    WorldState Apply(WorldState world);
}
