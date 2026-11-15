# Hanabi NPC — New Anime Universe

A standalone Probably Stolen merchant mod.

## Hanabi's trading

- **Appearance:** one 10% roll per in-game day, evaluated at `OnGenerateCustomerVeryLate`. Hanabi can visit at most once per day.
- **Buys:** items tagged `power_source_item` (power cells/batteries) and `MODULE`.
- **Sells:** one Hanabi Mask and one Hanabi Gun on each visit.
- **Custom item registration:** uses `OnModItemDirectoryInit` and `ModItemDirectory.Add`.
- **Types:** Hanabi Mask has both `HOUSEHOLD_GOOD` and `LUXURY_ITEM`; Hanabi Gun has `WEAPON`.
- **Dialogue:** separate Hanabi lines, localized in Vietnamese and English.
- **Portrait:** uses `Textures/hanabi.png`.

## Prices

The two base market values are **888 credits** (Hanabi Mask) and **999 credits** (Hanabi Gun). Hanabi adds a 5% seller premium to the values placed on her table, rounded to the nearest credit:

| Item | Base market value | Hanabi's listed value |
|---|---:|---:|
| Hanabi Mask | 888 | 932 |
| Hanabi Gun | 999 | 1,049 |

The premium is applied directly to the spawned stock item's `unitValue`; the code does not rely on `StoreClient.sellPriceModifier`.

## Build

The repository contains `Managed.zip` at its root. Extract it into `references/Managed/`, then run:

```powershell
dotnet build HanabiNPC/HanabiNPC.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"
```

The GitHub Actions workflow `.github/workflows/build-hanabinpc.yml` builds the DLL, converts all images to standard 8-bit RGBA PNG, and publishes an installable ZIP artifact with the `HanabiNPC/` folder at its root.

## Install

Extract the ZIP directly into the game's `Mods/` directory. It will create `Mods/HanabiNPC/manifest.xml`, `HanabiNPC.dll`, `Textures/` and `Localization/`. Enable Hanabi in the Mods menu and restart the game.

## Compatibility

The mod registers its two custom item factories through the official item-directory hook. The portrait uses the same customer-sprite dictionary technique as the project's Rem and Kurumi NPC mods; customer sprite registration is compatibility-sensitive and should be verified in-game after game updates.

Hanabi Gun is registered as a custom item with the `WEAPON` game-item type. This implementation does not add firing, ammunition, or equip/combat behaviour.
