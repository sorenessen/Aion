using Aion.Application.Sessions;
using Aion.Simulation.Time;
using Aion.Simulation.Worlds;

namespace Aion.Application.Tests.Sessions;

public sealed class SimulationSessionManagerTests
{
    [Fact]
    public void Create_RegistersIndependentSession()
    {
        var manager =
            new SimulationSessionManager();

        var world =
            new WorldState(
                WorldId.New(),
                SimulationTime.Zero);

        var id = manager.Create(world);
        var session = manager.Get(id);

        Assert.Equal(
            world.Id,
            session.CurrentWorld.Id);

        Assert.Equal(
            0,
            session.CurrentWorld.CurrentTime.TotalSeconds);
    }

    [Fact]
    public void Create_AssignsDifferentSessionIdentities()
    {
        var manager =
            new SimulationSessionManager();

        var first =
            manager.Create(CreateWorld());

        var second =
            manager.Create(CreateWorld());

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Sessions_AdvanceIndependently()
    {
        var manager =
            new SimulationSessionManager();

        var first =
            manager.Create(CreateWorld());

        var second =
            manager.Create(CreateWorld());

        manager.Get(first).Advance(60);

        Assert.Equal(
            60,
            manager.Get(first)
                .CurrentWorld
                .CurrentTime
                .TotalSeconds);

        Assert.Equal(
            0,
            manager.Get(second)
                .CurrentWorld
                .CurrentTime
                .TotalSeconds);
    }

    [Fact]
    public void TryGet_ReturnsFalseForUnknownSession()
    {
        var manager =
            new SimulationSessionManager();

        var found =
            manager.TryGet(
                SimulationSessionId.New(),
                out var session);

        Assert.False(found);
        Assert.Null(session);
    }

    [Fact]
    public void Get_ThrowsForUnknownSession()
    {
        var manager =
            new SimulationSessionManager();

        Assert.Throws<KeyNotFoundException>(
            () => manager.Get(
                SimulationSessionId.New()));
    }

    private static WorldState CreateWorld() =>
        new(
            WorldId.New(),
            SimulationTime.Zero);
}
