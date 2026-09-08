using Aion.Simulation.Worlds;

namespace Aion.Simulation.Operations;

public interface ISimulationOperation
{
    WorldState Apply(WorldState world);
}
