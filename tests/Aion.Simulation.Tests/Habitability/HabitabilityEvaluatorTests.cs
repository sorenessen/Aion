using Aion.Simulation.Habitability;
using Aion.Simulation.Planets;

namespace Aion.Simulation.Tests.Habitability;

public class HabitabilityEvaluatorTests
{
    [Fact]
    public void Assess_EarthLikeSurfaceLife_IdentifiesReferenceProfile()
    {
        var environment = CreateEarthLikeEnvironment();

        var assessment = HabitabilityEvaluator.Assess(
            environment,
            HabitabilityProfile.EarthLikeSurfaceLife);

        Assert.Equal(
            HabitabilityProfile.EarthLikeSurfaceLife,
            assessment.Profile);
    }

    [Fact]
    public void Assess_CurrentModelReturnsUnknownRatherThanUniversalHabitabilityClaim()
    {
        var environment = CreateEarthLikeEnvironment();

        var assessment = HabitabilityEvaluator.Assess(
            environment,
            HabitabilityProfile.EarthLikeSurfaceLife);

        Assert.Equal(
            HabitabilityPotential.Unknown,
            assessment.Potential);
    }

    [Fact]
    public void Assess_ReportsAvailableEnvironmentalEvidence()
    {
        var environment = CreateEarthLikeEnvironment();

        var assessment = HabitabilityEvaluator.Assess(
            environment,
            HabitabilityProfile.EarthLikeSurfaceLife);

        Assert.Contains(
            assessment.Evidence,
            item => item.Contains("Surface water"));
        Assert.Contains(
            assessment.Evidence,
            item => item.Contains("atmosphere"));
        Assert.Contains(
            assessment.Evidence,
            item => item.Contains("288.15"));
    }

    [Fact]
    public void Assess_ReportsFactorsNeededForBroaderBiologicalAssessment()
    {
        var environment = CreateEarthLikeEnvironment();

        var assessment = HabitabilityEvaluator.Assess(
            environment,
            HabitabilityProfile.EarthLikeSurfaceLife);

        Assert.Contains("pH", assessment.MissingFactors);
        Assert.Contains("Salinity", assessment.MissingFactors);
        Assert.Contains("Radiation environment", assessment.MissingFactors);
        Assert.Contains("Usable energy sources", assessment.MissingFactors);
    }

    [Fact]
    public void Assess_ExtremeSurfaceConditionsDoNotClaimUniversalUninhabitability()
    {
        var environment = new PlanetEnvironment(
            150,
            1,
            1,
            AtmosphereState.Vacuum);

        var assessment = HabitabilityEvaluator.Assess(
            environment,
            HabitabilityProfile.EarthLikeSurfaceLife);

        Assert.NotEqual(
            HabitabilityPotential.Unfavorable,
            assessment.Potential);

        Assert.Equal(
            HabitabilityPotential.Unknown,
            assessment.Potential);
    }

    private static PlanetEnvironment CreateEarthLikeEnvironment()
    {
        return new PlanetEnvironment(
            288.15,
            0.71,
            0.03,
            new AtmosphereState(
                101_325,
                new Dictionary<string, double>
                {
                    ["N2"] = 0.78,
                    ["O2"] = 0.21,
                    ["Ar"] = 0.01
                }));
    }
}
