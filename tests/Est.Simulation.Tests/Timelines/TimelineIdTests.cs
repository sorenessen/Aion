using Est.Simulation.Timelines;

namespace Est.Simulation.Tests.Timelines;

public class TimelineIdTests
{
    [Fact]
    public void New_CreatesNonEmptyIdentity()
    {
        var id = TimelineId.New();

        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void Constructor_RejectsEmptyIdentity()
    {
        Assert.Throws<ArgumentException>(
            () => new TimelineId(Guid.Empty));
    }
}
