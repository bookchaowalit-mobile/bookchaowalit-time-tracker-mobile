# Time Tracker — Mobile

.NET MAUI app for **Time Tracker** — track time per project and turn it into billable hours.

Part of [Chaowalit Greepoke](https://bookchaowalit.com)'s 101 Portfolio Projects.

## Canonical .NET MAUI template

This repo **is the canonical .NET MAUI 8 template** of `bookchaowalit-mobile`.
These repos are name-substituted copies of its layout, build setup and CI
(consolidation or archiving pending owner decision D5 / BD-033):

- [`commerce-ticket-booking-mobile`](https://github.com/bookchaowalit-mobile/commerce-ticket-booking-mobile)
- [`ai-text-summarizer-mobile`](https://github.com/bookchaowalit-mobile/ai-text-summarizer-mobile)

What the product does today: [PRODUCT.md](PRODUCT.md).

## Status

- `src/TimeTracker.Core` — plain `net8.0` class library with the app's domain
  logic, covered by xUnit tests in `tests/TimeTracker.Core.Tests` (36 tests, 3 files).
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

The .NET SDK is pinned to the 8.0.4xx feature band by `global.json`. Without the
pin a newer SDK (10.x on GitHub runners) rejects the `net8.0-android` workload as
end of support (`NETSDK1202`), which is why every CI run failed from 2026-10-01
until the pin. MAUI 8 itself is past Microsoft's support window (May 2025);
upgrading to a supported MAUI version is an open owner decision.

## CI

`.github/workflows/build.yml`, fail-closed, on GitHub-hosted Ubuntu 24.04:

1. `core` — `dotnet test tests/TimeTracker.Core.Tests -c Release`
2. `app-shared` — `dotnet build TimeTracker.csproj -c Release -p:MauiSharedCheck=true` (XamlC, no workload)
3. `android` — `dotnet workload install maui-android` + `dotnet build -f net8.0-android`
4. `security` — shared `security.yml` (gitleaks, Semgrep, OSV-Scanner, Trivy) from `platform-ci-workflows` v1.1.1

## Related

- **Frontend:** [bookchaowalit-website/time-tracker-frontend](https://github.com/bookchaowalit-website/bookchaowalit-time-tracker-frontend)
- **Portfolio:** [bookchaowalit.com](https://bookchaowalit.com)

## License

MIT
