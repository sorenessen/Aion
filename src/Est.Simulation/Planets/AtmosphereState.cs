using System.Collections.Immutable;

namespace Est.Simulation.Planets;

public sealed record AtmosphereState
{
    private const double CompositionTolerance = 1e-9;

    public AtmosphereState(
        double surfacePressurePascals,
        IReadOnlyDictionary<string, double> compositionByMoleFraction)
    {
        if (!double.IsFinite(surfacePressurePascals) ||
            surfacePressurePascals < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(surfacePressurePascals),
                "Surface pressure must be a finite non-negative value.");
        }

        ArgumentNullException.ThrowIfNull(compositionByMoleFraction);

        var composition = compositionByMoleFraction
            .ToImmutableDictionary(
                pair => pair.Key,
                pair => pair.Value,
                StringComparer.Ordinal);

        foreach (var (species, fraction) in composition)
        {
            if (string.IsNullOrWhiteSpace(species))
            {
                throw new ArgumentException(
                    "Atmospheric species names cannot be empty.",
                    nameof(compositionByMoleFraction));
            }

            if (!double.IsFinite(fraction) ||
                fraction < 0 ||
                fraction > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(compositionByMoleFraction),
                    "Atmospheric mole fractions must be finite values between 0 and 1.");
            }
        }

        if (surfacePressurePascals == 0)
        {
            if (composition.Count != 0)
            {
                throw new ArgumentException(
                    "A vacuum cannot have atmospheric composition.",
                    nameof(compositionByMoleFraction));
            }
        }
        else
        {
            if (composition.Count == 0)
            {
                throw new ArgumentException(
                    "A non-zero atmosphere must define its composition.",
                    nameof(compositionByMoleFraction));
            }

            var totalFraction = composition.Values.Sum();

            if (Math.Abs(totalFraction - 1.0) > CompositionTolerance)
            {
                throw new ArgumentException(
                    "Atmospheric mole fractions must sum to 1.",
                    nameof(compositionByMoleFraction));
            }
        }

        SurfacePressurePascals = surfacePressurePascals;
        CompositionByMoleFraction = composition;
    }

    public double SurfacePressurePascals { get; }

    public ImmutableDictionary<string, double> CompositionByMoleFraction { get; }

    public static AtmosphereState Vacuum =>
        new(
            0,
            ImmutableDictionary<string, double>.Empty);
}
