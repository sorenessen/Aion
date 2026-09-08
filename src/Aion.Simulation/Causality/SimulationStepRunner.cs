using System.Collections.Immutable;
using Aion.Simulation.Operations;
using Aion.Simulation.Worlds;

namespace Aion.Simulation.Causality;

public static class SimulationStepRunner
{
    public static SimulationStepResult Step(
        WorldState world,
        long elapsedSeconds,
        ICausalSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);

        return Step(
            world,
            elapsedSeconds,
            [system]);
    }

    public static SimulationStepResult Step(
        WorldState world,
        long elapsedSeconds,
        IEnumerable<ICausalSystem> systems)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(systems);

        if (elapsedSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));

        var systemArray = systems.ToImmutableArray();

        if (systemArray.Any(system => system is null))
            throw new ArgumentException(
                "Causal systems cannot contain null entries.",
                nameof(systems));

        var workingWorld = world;
        var changes = ImmutableArray.CreateBuilder<SimulationChange>();

        foreach (var system in systemArray)
        {
            var change = system.Evaluate(
                workingWorld,
                elapsedSeconds);

            if (change is null)
                throw new InvalidOperationException(
                    "A causal system must return a simulation change.");

            if (change.ElapsedSeconds != elapsedSeconds)
                throw new InvalidOperationException(
                    "A causal change must describe the step duration.");

            var changedWorld =
                SimulationOperationExecutor.Apply(
                    workingWorld,
                    change.Operation);

            if (changedWorld.Id != workingWorld.Id ||
                changedWorld.CurrentTime != workingWorld.CurrentTime)
            {
                throw new InvalidOperationException(
                    "A causal operation must preserve world identity and simulation time.");
            }

            workingWorld = changedWorld;
            changes.Add(change);
        }

        if (changes.Count == 0)
        {
            changes.Add(
                new SimulationChange(
                    new AdvanceTimeOperation(elapsedSeconds),
                    "Explicit time advancement",
                    $"Advanced simulation time by {elapsedSeconds} seconds.",
                    null,
                    elapsedSeconds));
        }

        var advancedWorld =
            SimulationOperationExecutor.Apply(
                workingWorld,
                new AdvanceTimeOperation(elapsedSeconds));

        return new SimulationStepResult(
            advancedWorld,
            elapsedSeconds,
            changes.ToImmutable());
    }
}
