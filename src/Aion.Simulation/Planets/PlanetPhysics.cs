namespace Aion.Simulation.Planets;

public static class PlanetPhysics
{
    // CODATA 2022 recommended Newtonian gravitational constant.
    public const double GravitationalConstant = 6.67430e-11;

    public static double CalculateSurfaceGravity(
        double massKilograms,
        double radiusMeters)
    {
        if (!double.IsFinite(massKilograms) || massKilograms <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(massKilograms));
        }

        if (!double.IsFinite(radiusMeters) || radiusMeters <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radiusMeters));
        }

        return GravitationalConstant
            * massKilograms
            / (radiusMeters * radiusMeters);
    }
}
