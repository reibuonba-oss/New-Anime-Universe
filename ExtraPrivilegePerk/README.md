# Thêm Tí Đặc Quyền — Starting Perk

A standalone starting-perk mod for Probably Stolen.

## Perk effects

- **Name:** Thêm Tí Đặc Quyền
- **Perk points:** grants **+1 starting perk point** for the new-game perk-selection budget
- **Extra slots:** +4 starting-perk slots while this perk is selected
- **Starting Credits:** deducts 100 Credits once when a new run begins
- **Icon:** uses the supplied gold-and-blue badge image; the game tints it according to the perk type

The perk is registered through the native `ModHelper.AddStartingPerk` API with `cost: -1`, `type: StartingPerk.StartingPerkType.NEGATIVE`, and `extraPerkSlots: 4`. The negative cost is how the native starting-perk system refunds one point to the player's perk-selection budget; no custom point manipulation or Harmony patch is needed.

The 100-Credit penalty is applied through `PlayerStore.ModCash(-100, true)` in `ModHook.OnNewGameStarted`. This hook fires for a new run and not when a save is loaded, so loading an existing save will not deduct Credits again.

## Build

Extract the repository's `Managed.zip` into `references/Managed/`, then run:

    dotnet build ExtraPrivilegePerk/ExtraPrivilegePerk.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"

The GitHub Actions workflow creates an installable ZIP.

## Test notes

CI verifies compilation and ZIP packaging against the game's reference assemblies. In-game testing is still required to confirm the perk shows a one-point refund, adds four slots, and deducts 100 Credits only on new-run creation.
