# Anime Universe — Recycler Machine

A standalone, vanilla-style Probably Stolen machine.

## Recipe

- Input grid: **12×4**, enough room for eight standard 3×2 Junk items at once.
- Base cycle: **4 Junk** (`junk`) → **1 Scrap Metal** (`scrap_metal`), overnight.
- Base energy draw: **5 energy per cycle**.
- At **50% or more Performance**: when at least 8 Junk are loaded, the machine processes **8 Junk → 2 Scrap Metal** in that same cycle.
- If Performance is below 50%, the machine processes only the base batch.
- Efficiency modules use the game's native machine-power calculation and reduce the draw.
- Quality modules do not change this machine's recipe, output, or power logic.

The input accepts only the vanilla item id `junk`; it will not consume other items that merely carry a JUNK type (for example improvised weapons).

## Interface and compatibility

- Uses the game's Machinery window, battery slot, universal module bay, and native overnight machine cycle callback.
- Uses the vanilla furnace sprite and an original item ID.
- Does not patch game methods with Harmony and declares no extra capabilities.
- Does **not** grant a free machine on game start. Obtain the Recycler from Kaito when his shop visit is available; enable Kaito NPC as well.
- An F9 debug spawn button is included for testing builds.

## Build

Extract the repository's `Managed.zip` into `references/Managed/`, then run:

```powershell
dotnet build RecyclerMachine/RecyclerMachine.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"
```

## Install

Extract the generated ZIP into the game's `Mods/` directory. Enable **Anime Universe — Recycler Machine** and **Kaito — The Collector** in the Mods menu and restart the game. Purchase the Recycler from Kaito; no free starter copy is given.

The DLL build and package checks do not replace an in-game save test. Confirm the nightly cycle, energy draw, output capacity, and module threshold on the exact game build before treating this as production-ready.
