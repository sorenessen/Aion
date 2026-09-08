using Aion.Simulation.Time;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Tests.Worlds;

public class WorldStateTests
{
    [Fact]
    public void Constructor_PreservesIdentityAndTime()
    {
        var id = WorldId.New();
        var time = new SimulationTime(500);

        var world = new WorldState(id, time);

        Assert.Equal(id, world.Id);
        Assert.Equal(time, world.CurrentTime);
    }

    [Fact]
    public void AdvanceBy_ReturnsNewWorldAtAdvancedTime()
    {
        var id = WorldId.New();
        var world = new WorldState(id, new SimulationTime(100));

        var advanced = world.AdvanceBy(60);

        Assert.Equal(id, advanced.Id);
        Assert.Equal(160, advanced.CurrentTime.TotalSeconds);
        Assert.Equal(100, world.CurrentTime.TotalSeconds);
    }

    [Fact]
    public void Constructor_RejectsEmptyWorldIdentity()
    {
        Assert.Throws<ArgumentException>(
            () => new WorldState(default, SimulationTime.Zero));
    }

    [Fact]
    public void AdvanceBy_NegativeDuration_Throws()
    {
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => world.AdvanceBy(-1));
    }
}
