using TimeTracker.Core;
using Xunit;

namespace TimeTracker.Core.Tests;

public class TimeTrackingTests
{
    private static readonly DateTimeOffset T0 = new(2026, 9, 30, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void StopwatchRecordsAndSwitchesTasks()
    {
        var sw = new Stopwatch();
        Assert.Null(sw.Stop(T0));
        sw.Start(" Website ", T0);
        Assert.Equal(TimeSpan.FromMinutes(10), sw.Elapsed(T0.AddMinutes(10)));
        sw.Start("Mobile", T0.AddMinutes(30)); // switching stops the first timer
        var e = sw.Stop(T0.AddMinutes(45));
        Assert.Equal(2, sw.Entries.Count);
        Assert.Equal("Website", sw.Entries[0].Project);
        Assert.Equal(TimeSpan.FromMinutes(30), sw.Entries[0].Duration);
        Assert.Equal(TimeSpan.FromMinutes(15), e!.Duration);
        Assert.False(sw.IsRunning);
    }

    [Fact]
    public void StopwatchRejectsBadInput()
    {
        var sw = new Stopwatch();
        Assert.Throws<ArgumentException>(() => sw.Start("  ", T0));
        sw.Start("x", T0);
        Assert.Throws<ArgumentException>(() => sw.Stop(T0.AddMinutes(-1)));
        Assert.Null(sw.Stop(T0)); // zero-length session discarded
        Assert.Empty(sw.Entries);
    }

    [Fact]
    public void ManualEntriesCannotOverlap()
    {
        var list = new List<TimeEntry>();
        Reports.AddManual(list, new TimeEntry("a", T0, T0.AddHours(1)));
        Reports.AddManual(list, new TimeEntry("b", T0.AddHours(1), T0.AddHours(2))); // touching is fine
        Assert.Throws<InvalidOperationException>(() => Reports.AddManual(list, new TimeEntry("c", T0.AddMinutes(30), T0.AddMinutes(90))));
        Assert.Throws<ArgumentException>(() => Reports.AddManual(list, new TimeEntry("d", T0.AddHours(5), T0.AddHours(5))));
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void TotalsByProjectIgnoresCase()
    {
        var totals = Reports.TotalsByProject(new[]
        {
            new TimeEntry("Web", T0, T0.AddMinutes(30)),
            new TimeEntry("web", T0.AddHours(1), T0.AddHours(2)),
            new TimeEntry("App", T0.AddHours(3), T0.AddHours(3.5)),
        });
        Assert.Equal(("Web", TimeSpan.FromMinutes(90)), totals[0]);
        Assert.Equal(("App", TimeSpan.FromMinutes(30)), totals[1]);
    }

    [Fact]
    public void TotalsByDaySplitsAtLocalMidnight()
    {
        var bangkok = TimeZoneInfo.CreateCustomTimeZone("ICT", TimeSpan.FromHours(7), "ICT", "ICT");
        // 16:00Z-18:00Z = 23:00-01:00 in UTC+7
        var e = new TimeEntry("late", new DateTimeOffset(2026, 9, 30, 16, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 9, 30, 18, 0, 0, TimeSpan.Zero));
        var days = Reports.TotalsByDay(new[] { e }, bangkok);
        Assert.Equal(TimeSpan.FromHours(1), days[new DateOnly(2026, 9, 30)]);
        Assert.Equal(TimeSpan.FromHours(1), days[new DateOnly(2026, 10, 1)]);
    }

    [Fact]
    public void RoundingAndBilling()
    {
        var q = TimeSpan.FromMinutes(15);
        Assert.Equal(TimeSpan.FromMinutes(15), Reports.RoundUp(TimeSpan.FromMinutes(1), q));
        Assert.Equal(TimeSpan.FromMinutes(30), Reports.RoundUp(TimeSpan.FromMinutes(30), q));
        Assert.Equal(TimeSpan.Zero, Reports.RoundUp(TimeSpan.Zero, q));
        Assert.Throws<ArgumentOutOfRangeException>(() => Reports.RoundUp(TimeSpan.FromMinutes(1), TimeSpan.Zero));
        Assert.Equal(125.00m, Reports.Billable(TimeSpan.FromMinutes(61), 100m, q));
        Assert.Equal(33.34m, Reports.Billable(TimeSpan.FromMinutes(20), 100.01m, TimeSpan.FromMinutes(1)));
    }

    [Theory]
    [InlineData(0, "0m")]
    [InlineData(45, "45m")]
    [InlineData(65, "1h 05m")]
    [InlineData(600, "10h 00m")]
    public void FormatsDurations(int minutes, string expected) =>
        Assert.Equal(expected, Reports.Format(TimeSpan.FromMinutes(minutes)));
}
