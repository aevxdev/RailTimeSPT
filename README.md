
# RailTime

Client-side BepInEx plugin for SPT 4.1.6. Displays the armored train arrival and departure countdown in the extraction list, with an optional screen overlay.

- **Plugin GUID:** `com.blackwell.railtime`
- **Plugin name:** `RailTime`
- **Version:** `1.0.0`
- **Assembly:** `RailTime.dll`
- **Namespace:** `Blackwell.RailTime`
- **Author:** `blackwell`

## Features

- Russian language by default; English is also supported.
- Extraction list displays:
  - `прибудет через` — train is approaching.
  - `отправится через` — train is at the station.
  - `уже уехал` — train has departed.
- Optional on-screen overlay.
- Four overlay positions: TopLeft, TopRight, BottomLeft, BottomRight.
- Configurable overlay font size.
- Client-side only. No server files are modified.

## Configuration

Settings are available through the BepInEx configuration system.

- **General:** enable or disable the plugin, select language.
- **ExitList:** show or hide the train timer in the extraction list.
- **Overlay:** enable the overlay, change its position, font size and toggle key.

The overlay is disabled by default.

## Build

Requires .NET Framework 4.7.2 targeting pack / MSBuild and an SPT 4.1.6 installation.

The project defaults `SPTPath` to `D:\SPT`. If SPT is installed elsewhere, pass the path explicitly:

```powershell
dotnet build .\RailTime.csproj -c Release -p:SPTPath="D:\SPT"
```

The build uses game and BepInEx assemblies from the local SPT installation and does not bundle them.

## Installation

1. Download `RailTime.dll`.
2. Create the following folder if it does not exist:

```text
BepInEx/
└── plugins/
    └── RailTime/
        └── RailTime.dll
```

3. Launch SPT.

The plugin configuration will be generated automatically.

## Compatibility

- Supported SPT version: **4.1.6**
- Requires BepInEx.
- Client-side plugin.
