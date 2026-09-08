using Aion.Simulation.Planets;

namespace Aion.Simulation.Habitability;

public static class HabitabilityEvaluator
{
    public static HabitabilityAssessment Assess(
        PlanetEnvironment environment,
        HabitabilityProfile profile)
    {
        ArgumentNullException.ThrowIfNull(environment);

        return profile switch
        {
            HabitabilityProfile.EarthLikeSurfaceLife =>
                AssessEarthLikeSurfaceLife(environment),

            _ => throw new ArgumentOutOfRangeException(
                nameof(profile),
                profile,
                "Unsupported habitability profile.")
        };
    }

    private static HabitabilityAssessment AssessEarthLikeSurfaceLife(
        PlanetEnvironment environment)
    {
        var evidence = new List<string>();
        var missingFactors = new List<string>
        {
            "Local temperature distribution",
            "Liquid water availability",
            "Water activity",
            "pH",
            "Salinity",
            "Radiation environment",
            "Bioavailable nutrients",
            "Usable energy sources",
            "Atmospheric chemistry suitability"
        };

        if (environment.SurfaceWaterFraction > 0)
        {
            evidence.Add("Surface water is present.");
        }
        else
        {
            evidence.Add("No surface water is represented.");
        }

        if (environment.Atmosphere.SurfacePressurePascals > 0)
        {
            evidence.Add("A non-vacuum atmosphere is present.");
        }
        else
        {
            evidence.Add("The modeled surface is exposed to vacuum.");
        }

        evidence.Add(
            $"Mean surface temperature is {environment.MeanSurfaceTemperatureKelvin} K.");

        return new HabitabilityAssessment(
            HabitabilityProfile.EarthLikeSurfaceLife,
            HabitabilityPotential.Unknown,
            evidence,
            missingFactors);
    }
}
