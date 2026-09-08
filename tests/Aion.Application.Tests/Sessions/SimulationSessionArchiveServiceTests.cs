using Aion.Application.Sessions;
using Aion.Persistence.Archives;
using Aion.Persistence.Storage;
using Aion.Simulation.Planets;
using Aion.Simulation.Time;
using Aion.Simulation.Worlds;

namespace Aion.Application.Tests.Sessions;

public sealed class SimulationSessionArchiveServiceTests
{
    [Fact]
    public void Load_CorruptArchive_DoesNotRegisterSession()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "Aion.Tests",
            Guid.NewGuid().ToString("N"),
            "timeline.json");

        try
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(path)!);

            File.WriteAllText(
                path,
                "{ not valid json");

            var manager =
                new SimulationSessionManager();

            var service =
                new SimulationSessionArchiveService(
                    manager,
                    new TimelineArchiveFileStore());

            Assert.Throws<System.Text.Json.JsonException>(
                () => service.Load(path));

            var knownId =
                SimulationSessionId.New();

            Assert.False(
                manager.TryGet(
                    knownId,
                    out _));
        }
        finally
        {
            var directory =
                Path.GetDirectoryName(path)!;

            if (Directory.Exists(directory))
            {
                Directory.Delete(
                    directory,
                    recursive: true);
            }
        }
    }

    [Fact]
    public void SaveAndLoad_PreservesHistoryAndCreatesIndependentSession()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "Aion.Tests",
            Guid.NewGuid().ToString("N"),
            "timeline.json");

        try
        {
            var manager = new SimulationSessionManager();
            var service = new SimulationSessionArchiveService(
                manager,
                new TimelineArchiveFileStore());

            var planet = new PlanetState(
                PlanetId.New(),
                "Test Planet",
                5.9722e24,
                6_371_000,
                new PlanetEnvironment(
                    288.15,
                    0.71,
                    0.1,
                    AtmosphereState.Vacuum));

            var originalId = manager.Create(
                new WorldState(
                    WorldId.New(),
                    SimulationTime.Zero,
                    [planet]));

            var original = manager.Get(originalId);
            original.Advance(120);

            original.ReplacePlanetEnvironment(
                planet.Id,
                new PlanetEnvironment(
                    300,
                    0.65,
                    0.02,
                    AtmosphereState.Vacuum));

            var savedTimeline = original.Timeline;

            service.Save(
                originalId,
                path,
                new TimelineArchiveProvenance(
                    "Aion",
                    "0.1.0-alpha",
                    "simulation"));

            var loadedId = service.Load(path);
            var loaded = manager.Get(loadedId);

            Assert.NotEqual(originalId, loadedId);
            Assert.NotSame(original, loaded);
            Assert.Equal(savedTimeline.Id, loaded.Timeline.Id);
            Assert.Equal(
                savedTimeline.CurrentWorld.Id,
                loaded.CurrentWorld.Id);
            Assert.Equal(
                savedTimeline.CurrentWorld.CurrentTime,
                loaded.CurrentWorld.CurrentTime);

            Assert.Equal(
                savedTimeline.Checkpoints.Select(x => x.Id),
                loaded.Timeline.Checkpoints.Select(x => x.Id));

            Assert.Equal(
                savedTimeline.Events.Select(x => x.Id),
                loaded.Timeline.Events.Select(x => x.Id));

            var restoredPlanet = Assert.Single(
                loaded.CurrentWorld.Planets);

            Assert.Equal(planet.Id, restoredPlanet.Id);
            Assert.Equal(
                300,
                restoredPlanet.Environment.MeanSurfaceTemperatureKelvin);
            Assert.Equal(
                0.65,
                restoredPlanet.Environment.SurfaceWaterFraction);
            Assert.Equal(
                0.02,
                restoredPlanet.Environment.IceCoverageFraction);

            Assert.Equal(2, loaded.Timeline.Events.Length);
            Assert.Equal(
                "User intervention",
                loaded.Timeline.Events[1].Cause);
            Assert.Equal(
                planet.Id,
                loaded.Timeline.Events[1].AffectedPlanetId);
            Assert.Equal(
                0,
                loaded.Timeline.Events[1].ElapsedSeconds);

            loaded.Advance(60);

            Assert.Equal(
                180,
                loaded.CurrentWorld.CurrentTime.TotalSeconds);
            Assert.Equal(
                120,
                original.CurrentWorld.CurrentTime.TotalSeconds);
            Assert.Equal(3, loaded.Timeline.Events.Length);
            Assert.Equal(2, original.Timeline.Events.Length);
        }
        finally
        {
            var directory = Path.GetDirectoryName(path)!;

            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }
}
