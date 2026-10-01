using System.Globalization;
using TimeTracker.Core;

namespace TimeTracker;

public partial class ReportsPage : ContentPage
{
    private static readonly TimeSpan Increment = TimeSpan.FromMinutes(15);

    public ReportsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Render();
    }

    private void OnRateChanged(object? sender, TextChangedEventArgs e) => Render();

    private void Render()
    {
        var rate = Reports.ParseRate(RateEntry.Text, CultureInfo.CurrentCulture) ?? 0m;
        var totals = Reports.TotalsByProject(TrackerState.Stopwatch.Entries);
        TotalsList.Children.Clear();
        foreach (var (project, total) in totals)
        {
            var billable = Reports.Billable(total, rate, Increment);
            TotalsList.Children.Add(new Label
            {
                Text = $"{project}: {Reports.Format(total)} · {billable.ToString("N2", CultureInfo.CurrentCulture)}",
            });
        }
        EmptyLabel.IsVisible = totals.Count == 0;
    }
}
