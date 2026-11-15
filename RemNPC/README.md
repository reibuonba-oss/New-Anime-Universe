# Rem NPC — New Anime Universe

A Probably Stolen mod that adds Rem as a rare upper-level merchant.

## Behaviour

- **Appearance:** one independent **9% roll per in-game day**, evaluated at `OnGenerateCustomerVeryLate`. Rem can visit at most once per day.
- **Sells:** four randomly selected vanilla items from the game's `LUXURY_ITEM` catalogue per visit.
- **Buys:** vanilla items with the `FOOD` type plus the supported water item IDs listed in `RemNPC.cs`.
- **Dialogue:** custom opening dialogue and a separate buy-food/water line, localized in English and Vietnamese.
- **Portrait:** uses `Textures/rem.png`, registered under a unique customer sprite key.

The customer uses `StoreClient.ClientIntent.SELLNBUY`, so she can trade in both directions during the same visit. Her buy budget starts at 1,600 and scales by 20 per in-game day.

## Build

The repository contains `Managed.zip` at its root. Extract it into `references/Managed/`, then run:

```powershell
dotnet build RemNPC.csproj -c Release
```

Or provide another reference folder:

```powershell
dotnet build RemNPC.csproj -c Release -p:ManagedDir="C:\Path\To\Probably Stolen_Data\Managed"
```

The GitHub Actions workflow `.github/workflows/build-remnpc.yml` builds the DLL against the repository's `Managed.zip` and publishes an installable `RemNPC` ZIP artifact.

## Install

Extract the built `RemNPC` folder into the game's `Mods/` directory, enable it in the Mods menu, and restart the game.

## Technical notes

- Uses the native `IMod` lifecycle and `ModHook` customer/assets hooks.
- Adds the customer through `StoreClientManager.AddClient` from `OnGenerateCustomerVeryLate`.
- The one-roll-per-day and one-visit-per-day guards are saved through the mod's run data.
- The customer portrait is installed through `SpriteDict.Instance.spriteDictionary`. Customer sprite registration is a compatibility-sensitive game surface; verify it after game updates.
- This source was prepared from the repository's reference assemblies and public modding examples. It still requires a successful CI build and an in-game test before it should be described as fully verified.
