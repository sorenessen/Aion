using Est.Simulation.Planets;

namespace Est.Simulation.Tests.Planets;

public class PlanetPhysicsTests
{
    [Fact]
    public void EarthLikePlanet_HasExpectedSurfaceGravity()
    {
        var gravity = PlanetPhysics.CalculateSurfaceGravity(
            5.9722e24,
            6_371_000);

        Assert.InRange(gravity, 9.81, 9.83);
    }

    [Fact]
    public void DoublingMass_DoublesSurfaceGravity()
    {
        var original = PlanetPhysics.CalculateSurfaceGravity(1e24, 1e6);
        var doubled = PlanetPhysics.CalculateSurfaceGravity(2e24, 1e6);

        Assert.Equal(original * 2, doubled, 10);
    }

    [Fact]
    public void DoublingRadius_QuartersSurfaceGravity()
    {
        var original = PlanetPhysics.CalculateSurfaceGravity(1e24, 1e6);
        var doubled = PlanetPhysics.CalculateSurfaceGravity(1e24, 2e6);

        Assert.Equal(original / 4, doubled, 10);
    }

    [Fact]
    public void PlanetState_ExposesDerivedSurfaceGravity()
    {
        var planet = new PlanetState(
            PlanetId.New(),
            "Earth",
            5.9722e24,
            6_371_000,
            new PlanetEnvironment(288.15, 0.71, 0.03, AtmosphereState.Vacuum));

        Assert.InRange(
            planet.SurfaceGravityMetersPerSecondSquared,
            9.81,
            9.83);
    }
}
