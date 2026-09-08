using Aion.Simulation.Planets;

namespace Aion.Simulation.Tests.Planets;

public class PlanetIdTests
{
    [Fact]
    public void New_ProducesNonEmptyIdentity()
    {
        var id = PlanetId.New();

        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void Constructor_RejectsEmptyIdentity()
    {
        Assert.Throws<ArgumentException>(
            () => new PlanetId(Guid.Empty));
    }
}
