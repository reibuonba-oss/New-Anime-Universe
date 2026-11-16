# Kaito — Materials & Power Merchant

A conservative, standalone Probably Stolen NPC mod.

## Trading rules

- **Buys only:** basic materials, refined materials, hand materials and energy cells. The exact vanilla item IDs `junk` and `scrap_metal` are also on his purchase whitelist.
- **Does not buy:** weapons, modules, luxury goods or other item categories.
- **Sells only:** Anime Universe Recycler (`nau_recycler`) and Advanced Battery Charger (`nau_advanced_charger`).
- Both machine sale items are optional runtime stock: the corresponding Recycler / Advanced Charger mod must be enabled and loaded. If one is unavailable, Kaito skips it instead of substituting unrelated vanilla stock.
- **Appearance:** one 8% roll per in-game day; at most one visit per day.
- **Budget:** 4,800 credits plus 25 credits per in-game day.
- **Dialogue:** Vietnamese and English.

## Implementation

- Uses the native customer-generation hook and buyer tag / ID whitelists.
- No Harmony patches or new customer sprites/factions.
- Kaito's table is restricted to the two machine IDs above; no filters, modules, weapons or other utility stock are added.
- Missing machine stock is skipped with a warning rather than breaking customer generation.

## Build

Extract the repository's `Managed.zip` into `references/Managed/`, then run:

```powershell
dotnet build KaitoNPC/KaitoNPC.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"
```

The workflow `.github/workflows/build-kaitonpc.yml` creates an installable ZIP.

## Install and dependencies

Extract the ZIP into the game's `Mods/` directory, enable Kaito, Recycler Machine and Advanced Battery Charger (to receive both machine stock items), then restart the game.

The CI build/package check does not replace an in-game save test. Confirm that the purchase whitelist accepts only the intended categories and that the two machine items appear when their respective mods are enabled.
