namespace Est.Simulation.Planets;

public sealed class PlanetNotFoundException : InvalidOperationException
{
    public PlanetNotFoundException(PlanetId planetId)
        : base($"Planet '{planetId.Value}' does not exist in this world.")
    {
        PlanetId = planetId;
    }

    public PlanetId PlanetId { get; }
}
