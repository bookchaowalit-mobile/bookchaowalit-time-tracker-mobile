namespace TimeTracker.Core;

/// <summary>A finished block of work on a project. End is exclusive.</summary>
public sealed record TimeEntry(string Project, DateTimeOffset Start, DateTimeOffset End, string Note = "")
{
    public TimeSpan Duration => End - Start;
}

/// <summary>Running/stopped timer. Only one timer runs at a time.</summary>
public sealed class Stopwatch
{
    private readonly List<TimeEntry> _entries = new();
    private (string Project, DateTimeOffset Start)? _running;

    public IReadOnlyList<TimeEntry> Entries => _entries;
    public bool IsRunning => _running is not null;
    public string? RunningProject => _running?.Project;

    /// <summary>Starts a timer. If another is running it is stopped first (switching task).</summary>
    public void Start(string project, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(project)) throw new ArgumentException("project is required", nameof(project));
        if (_running is not null) Stop(now);
        _running = (project.Trim(), now);
    }

    /// <summary>Stops the running timer and records the entry. Zero-length sessions are discarded.</summary>
    public TimeEntry? Stop(DateTimeOffset now, string note = "")
    {
        if (_running is not { } r) return null;
        if (now < r.Start) throw new ArgumentException("stop time is before start time", nameof(now));
        _running = null;
        if (now == r.Start) return null;
        var entry = new TimeEntry(r.Project, r.Start, now, note);
        _entries.Add(entry);
        return entry;
    }

    /// <summary>Elapsed time of the running timer, zero when stopped.</summary>
    public TimeSpan Elapsed(DateTimeOffset now) => _running is { } r ? now - r.Start : TimeSpan.Zero;
}

public static class Reports
{
    /// <summary>Adds a manual entry after validating that End &gt; Start and it overlaps nothing.</summary>
    public static void AddManual(List<TimeEntry> entries, TimeEntry entry)
    {
        if (entry.End <= entry.Start) throw new ArgumentException("end must be after start");
        var clash = entries.FirstOrDefault(e => e.Start < entry.End && entry.Start < e.End);
        if (clash is not null) throw new InvalidOperationException($"overlaps {clash.Project} {clash.Start:HH:mm}-{clash.End:HH:mm}");
        entries.Add(entry);
    }

    /// <summary>Total per project, largest first.</summary>
    public static IReadOnlyList<(string Project, TimeSpan Total)> TotalsByProject(IEnumerable<TimeEntry> entries) =>
        entries.GroupBy(e => e.Project, StringComparer.OrdinalIgnoreCase)
            .Select(g => (g.First().Project, Total: TimeSpan.FromTicks(g.Sum(e => e.Duration.Ticks))))
            .OrderByDescending(t => t.Total).ThenBy(t => t.Item1, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Time per local calendar day. Entries that cross midnight are split
    /// between the days they cover.
    /// </summary>
    public static IReadOnlyDictionary<DateOnly, TimeSpan> TotalsByDay(IEnumerable<TimeEntry> entries, TimeZoneInfo zone)
    {
        var result = new SortedDictionary<DateOnly, TimeSpan>();
        foreach (var e in entries)
        {
            var cursor = TimeZoneInfo.ConvertTime(e.Start, zone);
            var end = TimeZoneInfo.ConvertTime(e.End, zone);
            while (cursor < end)
            {
                var day = DateOnly.FromDateTime(cursor.DateTime);
                var nextMidnightLocal = day.AddDays(1).ToDateTime(TimeOnly.MinValue);
                var nextMidnight = new DateTimeOffset(nextMidnightLocal, zone.GetUtcOffset(nextMidnightLocal));
                var sliceEnd = nextMidnight < end ? nextMidnight : end;
                result[day] = result.GetValueOrDefault(day) + (sliceEnd - cursor);
                cursor = sliceEnd;
            }
        }
        return result;
    }

    /// <summary>Rounds up to the billing increment (e.g. 15 min). Zero stays zero.</summary>
    public static TimeSpan RoundUp(TimeSpan duration, TimeSpan increment)
    {
        if (increment <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(increment));
        if (duration <= TimeSpan.Zero) return TimeSpan.Zero;
        var blocks = (duration.Ticks + increment.Ticks - 1) / increment.Ticks;
        return TimeSpan.FromTicks(blocks * increment.Ticks);
    }

    /// <summary>Billable amount for a duration at an hourly rate, rounded to 2 decimals (banker's rounding avoided).</summary>
    public static decimal Billable(TimeSpan duration, decimal hourlyRate, TimeSpan increment)
    {
        if (hourlyRate < 0) throw new ArgumentOutOfRangeException(nameof(hourlyRate));
        var hours = (decimal)RoundUp(duration, increment).Ticks / TimeSpan.TicksPerHour;
        return Math.Round(hours * hourlyRate, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>"1h 05m", "45m", "0m".</summary>
    public static string Format(TimeSpan d)
    {
        var totalMinutes = (long)Math.Floor(d.TotalMinutes);
        if (totalMinutes < 0) totalMinutes = 0;
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        return h > 0 ? $"{h}h {m:00}m" : $"{m}m";
    }
}
