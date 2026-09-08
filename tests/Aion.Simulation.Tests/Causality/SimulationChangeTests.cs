using Aion.Simulation.Causality;
using Aion.Simulation.Operations;
using Aion.Simulation.Planets;

namespace Aion.Simulation.Tests.Causality;

public class SimulationChangeTests
{
    [Fact]
    public void Constructor_PreservesCauseAndMetrics()
    {
        var planetId = PlanetId.New();

        var change = new SimulationChange(
            new AdvanceTimeOperation(0),
            "test-cause",
            "A test change.",
            planetId,
            60,
            new Dictionary<string, double>
            {
                ["value"] = 42
            });

        Assert.Equal("test-cause", change.Cause);
        Assert.Equal("A test change.", change.Summary);
        Assert.Equal(planetId, change.AffectedPlanetId);
        Assert.Equal(60, change.ElapsedSeconds);
        Assert.Equal(42, change.Metrics["value"]);
    }

    [Fact]
    public void Constructor_CopiesMetricDictionary()
    {
        var metrics = new Dictionary<string, double>
        {
            ["value"] = 42
        };

        var change = new SimulationChange(
            new AdvanceTimeOperation(0),
            "test-cause",
            "A test change.",
            null,
            60,
            metrics);

        metrics["value"] = 100;

        Assert.Equal(42, change.Metrics["value"]);
    }
}
