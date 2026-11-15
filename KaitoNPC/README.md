# Kaito — The Collector

A conservative, standalone Probably Stolen NPC mod built around vanilla game systems.

## Design

- **Appearance:** one 8% roll per in-game day. Kaito can visit at most once per day.
- **Buys:** a curated list of vanilla weapons, plus items tagged `MODULE`, `ENERGY_CELL`, and `LUXURY_ITEM`.
- **Budget:** 4,800 credits plus 25 credits per in-game day.
- **Sells:** Advanced Water Filter, HydraTech Micro Water Purifier, Energy Cell Mk. II, Self Charging Energy Cell, and two different vanilla modules per visit.
- **Dialogue:** original Kaito dialogue in Vietnamese and English.
- **Portrait:** the supplied 242 × 300 transparent PNG.

## Low-risk implementation choices

- No custom item definitions or item directory hooks.
- No Harmony patches, reflection, filesystem access, or external capabilities.
- Uses the native `IMod` lifecycle, the customer-generation hook, and vanilla item IDs.
- Missing stock items are skipped with a warning rather than causing the customer generation to fail.
- The daily roll state is stored through the mod's per-run ModData.

## Build

Extract the repository's `Managed.zip` into `references/Managed/`, then run:

```powershell
dotnet build KaitoNPC/KaitoNPC.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"
```

The workflow `.github/workflows/build-kaitonpc.yml` creates an installable ZIP.

## Install

Extract the ZIP into the game's `Mods/` directory, enable Kaito in the Mods menu, and restart the game.

This is a compiled mod, but an in-game test is still needed to verify actual customer generation, trade acceptance and stock placement on the user's game build.
