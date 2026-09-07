using Aion.Simulation.Time;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Tests.Time;

public class SimulationClockTests
{
    [Fact]
    public void Constructor_StartsRunning()
    {
        var clock = new SimulationClock();

        Assert.False(clock.IsPaused);
    }

    [Fact]
    public void Tick_WhenRunning_ReturnsAdvancedWorld()
    {
        var clock = new SimulationClock();
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100));

        var result = clock.Tick(world, 60);

        Assert.Equal(160, result.CurrentTime.TotalSeconds);
        Assert.Equal(100, world.CurrentTime.TotalSeconds);
        Assert.Equal(world.Id, result.Id);
    }

    [Fact]
    public void Tick_WhenPaused_DoesNotAdvanceWorld()
    {
        var clock = new SimulationClock();
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100));

        clock.Pause();

        var result = clock.Tick(world, 60);

        Assert.True(clock.IsPaused);
        Assert.Same(world, result);
        Assert.Equal(100, result.CurrentTime.TotalSeconds);
    }

    [Fact]
    public void Resume_AllowsAutomaticAdvancementAgain()
    {
        var clock = new SimulationClock();
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero);

        clock.Pause();
        var pausedResult = clock.Tick(world, 60);

        clock.Resume();
        var resumedResult = clock.Tick(pausedResult, 60);

        Assert.False(clock.IsPaused);
        Assert.Equal(60, resumedResult.CurrentTime.TotalSeconds);
    }

    [Fact]
    public void Tick_NegativeDuration_ThrowsWhenRunning()
    {
        var clock = new SimulationClock();
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => clock.Tick(world, -1));
    }

    [Fact]
    public void Tick_NegativeDuration_ThrowsWhenPaused()
    {
        var clock = new SimulationClock();
        var world = new WorldState(
            WorldId.New(),
            SimulationTime.Zero);

        clock.Pause();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => clock.Tick(world, -1));
    }

    [Fact]
    public void ExplicitWorldAdvance_WorksWhileClockIsPaused()
    {
        var clock = new SimulationClock();
        var world = new WorldState(
            WorldId.New(),
            new SimulationTime(100));

        clock.Pause();

        var result = world.AdvanceBy(60);

        Assert.True(clock.IsPaused);
        Assert.Equal(160, result.CurrentTime.TotalSeconds);
    }
}
