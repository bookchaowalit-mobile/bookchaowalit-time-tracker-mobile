using TimeTracker.Core;
using Xunit;

namespace TimeTracker.Core.Tests;

public class EdgeCaseTests
{
    private static readonly DateTimeOffset T0 = new(2026, 9, 30, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void StopKeepsNoteAndRestartingSameProjectRecordsAnEntry()
    {
        var sw = new Stopwatch();
        sw.Start("Web", T0);
        sw.Start("Web", T0.AddMinutes(20)); // same project: closes the first session
        var e = sw.Stop(T0.AddMinutes(30), "review");
        Assert.Equal(2, sw.Entries.Count);
        Assert.Equal("review", e!.Note);
        Assert.Equal("", sw.Entries[0].Note);
        Assert.Null(sw.RunningProject);
        Assert.Equal(TimeSpan.Zero, sw.Elapsed(T0.AddHours(5)));
    }

    [Fact]
    public void ManualEntryThatContainsAnExistingOneIsRejected()
    {
        var list = new List<TimeEntry> { new("a", T0.AddMinutes(10), T0.AddMinutes(20)) };
        var ex = Assert.Throws<InvalidOperationException>(() => Reports.AddManual(list, new TimeEntry("b", T0, T0.AddHours(1))));
        Assert.Contains("a", ex.Message);
        Assert.Throws<ArgumentException>(() => Reports.AddManual(list, new TimeEntry("c", T0.AddHours(2), T0.AddHours(1))));
    }

    [Fact]
    public void TotalsByProjectBreaksTiesByName()
    {
        var totals = Reports.TotalsByProject(new[]
        {
            new TimeEntry("Zeta", T0, T0.AddMinutes(30)),
            new TimeEntry("Alpha", T0.AddHours(1), T0.AddHours(1.5)),
        });
        Assert.Equal(new[] { "Alpha", "Zeta" }, totals.Select(t => t.Project));
        Assert.Empty(Reports.TotalsByProject(Array.Empty<TimeEntry>()));
    }

    [Fact]
    public void TotalsByDaySplitsSessionsSpanningSeveralDays()
    {
        var utc = TimeZoneInfo.Utc;
        var e = new TimeEntry("marathon", new DateTimeOffset(2026, 9, 29, 20, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 10, 1, 2, 0, 0, TimeSpan.Zero));
        var days = Reports.TotalsByDay(new[] { e }, utc);
        Assert.Equal(new[] { new DateOnly(2026, 9, 29), new DateOnly(2026, 9, 30), new DateOnly(2026, 10, 1) }, days.Keys);
        Assert.Equal(TimeSpan.FromHours(4), days[new DateOnly(2026, 9, 29)]);
        Assert.Equal(TimeSpan.FromHours(24), days[new DateOnly(2026, 9, 30)]);
        Assert.Equal(TimeSpan.FromHours(2), days[new DateOnly(2026, 10, 1)]);
        Assert.Equal(e.Duration, days.Values.Aggregate(TimeSpan.Zero, (a, b) => a + b));
    }

    [Fact]
    public void TotalsByDayCountsRealElapsedTimeOnDstChangeDays()
    {
        // Europe/London springs forward on 2026-03-29 at 01:00 -> 02:00, so that local day has 23 hours.
        if (!TimeZoneInfo.TryFindSystemTimeZoneById("Europe/London", out var london)) return; // no tz database on host
        var start = new DateTimeOffset(2026, 3, 29, 0, 0, 0, TimeSpan.Zero);  // 00:00 GMT
        var end = new DateTimeOffset(2026, 3, 29, 23, 0, 0, TimeSpan.Zero);   // 00:00 BST next day
        var days = Reports.TotalsByDay(new[] { new TimeEntry("x", start, end) }, london);
        Assert.Single(days);
        Assert.Equal(TimeSpan.FromHours(23), days[new DateOnly(2026, 3, 29)]);
    }

    [Fact]
    public void BillingEdgeCases()
    {
        var q = TimeSpan.FromMinutes(15);
        Assert.Equal(0m, Reports.Billable(TimeSpan.FromHours(3), 0m, q));
        Assert.Equal(0m, Reports.Billable(TimeSpan.Zero, 100m, q));
        Assert.Throws<ArgumentOutOfRangeException>(() => Reports.Billable(TimeSpan.FromHours(1), -1m, q));
        Assert.Equal(TimeSpan.Zero, Reports.RoundUp(TimeSpan.FromMinutes(-5), q));
        Assert.Throws<ArgumentOutOfRangeException>(() => Reports.RoundUp(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(-15)));
        // 7 min at 10.02/h = 1.169 -> 1.17
        Assert.Equal(1.17m, Reports.Billable(TimeSpan.FromMinutes(7), 10.02m, TimeSpan.FromMinutes(1)));
    }

    [Theory]
    [InlineData(-30, "0m")]
    [InlineData(59.99, "59m")]
    [InlineData(60, "1h 00m")]
    [InlineData(1500, "25h 00m")]
    public void FormatFloorsAndClamps(double minutes, string expected) =>
        Assert.Equal(expected, Reports.Format(TimeSpan.FromMinutes(minutes)));
}
