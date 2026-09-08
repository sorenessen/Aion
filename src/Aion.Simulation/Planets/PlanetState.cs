namespace Aion.Simulation.Planets;

public sealed record PlanetState
{
    public PlanetState(
        PlanetId id,
        string name,
        double massKilograms,
        double meanRadiusMeters)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Planet identity cannot be empty.",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Planet name cannot be empty.",
                nameof(name));
        }

        if (!double.IsFinite(massKilograms) || massKilograms <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(massKilograms),
                "Planet mass must be a finite positive value.");
        }

        if (!double.IsFinite(meanRadiusMeters) || meanRadiusMeters <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(meanRadiusMeters),
                "Planet mean radius must be a finite positive value.");
        }

        Id = id;
        Name = name;
        MassKilograms = massKilograms;
        MeanRadiusMeters = meanRadiusMeters;
    }

    public PlanetId Id { get; private init; }

    public string Name { get; init; }

    public double MassKilograms { get; private init; }

    public double MeanRadiusMeters { get; private init; }

    public double SurfaceGravityMetersPerSecondSquared =>
        PlanetPhysics.CalculateSurfaceGravity(
            MassKilograms,
            MeanRadiusMeters);
}
