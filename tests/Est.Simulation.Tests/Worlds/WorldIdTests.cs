using Est.Simulation.Worlds;

namespace Est.Simulation.Tests.Worlds;

public class WorldIdTests
{
    [Fact]
    public void New_CreatesNonEmptyIdentity()
    {
        var id = WorldId.New();

        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void Constructor_EmptyGuid_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new WorldId(Guid.Empty));
    }
}
