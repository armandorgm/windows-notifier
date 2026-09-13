# Windows Notifier

Windows 11 WPF overlay that shows real-time trading order events. It listens on a local TCP port and can look up related prices in PostgreSQL or SQLite.

## Requirements

- .NET 9 SDK
- Windows (WPF)

## Setup

1. Copy `settings.example.json` to `settings.json` and fill in your local database connection.
2. Build and run:

```bash
dotnet restore
dotnet run
```

## Mock events

With the overlay running, send sample events:

```bash
python mock_sender.py
```
