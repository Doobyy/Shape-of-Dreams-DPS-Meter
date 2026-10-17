# Shape of Dreams DPS Meter

A client-side combat meter for **Shape of Dreams**. It tracks damage, healing, and barrier contribution during a run, with separate views for individual and party performance.

The source is published here so players can inspect how attribution works and build the mod themselves.

## Features

- **Damage tracking:** current DPS and total damage, with breakdowns by skill and Essence.
- **Healing and barriers:** healing-per-second (HPS), barrier-per-second (BPS), totals, and source breakdowns.
- **Party comparison:** compare player contributions in DPS and total views, including each player's share of the total.
- **Run history:** retain recent run summaries, including character, outcome, run duration, and visited-world/map information.
- **Display filters:** optionally exclude overkill damage, overhealing, or damage attributed to Essence of Mortality from displayed calculations. These filters affect the meter's numbers and preserve underlying run data so totals can be recalculated when a filter changes.
- **Configurable overlay:** move the meter and settings window, choose views/metrics to show, and use the context menu for available actions such as settings, reload, export, and reset.

## Screenshots

Screenshots will be added here.

<!-- TODO: Add current screenshots of the DPS, party, settings, and run-history views. -->

## Installation

Use the mod package supplied with the release or install it through the game's supported mod/Workshop workflow when available. Follow the installation instructions included with that specific release package.

The repository is primarily the source-code project; a compiled `DPSMeter.dll` is distributed separately with a release package.

## Build from source

### Requirements

- Windows and Visual Studio/MSBuild capable of building **.NET Framework 4.8.1** projects.
- A local installation of Shape of Dreams, for the game's managed assemblies referenced by the project.

### Build

1. Clone or download this repository.
2. Open `DPSMeter.csproj` in Visual Studio.
3. If the game is installed somewhere other than the default path, update the `ShapeOfDreamsHome` MSBuild property to point to its installation directory.
4. Select **Release** and build the project.
5. The compiled assembly is written to `bin/Release/DPSMeter.dll`.

The project references game-managed assemblies from `Shape of Dreams_Data/Managed`. Those game files are not included in this repository.

## Attribution approach

The meter observes the game's client-side combat events and uses the game's actor/skill relationships to attribute effects to their sources. It is intended to report combat activity; it does not replace or modify the game's damage calculations.

## Repository contents

- `src/` — mod source code.
- `about/` — mod metadata and in-game description.
- `DPSMeter.csproj` — project and game-reference configuration.

## Notes

This project is maintained as a community mod. The published source is provided for transparency and independent inspection; the release DLL should be treated as a separate build artifact.
