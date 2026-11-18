# Con Người May Mắn — Starting Perk

A standalone starting-perk mod for Probably Stolen.

## Perk effects

- **Name:** Con Người May Mắn
- **Cost:** 2 perk points
- **Extra slots:** +4 starting-perk slots while this perk is selected
- **Starting Luck:** +15 when a new run begins

The perk is registered through the game's native ModHelper.AddStartingPerk API. Its Luck bonus is applied to PlayerStore.Instance.healthData.luck from ModHook.OnNewGameStarted, so the value is stored with the run. The bonus is not re-applied when loading a save.

## Build

Extract the repository's Managed.zip into references/Managed/, then run:

    dotnet build LuckyHumanPerk/LuckyHumanPerk.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"

The GitHub Actions workflow creates an installable ZIP.

## Test notes

A successful CI build confirms the code compiles against the repository's game reference assemblies. In-game testing is still required to confirm the perk appears, unlocks four additional slots, and starts a new run with +15 Luck.
