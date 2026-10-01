using TimeTracker.Core;

namespace TimeTracker;

/// <summary>
/// In-memory app state shared by the pages. Persistence (SQLite) is a
/// backlog item; until then entries live for the app session only.
/// </summary>
public static class TrackerState
{
    public static Stopwatch Stopwatch { get; } = new();
}
