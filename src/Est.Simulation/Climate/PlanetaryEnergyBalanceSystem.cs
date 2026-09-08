using Est.Simulation.Causality;
using Est.Simulation.Operations;
using Est.Simulation.Planets;
using Est.Simulation.Worlds;

namespace Est.Simulation.Climate;

public sealed class PlanetaryEnergyBalanceSystem
    : ICausalSystem
{
    public const double StefanBoltzmannConstant =
        5.670374419e-8;

    private readonly PlanetId _planetId;
    private readonly PlanetaryEnergyBalanceParameters _parameters;

    public PlanetaryEnergyBalanceSystem(
        PlanetId planetId,
        PlanetaryEnergyBalanceParameters parameters)
    {
        if (planetId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Planet identity cannot be empty.",
                nameof(planetId));
        }

        ArgumentNullException.ThrowIfNull(parameters);

        _planetId = planetId;
        _parameters = parameters;
    }

    public SimulationChange Evaluate(
        WorldState world,
        long elapsedSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (elapsedSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elapsedSeconds));
        }

        var planet = world.Planets
            .FirstOrDefault(candidate => candidate.Id == _planetId);

        if (planet is null)
        {
            throw new InvalidOperationException(
                "The target planet does not exist in this world.");
        }

        var environment = planet.Environment;

        var albedo = CalculateAlbedo(
            environment.IceCoverageFraction);

        var absorbedSolarFlux =
            _parameters.StellarFluxWattsPerSquareMeter
            * (1 - albedo)
            / 4;

        var outgoingLongwaveFlux =
            _parameters.EffectiveLongwaveEmissivity
            * StefanBoltzmannConstant
            * Math.Pow(
                environment.MeanSurfaceTemperatureKelvin,
                4);

        var netFlux =
            absorbedSolarFlux - outgoingLongwaveFlux;

        var temperatureChange =
            netFlux
            * elapsedSeconds
            / _parameters
                .EffectiveHeatCapacityJoulesPerSquareMeterKelvin;

        var newTemperature =
            environment.MeanSurfaceTemperatureKelvin
            + temperatureChange;

        if (!double.IsFinite(newTemperature) ||
            newTemperature < 0)
        {
            throw new InvalidOperationException(
                "The energy-balance integration produced an invalid temperature. Use a smaller step or different model parameters.");
        }

        var targetIceCoverage =
            CalculateTargetIceCoverage(newTemperature);

        var responseFraction = Math.Min(
            1.0,
            elapsedSeconds
            / _parameters.IceResponseTimescaleSeconds);

        var newIceCoverage =
            environment.IceCoverageFraction
            + (targetIceCoverage
                - environment.IceCoverageFraction)
            * responseFraction;

        newIceCoverage = Math.Clamp(
            newIceCoverage,
            0,
            1);

        var newEnvironment =
            new PlanetEnvironment(
                newTemperature,
                environment.SurfaceWaterFraction,
                newIceCoverage,
                environment.Atmosphere);

        return new SimulationChange(
            new ReplacePlanetEnvironmentOperation(
                planet.Id,
                newEnvironment),
            "planetary-energy-balance",
            "Radiative energy imbalance changed planetary temperature and ice coverage.",
            planet.Id,
            elapsedSeconds,
            new Dictionary<string, double>
            {
                ["absorbedSolarFluxWattsPerSquareMeter"] =
                    absorbedSolarFlux,
                ["outgoingLongwaveFluxWattsPerSquareMeter"] =
                    outgoingLongwaveFlux,
                ["netRadiativeFluxWattsPerSquareMeter"] =
                    netFlux,
                ["previousTemperatureKelvin"] =
                    environment.MeanSurfaceTemperatureKelvin,
                ["newTemperatureKelvin"] =
                    newTemperature,
                ["previousIceCoverageFraction"] =
                    environment.IceCoverageFraction,
                ["newIceCoverageFraction"] =
                    newIceCoverage
            });
    }

    private double CalculateAlbedo(
        double iceCoverageFraction)
    {
        return _parameters.IceFreeAlbedo
            + iceCoverageFraction
            * (_parameters.IceAlbedo
                - _parameters.IceFreeAlbedo);
    }

    private double CalculateTargetIceCoverage(
        double temperatureKelvin)
    {
        if (temperatureKelvin <=
            _parameters.FullIceTemperatureKelvin)
        {
            return 1;
        }

        if (temperatureKelvin >=
            _parameters.IceFreeTemperatureKelvin)
        {
            return 0;
        }

        var range =
            _parameters.IceFreeTemperatureKelvin
            - _parameters.FullIceTemperatureKelvin;

        return (
            _parameters.IceFreeTemperatureKelvin
            - temperatureKelvin)
            / range;
    }
}
