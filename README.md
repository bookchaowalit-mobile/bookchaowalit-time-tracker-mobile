# Time Tracker — Mobile

.NET MAUI app for **Time Tracker** — track time per project and turn it into billable hours.

Part of [Chaowalit Greepoke](https://bookchaowalit.com)'s 101 Portfolio Projects.

## Status

- `src/TimeTracker.Core` — plain `net8.0` class library with the app's domain
  logic, covered by xUnit tests in `tests/TimeTracker.Core.Tests` (20 tests).
- `TimeTracker.csproj` + XAML at the repo root — the .NET MAUI 8 app in the standard
  `dotnet new maui` layout (`App.xaml`, `AppShell.xaml`, `MauiProgram.cs`,
  `Platforms/Android`, `Platforms/iOS`, `Resources/`). Two tabs: **Timer** (start/switch/stop, live elapsed time, entry list) and **Reports** (totals per project with billable amount at an hourly rate, 15-minute increments).
  State is in memory for now (see [docs/UPGRADE-PLAN.md](docs/UPGRADE-PLAN.md)).
- Verified locally without the MAUI workload: the shared XAML/C# compiles
  against `Microsoft.Maui.Controls` 8 with XamlC (`-p:MauiSharedCheck=true`,
  which builds a plain `net8.0` target and skips `Platforms/`). The Android/iOS
  heads are built by CI (`android` job) only; they have not been built locally.

## Core features (`TimeTracker.Core`)

- Single running timer; starting another project stops the current one; zero-length sessions are discarded
- Manual entries with overlap detection
- Totals per project (case-insensitive) and per local day, splitting sessions that cross midnight
- Round-up to billing increments and billable amount (`decimal`, away-from-zero rounding)
- Duration formatting (`1h 05m`)

## Tech Stack

- **Core:** C# / .NET 8 class library (nullable enabled, warnings as errors)
- **Tests:** xUnit
- **App:** .NET MAUI 8 — Android, iOS

## Getting Started

```bash
dotnet test tests/TimeTracker.Core.Tests                                    # core + unit tests
dotnet build TimeTracker.csproj -c Release -p:MauiSharedCheck=true          # app XAML/C# check, no workload needed
dotnet workload install maui-android && dotnet build TimeTracker.csproj -f net8.0-android   # Android app (needs Android SDK + JDK 17)
```

On Linux only the Android target is enabled; macOS/Windows also get `net8.0-ios`.

## Related

- **Frontend:** [bookchaowalit-website/time-tracker-frontend](https://github.com/bookchaowalit-website/bookchaowalit-time-tracker-frontend)
- **Portfolio:** [bookchaowalit.com](https://bookchaowalit.com)

## License

MIT
