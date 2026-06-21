# RynthRemote

A phone app (iOS, .NET MAUI Blazor) that **monitors and remote-controls your Asheron's Call
multi-boxes** through the [RynthCore](https://github.com/tombohar) StatusAgent running on your PC.

It's a clean standalone extraction of the "AC Clients" dashboard from the personal MicroManager
app — none of the finance/cycling/database baggage, just the AC piece.

## What it shows / does

- **Live per-client status:** state, HP/Stam/Mana, bot action + target, fps + pump rate, stale flags.
- **Activity / health:** kills/hr + session kills, time-since-last-kill, XP/hr, deaths, vitae %,
  burden %, free pack slots, current area, last engine warning/error.
- **Casting components:** per-tier scarab counts + prismatic tapers (amber when low).
- **Equipped gear + full appraisal:** every worn item with its instance ID; tap for armor level +
  resistances/banes, weapon damage/defenses, workmanship, value, mana, item spells, and description.
- **Remote control:** module toggles (Nav/Combat/Buffing/Looting/Meta), macro Start/Stop, nav/loot/meta
  profile pickers, and utilities (clear busy, force rebuff, cancel).

## How it connects

```
Phone (RynthRemote)  --HTTP-->  RynthCore.StatusAgent (your PC)  <--file-->  RynthCore engine/plugin
        GET /status   (poll)
        POST /command (control)
```

The app is a thin client: it only talks to **your own** StatusAgent. It sends nothing anywhere else.

### PC side (one-time)
1. Run RynthCore with `EnableStatusExport: true` (and `EnableRemoteControl: true` for the toggles)
   in `%APPDATA%\RynthCore\engine.json` / `statusagent.json`.
2. Run `RynthCore.StatusAgent` with `ServeHttp: true` (and `EnableRemoteControl: true`).
3. Reach your PC from the phone: same Wi-Fi (LAN IP) or a tunnel (Tailscale). See the agent docs.

### Phone side
Open RynthRemote → **Connection** → enter your agent URL (e.g. `http://your-pc:8740/status` or just
`your-pc:8740`) and an optional token → Save.

## Build

```bash
# Local compile-check (Windows head)
dotnet build src/RynthRemote/RynthRemote.csproj -f net10.0-windows10.0.19041.0

# iOS unsigned IPA — produced in CI on a macOS runner (.github/workflows/ios-build.yml).
```

## Distribution

CI builds an **unsigned IPA** on every push to `main`. Install paths:
- **SideStore/AltStore** (free): point the source at the published `apps.json` feed. The publish step
  activates once a `RELEASES_TOKEN` secret + a `tombohar/rynthremote-releases` repo exist.
- **TestFlight** (needs the $99 Apple Developer Program): re-sign the IPA and upload — no VPN dance,
  90-day builds, public install link. Recommended once distributing more widely.
