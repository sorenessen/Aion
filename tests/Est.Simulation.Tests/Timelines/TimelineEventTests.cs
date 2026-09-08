using Est.Simulation.Causality;
using Est.Simulation.Operations;
using Est.Simulation.Time;
using Est.Simulation.Timelines;

namespace Est.Simulation.Tests.Timelines;

public class TimelineEventTests
{
    [Fact]
    public void FromChange_CapturesHistoricalFactsWithoutOperation()
    {
        var metrics = new Dictionary<string, double>
        {
            ["netFlux"] = 12.5
        };

        var change = new SimulationChange(
            new AdvanceTimeOperation(0),
            "planetary-energy-balance",
            "Planet warmed.",
            null,
            60,
            metrics);

        var timelineEvent =
            TimelineEvent.FromChange(
                TimelineId.New(),
                new SimulationTime(160),
                change);

        Assert.Equal(
            "planetary-energy-balance",
            timelineEvent.Cause);

        Assert.Equal(
            "Planet warmed.",
            timelineEvent.Summary);

        Assert.Equal(
            160,
            timelineEvent.OccurredAt.TotalSeconds);

        Assert.Equal(
            60,
            timelineEvent.ElapsedSeconds);

        Assert.Equal(
            12.5,
            timelineEvent.Metrics["netFlux"]);
    }

    [Fact]
    public void Constructor_CopiesMetricsIntoImmutableStorage()
    {
        var metrics = new Dictionary<string, double>
        {
            ["value"] = 42
        };

        var timelineEvent =
            new TimelineEvent(
                Guid.NewGuid(),
                TimelineId.New(),
                SimulationTime.Zero,
                "test",
                "Test event.",
                null,
                0,
                metrics);

        metrics["value"] = 100;

        Assert.Equal(
            42,
            timelineEvent.Metrics["value"]);
    }

    [Fact]
    public void Constructor_RejectsNonFiniteMetric()
    {
        Assert.Throws<ArgumentException>(
            () => new TimelineEvent(
                Guid.NewGuid(),
                TimelineId.New(),
                SimulationTime.Zero,
                "test",
                "Test event.",
                null,
                0,
                new Dictionary<string, double>
                {
                    ["value"] = double.NaN
                }));
    }
}
