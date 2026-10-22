# Upgrade plan

## Current state

Score: 6/10 (was 4/10) — app now has the full MAUI layout and its shared XAML/C#
compiles (XamlC) against the core; platform heads are CI-built only, no persistence yet.

## Backlog

- P0 (done 2026-10-22): `android` CI job is green after pinning the .NET 8 SDK in
  `global.json` (it failed with NETSDK1202 under the runner's default .NET 10 SDK).
- P1 (owner decision): move off MAUI 8, which is past its support window.
- P1: Persist entries (SQLite via `sqlite-net-pcl`) behind an interface in core;
  today entries live only for the app session.
- P1: Weekly report view and CSV export; manual entry form using `Reports.AddManual`.
- P2: Build `net8.0-ios` on a macOS runner.
- P2: Add a solution file or `Directory.Build.props` to share analyzer settings
  between core and app.

## Done in this pass (pass 1)

- Added `src/TimeTracker.Core` with the app's domain logic and 10 xUnit tests.
- App project now references the core library and excludes `src/**`/`tests/**` from its globs.
- CI: removed the `|| true` steps that hid a non-compiling app; runs the core tests for real.
- README now states what is verified and what is not.

## Done in this pass (pass 2)

- Scaffolded the standard `dotnet new maui` (.NET 8) files wired to the core:
  `App.xaml`, `AppShell.xaml.cs`, `MauiProgram.cs`, `Platforms/Android`
  (`MainActivity`, `MainApplication`, manifest without INTERNET), `Platforms/iOS`
  (`AppDelegate`, `Program`, `Info.plist`), `Resources/` icon + splash SVGs.
- `RootNamespace` now matches the C# namespace; added `ApplicationId`, versions,
  `ImplicitUsings`/`Nullable`, and a unique `ApplicationIdGuid`.
- Replaced the missing `ExplorePage`/`ProfilePage` with real tabs: Timer and Reports, both using `TimeTracker.Core`.
- 10 more edge-case tests (multi-day split, DST day in Europe/London, billing/rounding bounds, formatting clamps): 20 total.
- `MauiSharedCheck=true` build switch: compiles the shared XAML/C# as `net8.0`
  without the workload (verified locally, Release/XamlC, 0 warnings).
- CI: new `app-shared` job (that check) and `android` job (`maui-android` workload build).

## Done in this pass (pass 3)

- Fixed hourly-rate parsing on the Reports tab: `NumberStyles.Number` accepts group separators anywhere, so "1,5" typed in en-US billed at 15/h (and "1.5" in de-DE at 15/h). New `Reports.ParseRate` requires real 3-digit groups, rejects signs/garbage, and is unit-tested per culture.
- Project names: zero-width-only names (U+200B/U+2060/BOM) passed `IsNullOrWhiteSpace`; `Projects.Normalize` strips them and is now also applied to `Reports.AddManual`, which previously accepted blank projects and kept " Acme " separate from "Acme" in totals.
- Pinned day splitting when DST skips local midnight (America/Santiago) — already correct.
- 13 new test cases (36 total), verified with `dotnet test tests/TimeTracker.Core.Tests` and `dotnet build TimeTracker.csproj -c Release -p:MauiSharedCheck=true`.
