namespace Est.Persistence.Archives;

public sealed record TimelineArchiveProvenance
{
    public TimelineArchiveProvenance(
        string producer,
        string producerVersion,
        string origin)
    {
        if (string.IsNullOrWhiteSpace(producer))
        {
            throw new ArgumentException(
                "Archive producer cannot be empty.",
                nameof(producer));
        }

        if (string.IsNullOrWhiteSpace(producerVersion))
        {
            throw new ArgumentException(
                "Archive producer version cannot be empty.",
                nameof(producerVersion));
        }

        if (string.IsNullOrWhiteSpace(origin))
        {
            throw new ArgumentException(
                "Archive origin cannot be empty.",
                nameof(origin));
        }

        Producer = producer;
        ProducerVersion = producerVersion;
        Origin = origin;
    }

    public string Producer { get; }

    public string ProducerVersion { get; }

    public string Origin { get; }
}
