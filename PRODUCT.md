# Time Tracker (mobile) — product notes

Describes only what exists in this repository today (2026-10-22).

## What it is

A .NET MAUI 8 app that times work per project and turns the time into
billable amounts. It is also the **canonical .NET MAUI template** of the
`bookchaowalit-mobile` portfolio; `commerce-ticket-booking-mobile` and
`ai-text-summarizer-mobile` are copies of its layout and CI.

## Screens (2 tabs, `AppShell`)

- **Timer** (`MainPage`): project name field, Start / Stop buttons, status,
  live elapsed time, list of finished entries. Starting a different project
  stops the running one; zero-length sessions are dropped.
- **Reports** (`ReportsPage`): totals per project and the billable amount at
  an hourly rate typed by the user (locale-aware number parsing), billed in
  15-minute increments rounded up.

## Data

- All state is in memory (`TrackerState`), for the app session only. There is
  no persistence, no account, no backend, no network use (the Android manifest
  does not request INTERNET) and no analytics.

## Domain logic (`src/TimeTracker.Core`, plain `net8.0`)

Single running timer, manual entries with overlap detection, totals per
project (case-insensitive) and per local day (sessions crossing midnight are
split), round-up to billing increments, `decimal` billable amounts, duration
formatting. Covered by 36 xUnit tests in `tests/TimeTracker.Core.Tests`
(3 files: `TimeTrackingTests.cs`, `EdgeCaseTests.cs`, `Pass3Tests.cs`).

## Build and distribution status

- CI builds the core and tests, compiles the shared XAML with XamlC, and
  builds the Android app (Debug) with the `maui-android` workload on the
  .NET 8 SDK pinned by `global.json`.
- iOS is not built anywhere yet (needs a macOS runner).
- No signed release build, no store listing, no installs.
- MAUI 8 is past Microsoft's support window (May 2025); moving to a supported
  MAUI version is an owner decision.

## Not built yet

Persistence (SQLite), manual-entry form, weekly report and CSV export, iOS
build. See [docs/UPGRADE-PLAN.md](docs/UPGRADE-PLAN.md).
