using Est.Api;
using Est.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Json;

namespace Est.Api.Tests.Sessions;

public sealed class SessionArchiveEndpointTests
{
    [Fact]
    public async Task SaveAndLoad_PreservesWorldAndTimelineHistory()
    {
        var directory = CreateTempDirectory();

        try
        {
            await using var factory = CreateFactory(directory);
            using var client = factory.CreateClient();

            var creation = await client.PostAsJsonAsync(
                "/sessions",
                new CreateSessionRequest(
                [
                    new PlanetCreationRequest(
                        "Test Planet",
                        5.9722e24,
                        6_371_000,
                        new PlanetEnvironmentCreationRequest(
                            288.15,
                            0.71,
                            0.1,
                            new AtmosphereCreationRequest(
                                0,
                                new Dictionary<string, double>())))
                ]));

            creation.EnsureSuccessStatusCode();

            var created = await creation.Content
                .ReadFromJsonAsync<SessionResponse>();

            Assert.NotNull(created);

            var originalWorld = await client.GetFromJsonAsync<WorldResponse>(
                $"/sessions/{created.SessionId}/world");

            Assert.NotNull(originalWorld);
            var planet = Assert.Single(originalWorld.Planets);

            var advance = await client.PostAsJsonAsync(
                $"/sessions/{created.SessionId}/advance",
                new AdvanceTimeRequest(120));

            Assert.Equal(HttpStatusCode.OK, advance.StatusCode);

            var intervention = await client.PostAsJsonAsync(
                $"/sessions/{created.SessionId}/planets/{planet.PlanetId}/environment",
                new ReplacePlanetEnvironmentRequest(
                    300,
                    0.65,
                    0.02,
                    new AtmosphereReplacementRequest(
                        0,
                        new Dictionary<string, double>())));

            Assert.Equal(HttpStatusCode.OK, intervention.StatusCode);

            var save = await client.PostAsync(
                $"/sessions/{created.SessionId}/archives",
                null);

            Assert.Equal(HttpStatusCode.Created, save.StatusCode);

            var archive = await save.Content
                .ReadFromJsonAsync<ArchiveResponse>();

            Assert.NotNull(archive);
            Assert.NotEqual(Guid.Empty, archive.ArchiveId);
            Assert.Equal(created.WorldId, archive.WorldId);
            Assert.Equal(created.TimelineId, archive.TimelineId);

            var load = await client.PostAsync(
                $"/archives/{archive.ArchiveId}/load",
                null);

            Assert.Equal(HttpStatusCode.Created, load.StatusCode);

            var restored = await load.Content
                .ReadFromJsonAsync<SessionResponse>();

            Assert.NotNull(restored);
            Assert.NotEqual(created.SessionId, restored.SessionId);
            Assert.Equal(created.WorldId, restored.WorldId);
            Assert.Equal(created.TimelineId, restored.TimelineId);
            Assert.Equal(120, restored.CurrentTimeSeconds);
            Assert.Equal(2, restored.EventCount);
            Assert.Equal(1, restored.CheckpointCount);

            var restoredWorld = await client.GetFromJsonAsync<WorldResponse>(
                $"/sessions/{restored.SessionId}/world");

            Assert.NotNull(restoredWorld);
            var restoredPlanet = Assert.Single(restoredWorld.Planets);

            Assert.Equal(planet.PlanetId, restoredPlanet.PlanetId);
            Assert.Equal(300, restoredPlanet.Environment.MeanSurfaceTemperatureKelvin);
            Assert.Equal(0.65, restoredPlanet.Environment.SurfaceWaterFraction);
            Assert.Equal(0.02, restoredPlanet.Environment.IceCoverageFraction);

            var timeline = await client.GetFromJsonAsync<TimelineResponse>(
                $"/sessions/{restored.SessionId}/timeline");

            Assert.NotNull(timeline);
            Assert.Equal(created.TimelineId, timeline.TimelineId);
            Assert.Equal(2, timeline.Events.Length);
            Assert.Equal("User intervention", timeline.Events[1].Cause);
            Assert.Equal(planet.PlanetId, timeline.Events[1].AffectedPlanetId);
            Assert.Equal(0, timeline.Events[1].ElapsedSeconds);

            var continued = await client.PostAsJsonAsync(
                $"/sessions/{restored.SessionId}/advance",
                new AdvanceTimeRequest(60));

            Assert.Equal(HttpStatusCode.OK, continued.StatusCode);

            var original = await client.GetFromJsonAsync<SessionResponse>(
                $"/sessions/{created.SessionId}");

            Assert.NotNull(original);
            Assert.Equal(120, original.CurrentTimeSeconds);
            Assert.Equal(2, original.EventCount);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Load_UnknownArchive_ReturnsNotFound()
    {
        var directory = CreateTempDirectory();

        try
        {
            await using var factory = CreateFactory(directory);
            using var client = factory.CreateClient();

            var response = await client.PostAsync(
                $"/archives/{Guid.NewGuid()}/load",
                null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static WebApplicationFactory<Program> CreateFactory(
        string directory)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.UseSetting(
                    "Est:ArchiveDirectory",
                    directory));
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "Est.Api.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);
        return directory;
    }
}
