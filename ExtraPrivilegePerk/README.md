# Thêm Tí Đặc Quyền — Starting Perk

A standalone starting-perk mod for Probably Stolen.

## Perk effects

- **Name:** Thêm Tí Đặc Quyền
- **Point cost:** 0 perk points (native Neutral / amber type)
- **Extra slots:** +4 starting-perk slots while this perk is selected
- **Starting Credits:** deducts 100 Credits once when a new run begins
- **Icon:** uses the supplied gold-and-blue badge image

The perk is registered through the native `ModHelper.AddStartingPerk` API with `extraPerkSlots: 4`. The 100-Credit penalty is applied through `PlayerStore.ModCash(-100, true)` in `ModHook.OnNewGameStarted`. The game documents that this hook fires for a new run and not when a save is loaded, so loading an existing save will not deduct Credits again.

The native API adds four additional starting perk slots; it does not expose a separate gold-colored slot style. The perk itself uses the native Neutral type, which is amber-tinted, and displays the supplied badge as its icon.

## Build

Extract the repository's `Managed.zip` into `references/Managed/`, then run:

    dotnet build ExtraPrivilegePerk/ExtraPrivilegePerk.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"

The GitHub Actions workflow creates an installable ZIP.

## Test notes

CI verifies compilation and ZIP packaging against the game's reference assemblies. In-game testing is still required to confirm the perk appears, shows the icon, adds four slots, and deducts 100 Credits only on new-run creation.
