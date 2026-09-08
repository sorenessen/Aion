using System.Collections.Immutable;

namespace Aion.Simulation.Habitability;

public sealed record HabitabilityAssessment
{
    public HabitabilityAssessment(
        HabitabilityProfile profile,
        HabitabilityPotential potential,
        IEnumerable<string> evidence,
        IEnumerable<string> missingFactors)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(missingFactors);

        Profile = profile;
        Potential = potential;
        Evidence = evidence.ToImmutableArray();
        MissingFactors = missingFactors.ToImmutableArray();
    }

    public HabitabilityProfile Profile { get; }

    public HabitabilityPotential Potential { get; }

    public ImmutableArray<string> Evidence { get; }

    public ImmutableArray<string> MissingFactors { get; }
}
