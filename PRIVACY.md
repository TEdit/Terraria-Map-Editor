# TEdit Privacy Policy

TEdit collects **no usage analytics, no personal information, and no telemetry by default**. The only data ever transmitted is anonymous crash reports, and only after you explicitly opt in.

## What is collected

TEdit uses [Microsoft Application Insights](https://learn.microsoft.com/azure/azure-monitor/app/app-insights-overview) to send **exception reports** when the application crashes or encounters an unhandled error. Nothing else is tracked — there are no `TrackEvent`, `TrackPageView`, `TrackMetric`, or `TrackRequest` calls anywhere in the codebase.

Each exception report contains:

| Field | Value |
|-------|-------|
| Exception type, message, and stack trace | The .NET exception details, with the username segment of any `C:\Users\<name>\` path in the message scrubbed to `C:\Users\[user]\` before sending |
| `AppVersion` | The TEdit version string (e.g. `4.20.0`) |
| `OperatingSystem` | `Environment.OSVersion` — the Windows version and build number only (e.g. `Microsoft Windows NT 10.0.26100.0`). **Your machine name is not included in this field.** |
| `RoleInstance` | The literal string `"TEdit-Wpf"` |
| `SessionId` | A random GUID generated fresh each time TEdit launches. It is **not persisted** to disk and cannot be correlated across sessions or with you personally. |
| `User.Id` | The literal string `"TEdit"` (the same value for every user — not a user identifier) |

Application Insights ingestion adds standard envelope metadata such as a timestamp and the source IP address (Microsoft truncates IPs at ingestion in accordance with its [data privacy practices](https://learn.microsoft.com/azure/azure-monitor/app/data-retention-privacy)).

## What is NOT collected

- World files, schematic files, or any of their contents
- File paths to your worlds, schematics, or Terraria installation
- Your Windows username, real name, or email address
- Your machine name / computer name (not transmitted in any field TEdit sets)
- Your Steam ID, Terraria save path, or any other configured path
- Mouse, keyboard, click, or tool-usage analytics
- Any cross-session identifier — `SessionId` is regenerated every launch and never written to disk
- Advertising identifiers or device fingerprinting

## Anonymization

Before any error message is recorded, TEdit applies a regular expression that replaces the Windows username segment of paths matching `C:\Users\<name>\` with `C:\Users\[user]\`. This applies to:

- **Local log files** — the full log message including stack trace is scrubbed before being written to disk.
- **Telemetry exception reports** — the exception message is scrubbed before being sent to Application Insights.

Code reference: `src/TEdit/ErrorLogging.cs` — see `UserPathRegex` and the `Log` and `LogException` methods.

> ⚠️ The scrubber targets the `C:\Users\<name>\` pattern. Stack-trace frames that reference assembly-relative paths or paths under other roots (e.g. `D:\…`) are sent as-is. In release builds, stack-trace paths typically reference TEdit's own install location and do not contain your username.

## How to disable

Telemetry is **opt-in**. On first launch TEdit shows a prompt asking whether to enable anonymous error reporting; if you decline, nothing is ever transmitted. You can change your choice at any time:

1. Open **Settings** (the gear icon).
2. Go to the **Privacy** category.
3. Toggle **Enable Error Reporting** off (or on).

The change takes effect immediately — the in-memory telemetry client is torn down or rebuilt the moment the toggle is flipped.

Alternatively you can edit your TEdit user settings JSON directly and set:

```json
"Telemetry": 0
```

Values: `-1` = not yet asked (default on first run), `0` = disabled, `1` = enabled.

## Local logs

Independent of telemetry, TEdit writes local diagnostic logs to:

```
%TEMP%\TEdit\Logs\TEdit_<timestamp>.txt
```

These logs:

- Stay on your machine and are **never transmitted** to TEdit, Microsoft, or anyone else.
- Have the same username scrubbing applied before each line is written.
- Roll over at 100 MB per file.
- Are automatically deleted after 7 days.

You can open the current log from the application via **Help → View Log**.

## Where the data goes

Exception reports are sent over HTTPS to Microsoft's Application Insights ingestion endpoint (`dc.services.visualstudio.com`) and stored in the TEdit project's Application Insights workspace. Retention follows Microsoft's default Application Insights policy. The data is reviewed only to diagnose crashes and improve stability.

## Changes to this policy

If the data collected or the anonymization behavior changes in a future release, the first-run prompt will re-appear so you can re-confirm your choice for the new version.

## Contact

Issues or questions about this policy: <https://github.com/TEdit/Terraria-Map-Editor/issues>
