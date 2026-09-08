using Aion.Application.Sessions;
using Aion.Simulation.Planets;
using Aion.Simulation.Time;
using Aion.Simulation.Worlds;

namespace Aion.Application.Tests.Sessions;

public sealed class SimulationSessionTests
{
    [Fact]
    public void Create_PreservesInitialWorld()
    {
        var world = new WorldState(WorldId.New(), SimulationTime.Zero);
        var session = new SimulationSession(world);

        Assert.Equal(world.Id, session.CurrentWorld.Id);
        Assert.Equal(0, session.CurrentWorld.CurrentTime.TotalSeconds);
        Assert.Single(session.Timeline.Checkpoints);
        Assert.Empty(session.Timeline.Events);
    }

    [Fact]
    public void Advance_UpdatesTimeAndRecordsHistory()
    {
        var session = CreateSession();

        var timeline = session.Advance(60);

        Assert.Equal(60, timeline.CurrentWorld.CurrentTime.TotalSeconds);
        Assert.Single(timeline.Events);
        Assert.Equal(60, timeline.Events[0].ElapsedSeconds);
        Assert.Equal(timeline.Id, timeline.Events[0].TimelineId);
    }

    [Fact]
    public void ExplicitAdvance_WorksWhilePaused()
    {
        var session = CreateSession();
        session.Pause();

        session.Advance(30);

        Assert.True(session.IsPaused);
        Assert.Equal(30, session.CurrentWorld.CurrentTime.TotalSeconds);
    }

    [Fact]
    public void Tick_RespectsPauseAndResume()
    {
        var session = CreateSession();
        session.Pause();

        session.Tick(60);
        Assert.Equal(0, session.CurrentWorld.CurrentTime.TotalSeconds);
        Assert.Empty(session.Timeline.Events);

        session.Resume();
        session.Tick(60);

        Assert.Equal(60, session.CurrentWorld.CurrentTime.TotalSeconds);
        Assert.Single(session.Timeline.Events);
    }

    [Fact]
    public void Advance_RejectsNegativeDurationWithoutChangingState()
    {
        var session = CreateSession();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => session.Advance(-1));

        Assert.Equal(0, session.CurrentWorld.CurrentTime.TotalSeconds);
        Assert.Empty(session.Timeline.Events);
    }

    [Fact]
    public void ReplacePlanetEnvironment_ChangesWorldWithoutAdvancingTime()
    {
        var planet = CreatePlanet();
        var session = new SimulationSession(
            new WorldState(
                WorldId.New(),
                new SimulationTime(120),
                [planet]));

        var originalTimeline = session.Timeline;

        var environment = new PlanetEnvironment(
            300,
            0.65,
            0.02,
            AtmosphereState.Vacuum);

        var timeline = session.ReplacePlanetEnvironment(
            planet.Id,
            environment);

        var changedPlanet = Assert.Single(timeline.CurrentWorld.Planets);

        Assert.Equal(planet.Id, changedPlanet.Id);
        Assert.Equal(planet.Name, changedPlanet.Name);
        Assert.Equal(planet.MassKilograms, changedPlanet.MassKilograms);
        Assert.Equal(planet.MeanRadiusMeters, changedPlanet.MeanRadiusMeters);
        Assert.Same(environment, changedPlanet.Environment);
        Assert.Equal(120, timeline.CurrentWorld.CurrentTime.TotalSeconds);
        Assert.Equal(originalTimeline.Id, timeline.Id);
        Assert.Single(timeline.Events);
        Assert.Equal(0, timeline.Events[0].ElapsedSeconds);
        Assert.Equal(planet.Id, timeline.Events[0].AffectedPlanetId);
        Assert.Equal(120, timeline.Events[0].OccurredAt.TotalSeconds);

        Assert.Empty(originalTimeline.Events);
        Assert.Same(planet.Environment,
            originalTimeline.CurrentWorld.Planets[0].Environment);
    }

    [Fact]
    public void ReplacePlanetEnvironment_RejectsUnknownPlanetWithoutChangingState()
    {
        var planet = CreatePlanet();
        var session = new SimulationSession(
            new WorldState(
                WorldId.New(),
                SimulationTime.Zero,
                [planet]));

        var originalTimeline = session.Timeline;

        Assert.Throws<PlanetNotFoundException>(
            () => session.ReplacePlanetEnvironment(
                PlanetId.New(),
                new PlanetEnvironment(
                    300,
                    0.5,
                    0,
                    AtmosphereState.Vacuum)));

        Assert.Same(originalTimeline, session.Timeline);
        Assert.Empty(session.Timeline.Events);
    }

    private static PlanetState CreatePlanet() =>
        new(
            PlanetId.New(),
            "Test Planet",
            5.9722e24,
            6_371_000,
            new PlanetEnvironment(
                288.15,
                0.71,
                0.1,
                AtmosphereState.Vacuum));

    private static SimulationSession CreateSession() =>
        new(new WorldState(WorldId.New(), SimulationTime.Zero));
}
