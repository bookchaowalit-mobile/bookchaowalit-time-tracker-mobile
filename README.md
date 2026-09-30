# Time Tracker — Mobile

.NET MAUI app for **Time Tracker** — track time per project and turn it into billable hours.

Part of [Chaowalit Greepoke](https://bookchaowalit.com)'s 101 Portfolio Projects.

## Status

- `src/TimeTracker.Core` — plain `net8.0` class library with the app's domain
  logic, covered by xUnit tests in `tests/TimeTracker.Core.Tests`. **This is the
  verified part of the repo** and what CI builds.
- `TimeTracker.csproj` + XAML at the repo root — the MAUI app shell. It references
  `TimeTracker.Core` but **does not compile yet**: `App.xaml`, `AppShell.xaml.cs`,
  `MauiProgram.cs`, the `Platforms/` folders and the `ExplorePage` /
  `ProfilePage` referenced from `AppShell.xaml` are missing. See
  [docs/UPGRADE-PLAN.md](docs/UPGRADE-PLAN.md).

## Core features (`TimeTracker.Core`)

- Single running timer; starting another project stops the current one; zero-length sessions are discarded
- Manual entries with overlap detection
- Totals per project (case-insensitive) and per local day, splitting sessions that cross midnight
- Round-up to billing increments and billable amount (`decimal`, away-from-zero rounding)
- Duration formatting (`1h 05m`)

## Tech Stack

- **Core:** C# / .NET 8 class library (nullable enabled, warnings as errors)
- **Tests:** xUnit
- **App (planned):** .NET MAUI 8 — Android, iOS

## Getting Started

```bash
dotnet test tests/TimeTracker.Core.Tests      # build core + run unit tests
```

The MAUI app additionally needs `dotnet workload install maui` plus the
Android SDK / Xcode once the missing app files are added.

## Related

- **Frontend:** [bookchaowalit-website/time-tracker-frontend](https://github.com/bookchaowalit-website/bookchaowalit-time-tracker-frontend)
- **Portfolio:** [bookchaowalit.com](https://bookchaowalit.com)

## License

MIT
