using Aion.Persistence.Archives;
using Aion.Simulation.Timelines;

namespace Aion.Persistence.Storage;

public sealed class TimelineArchiveFileStore
{
    public void Save(
        string path,
        SimulationTimeline timeline,
        TimelineArchiveProvenance provenance)
    {
        SaveCore(
            path,
            timeline,
            provenance,
            overwrite: true);
    }

    public void SaveNew(
        string path,
        SimulationTimeline timeline,
        TimelineArchiveProvenance provenance)
    {
        SaveCore(
            path,
            timeline,
            provenance,
            overwrite: false);
    }

    private static void SaveCore(
        string path,
        SimulationTimeline timeline,
        TimelineArchiveProvenance provenance,
        bool overwrite)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "Archive path cannot be empty.",
                nameof(path));
        }

        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(provenance);

        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);

        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException(
                "Archive path did not resolve to a directory.");
        }

        Directory.CreateDirectory(directory);

        var json =
            TimelineArchiveSerializer.Serialize(
                timeline,
                provenance);

        var tempPath =
            fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";

        try
        {
            File.WriteAllText(
                tempPath,
                json);

            File.Move(
                tempPath,
                fullPath,
                overwrite);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    public TimelineArchive Load(
        string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "Archive path cannot be empty.",
                nameof(path));
        }

        var fullPath = Path.GetFullPath(path);

        var json =
            File.ReadAllText(fullPath);

        return TimelineArchiveSerializer.Deserialize(
            json);
    }
}
