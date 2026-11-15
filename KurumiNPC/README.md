# Kurumi NPC — New Anime Universe

A standalone Probably Stolen NPC mod. Kurumi is an upper-level arms dealer who buys weapons and sells advanced energy cells and modules.

## Behaviour

- **Appearance:** one independent **9% roll per in-game day** at `OnGenerateCustomerVeryLate`; at most one Kurumi visit per day.
- **Buys:** firearms, melee weapons and gun modifications. The client has the native `SELLNBUY` intent and uses the `WEAPON` and `GUN_MOD` buying tags, with an explicit vanilla weapon-ID list as a fallback.
- **Sells:** an Energy Cell (Mk. II), a Self Charging Energy Cell, and four randomly selected specialist modules on each visit.
- **Dialogue:** custom Kurumi dialogue localized in Vietnamese and English.
- **Portrait:** uses the supplied `Textures/kurumi.png` portrait, registered under a unique sprite key.

The buying budget starts at 6,000 and increases by 50 per in-game day. This is the NPC's offer budget, not a guaranteed purchase price.

## Build

The repository contains `Managed.zip` at its root. Extract it into `references/Managed/`, then run:

```powershell
dotnet build KurumiNPC.csproj -c Release
```

Or specify another reference folder:

```powershell
dotnet build KurumiNPC.csproj -c Release -p:ManagedDir="C:\Path\To\Probably Stolen_Data\Managed"
```

The workflow `.github/workflows/build-kuruminpc.yml` builds the DLL and publishes an installable ZIP artifact.

## Install

Extract the built `KurumiNPC.zip` so the game folder contains:

```
Probably Stolen/
└── Mods/
    └── KurumiNPC/
        ├── manifest.xml
        ├── KurumiNPC.dll
        ├── Textures/kurumi.png
        └── Localization/
            ├── en.csv
            └── vi.csv
```

Enable Kurumi in the Mods menu and restart the game.

## Technical notes

- Uses the native `IMod` lifecycle and customer/assets hooks.
- Adds the NPC through `StoreClientManager.AddClient`.
- Uses per-run ModData guards so the appearance roll is evaluated once per day.
- Registers its portrait through `SpriteDict.Instance.spriteDictionary`. This is a compatibility-sensitive customer-sprite surface and should be checked after game updates.
- A successful CI build confirms compilation against this repository's `Managed.zip`; it does not replace an in-game test of customer arrival and two-way trading.
