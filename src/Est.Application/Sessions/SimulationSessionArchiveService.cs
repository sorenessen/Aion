using Est.Persistence.Archives;
using Est.Persistence.Storage;
using Est.Simulation.Timelines;

namespace Est.Application.Sessions;

public sealed class SimulationSessionArchiveService
{
    private readonly SimulationSessionManager _manager;
    private readonly TimelineArchiveFileStore _store;

    public SimulationSessionArchiveService(
        SimulationSessionManager manager,
        TimelineArchiveFileStore store)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public SimulationTimeline Save(
        SimulationSessionId sessionId,
        string path,
        TimelineArchiveProvenance provenance)
    {
        var session = _manager.Get(sessionId);
        var timeline = session.Timeline;

        _store.SaveNew(
            path,
            timeline,
            session.Definition,
            provenance);

        return timeline;
    }

    public SimulationSessionId Load(string path)
    {
        var archive = _store.Load(path);

        return _manager.Restore(
            archive.Timeline,
            archive.Definition);
    }
}
