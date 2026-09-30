using System.Globalization;
using TimeTracker.Core;
using Xunit;

namespace TimeTracker.Core.Tests;

public class Pass3Tests
{
    private static readonly DateTimeOffset T0 = new(2026, 9, 30, 9, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("​")]
    [InlineData("﻿ ⁠")]
    public void ZeroWidthProjectNameIsRejected(string name)
    {
        Assert.Throws<ArgumentException>(() => new Stopwatch().Start(name, T0));
    }

    [Fact]
    public void ProjectNamesDropZeroWidthCharacters()
    {
        var sw = new Stopwatch();
        sw.Start("﻿Acme​", T0);
        Assert.Equal("Acme", sw.RunningProject);
    }

    [Fact]
    public void ManualEntryProjectIsValidatedAndNormalisedLikeTheTimer()
    {
        var entries = new List<TimeEntry>();
        Assert.Throws<ArgumentException>(() => Reports.AddManual(entries, new TimeEntry(" ", T0, T0.AddHours(1))));
        Assert.Throws<ArgumentException>(() => Reports.AddManual(entries, new TimeEntry("​", T0, T0.AddHours(1))));
        Reports.AddManual(entries, new TimeEntry(" Acme ", T0, T0.AddHours(1)));
        Reports.AddManual(entries, new TimeEntry("acme", T0.AddHours(1), T0.AddHours(2)));
        var totals = Reports.TotalsByProject(entries);
        Assert.Single(totals);
        Assert.Equal(("Acme", TimeSpan.FromHours(2)), totals[0]);
    }

    [Fact]
    public void MidnightSkippedByDstStillSplitsDaysCorrectly()
    {
        // Chile springs forward at local midnight: Sunday 2026-09-06 starts at 01:00 -03.
        if (!TimeZoneInfo.TryFindSystemTimeZoneById("America/Santiago", out var santiago)) return;
        var start = new DateTimeOffset(2026, 9, 5, 22, 0, 0, TimeSpan.FromHours(-4));
        var end = new DateTimeOffset(2026, 9, 6, 2, 0, 0, TimeSpan.FromHours(-3));
        var days = Reports.TotalsByDay(new[] { new TimeEntry("P", start, end) }, santiago);
        Assert.Equal(TimeSpan.FromHours(2), days[new DateOnly(2026, 9, 5)]);
        Assert.Equal(TimeSpan.FromHours(1), days[new DateOnly(2026, 9, 6)]);
    }

    [Theory]
    [InlineData("en-US", "1,5")]
    [InlineData("en-US", "1,2345")]
    [InlineData("en-US", "-5")]
    [InlineData("en-US", "12abc")]
    [InlineData("en-US", "")]
    [InlineData("de-DE", "1.5")]
    public void RateParsingRejectsAmbiguousOrInvalidInput(string culture, string text)
    {
        Assert.Null(Reports.ParseRate(text, CultureInfo.GetCultureInfo(culture)));
    }

    [Theory]
    [InlineData("en-US", "1,500.50", "1500.50")]
    [InlineData("en-US", " 45 ", "45")]
    [InlineData("de-DE", "1.500,5", "1500.5")]
    [InlineData("de-DE", "45,5", "45.5")]
    [InlineData("th-TH", "1,200", "1200")]
    public void RateParsingAcceptsLocaleNumbers(string culture, string text, string expected)
    {
        Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), Reports.ParseRate(text, CultureInfo.GetCultureInfo(culture)));
    }
}
