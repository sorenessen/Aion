namespace Est.Api;

public sealed class SessionArchiveLocation
{
    public SessionArchiveLocation(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException(
                "Archive root directory cannot be empty.",
                nameof(rootDirectory));

        RootDirectory = Path.GetFullPath(rootDirectory);
    }

    public string RootDirectory { get; }

    public string GetPath(Guid archiveId)
    {
        if (archiveId == Guid.Empty)
            throw new ArgumentException(
                "Archive identity cannot be empty.",
                nameof(archiveId));

        return Path.Combine(
            RootDirectory,
            archiveId.ToString("N") + ".json");
    }
}
