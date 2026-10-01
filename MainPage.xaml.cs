using TimeTracker.Core;

namespace TimeTracker;

public partial class MainPage : ContentPage
{
    private readonly IDispatcherTimer _tick;

    public MainPage()
    {
        InitializeComponent();
        _tick = Dispatcher.CreateTimer();
        _tick.Interval = TimeSpan.FromSeconds(1);
        _tick.Tick += (_, _) => Refresh();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _tick.Start();
        Refresh();
    }

    protected override void OnDisappearing()
    {
        _tick.Stop();
        base.OnDisappearing();
    }

    private void OnStartClicked(object? sender, EventArgs e)
    {
        try
        {
            TrackerState.Stopwatch.Start(ProjectEntry.Text ?? string.Empty, DateTimeOffset.Now);
            ErrorLabel.IsVisible = false;
        }
        catch (ArgumentException ex)
        {
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
        }
        Refresh();
    }

    private void OnStopClicked(object? sender, EventArgs e)
    {
        TrackerState.Stopwatch.Stop(DateTimeOffset.Now);
        Refresh();
    }

    private void Refresh()
    {
        var sw = TrackerState.Stopwatch;
        StatusLabel.Text = sw.IsRunning ? $"Tracking: {sw.RunningProject}" : "Stopped";
        ElapsedLabel.Text = Reports.Format(sw.Elapsed(DateTimeOffset.Now));
        StopButton.IsEnabled = sw.IsRunning;
        StartButton.Text = sw.IsRunning ? "Switch" : "Start";

        if (EntriesList.Children.Count == sw.Entries.Count) return;
        EntriesList.Children.Clear();
        foreach (var entry in sw.Entries.Reverse())
        {
            EntriesList.Children.Add(new Label
            {
                Text = $"{entry.Project} · {entry.Start.ToLocalTime():HH:mm}–{entry.End.ToLocalTime():HH:mm} · {Reports.Format(entry.Duration)}",
            });
        }
    }
}
