# BlitzAdBlocker

Removes ads from the [Blitz](https://blitz.gg) app.

Blitz shows ads unless you're a premium user. BlitzAdBlocker blocks the ad
networks Blitz loads from — Aditude, Google AdX, prebid bidders, video
networks and DMP syncs — so the ads never load. Blitz itself is never
modified.

Buttons:

- **Enable** — adds the ad-network domains to the hosts file
- **Disable** — removes them again

The hosts file is backed up (`hosts.blitzadblock.bak`) before every change.
Blitz's own "Get Premium" banner is left alone — it comes from Blitz itself,
not an ad network.

## Why the hosts file

Blitz premium status is verified on Blitz's servers, so there is no local
flag that disables ads. Blitz does load its ads from distinct third-party
domains, and blocking those at the hosts level kills every ad request without
touching the app.

## Building from source

Requires the [.NET SDK](https://dotnet.microsoft.com/download) (8.0 or newer):

```sh
dotnet build src/BlitzAdBlocker/BlitzAdBlocker.csproj -c Release
```

Output: `src/BlitzAdBlocker/bin/Release/net48/BlitzAdBlocker.exe` (a single
exe, ~30 KB, for Windows 10/11 — .NET Framework 4.8 ships with Windows, nothing
to install).

Fonts: Livvic (titles, buttons, status) and Inter (details), both embedded.

Releases are built by GitHub Actions and ship with a sha256 checksum.