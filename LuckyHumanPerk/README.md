# Con Người May Mắn — Starting Perk

A standalone starting-perk mod for Probably Stolen.

## Perk effects

- **Name:** Con Người May Mắn
- **Cost:** 2 perk points
- **Extra slots:** +4 starting-perk slots while this perk is selected
- **Starting Luck:** +15 when a new run begins
- **Icon:** a white 32×32 four-leaf clover glyph in `Textures/Items/lucky_human_icon.png`; the game tints it green for this positive perk

The perk is registered through the game's native `ModHelper.AddStartingPerk` API with `iconKey: "new_anime_universe.lucky_human:lucky_human_icon"`. Its Luck bonus is applied to `PlayerStore.Instance.healthData.luck` from `ModHook.OnNewGameStarted`, so the value is stored with the run. The bonus is not re-applied when loading a save.

Perk icons must be item sprites in `Textures/Items/`. The build workflow verifies that the icon is included in the installable ZIP.

## Build

Extract the repository's Managed.zip into references/Managed/, then run:

    dotnet build LuckyHumanPerk/LuckyHumanPerk.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"

The GitHub Actions workflow creates an installable ZIP.

## Test notes

CI verifies compilation and ZIP packaging against the game's reference assemblies. In-game testing is still required to confirm the clover icon appears at the intended size, the perk unlocks four additional slots, and a new run starts with +15 Luck.
