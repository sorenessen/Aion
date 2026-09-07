using Aion.Simulation.Time;

namespace Aion.Simulation.Tests.Time;

public class SimulationTimeTests
{
    [Fact]
    public void Zero_StartsAtZeroSeconds()
    {
        Assert.Equal(0, SimulationTime.Zero.TotalSeconds);
    }


    [Fact]
    public void Constructor_NegativeTime_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new SimulationTime(-1));
    }

    [Fact]
    public void AdvanceBy_ReturnsNewAdvancedTime()
    {
        var start = new SimulationTime(100);

        var result = start.AdvanceBy(60);

        Assert.Equal(160, result.TotalSeconds);
        Assert.Equal(100, start.TotalSeconds);
    }

    [Fact]
    public void AdvanceBy_Zero_DoesNotChangeTime()
    {
        var start = new SimulationTime(100);

        var result = start.AdvanceBy(0);

        Assert.Equal(start, result);
    }

    [Fact]
    public void AdvanceBy_NegativeDuration_Throws()
    {
        var start = SimulationTime.Zero;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => start.AdvanceBy(-1));
    }
}
