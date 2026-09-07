using Aion.Simulation.Time;

namespace Aion.Simulation.Tests.Time;

public class SimulationClockTests
{
    [Fact]
    public void Constructor_UsesInitialTime()
    {
        var clock = new SimulationClock(new SimulationTime(500));

        Assert.Equal(500, clock.CurrentTime.TotalSeconds);
        Assert.False(clock.IsPaused);
    }

    [Fact]
    public void AdvanceBy_WhenRunning_AdvancesCurrentTime()
    {
        var clock = new SimulationClock(SimulationTime.Zero);

        clock.AdvanceBy(60);

        Assert.Equal(60, clock.CurrentTime.TotalSeconds);
    }

    [Fact]
    public void Pause_PreventsTimeFromAdvancing()
    {
        var clock = new SimulationClock(SimulationTime.Zero);
        clock.Pause();

        clock.AdvanceBy(60);

        Assert.True(clock.IsPaused);
        Assert.Equal(SimulationTime.Zero, clock.CurrentTime);
    }

    [Fact]
    public void Resume_AllowsTimeToAdvanceAgain()
    {
        var clock = new SimulationClock(SimulationTime.Zero);
        clock.Pause();
        clock.AdvanceBy(60);

        clock.Resume();
        clock.AdvanceBy(60);

        Assert.False(clock.IsPaused);
        Assert.Equal(60, clock.CurrentTime.TotalSeconds);
    }

    [Fact]
    public void AdvanceBy_NegativeDuration_ThrowsWhenRunning()
    {
        var clock = new SimulationClock(SimulationTime.Zero);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => clock.AdvanceBy(-1));
    }

    [Fact]
    public void AdvanceBy_NegativeDuration_ThrowsWhenPaused()
    {
        var clock = new SimulationClock(SimulationTime.Zero);
        clock.Pause();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => clock.AdvanceBy(-1));
    }
}
