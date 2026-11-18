# Gian Thương — Starting Perk

A standalone golden starting perk for Probably Stolen.

## Effects

- **Perk name:** Gian Thương (Price Gouger)
- **Type / cost:** Neutral (amber/gold tint), costs 1 starting perk point
- **Sale bonus:** +5% to the final negotiated value whenever a customer buys an item owned by the player
- **Displayed price:** the negotiation list uses the adjusted value, so the 5% increase is visible before accepting the deal
- **Actual payment:** the cash settlement calls the same native negotiated-value calculation, so the player receives the displayed adjusted amount
- **Starting cash:** +50 Credits
- **Faction reputation:** -5 reputation with every faction present in the new run's reputation list

The sale adjustment is applied in a Harmony postfix on the game's `GameItem.GetNegociatedValue()`. The game uses this same calculation for the sell list and for the sale settlement. The patch only applies to player-owned items while a customer is active; buying goods from a customer is unchanged. The per-item 5% bonus is rounded to a whole Credit, midpoint away from zero.

When the player hovers a sale item during a customer visit, its tooltip also shows `Gian Thương: +5% giá bán` (or the English equivalent), making the modifier explicit.

Starting Credits and reputation are granted once from `ModHook.OnNewGameStarted`, only when this perk is selected. A per-run mod-data guard prevents duplicate application. Loading an existing save does not grant the starting effects again.

## Build

Extract the repository's `Managed.zip` into `references/Managed/`, then run:

    dotnet build GianThuongPerk/GianThuongPerk.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"

The project references the game's `Assembly-CSharp.dll` and `0Harmony.dll`. The GitHub Actions workflow builds and verifies an installable ZIP.

## Verification notes

CI verifies compilation and ZIP contents. A live in-game test is still required to confirm the exact 1.0.0 build's negotiation UI and payment behavior. The displayed list and settlement are targeted through the same native value method, but the mod has not been tested in a live save here.
