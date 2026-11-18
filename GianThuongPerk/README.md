# Gian Thương — Starting Perk

A standalone golden starting perk for Probably Stolen.

## Effects

- **Perk name:** Gian Thương (Price Gouger)
- **Type / cost:** Neutral (amber/gold tint), costs 1 starting perk point
- **Sale bonus:** +5% to the final negotiated value whenever a customer buys an item owned by the player
- **Displayed sale price:** the price at the right side of the negotiation item row includes the +5%
- **Price breakdown:** the expanded row adds a native Market-stage entry, **Gian Thương (+5%)**, alongside other market modifiers such as retail markup
- **Actual payment:** the native negotiation settlement uses the same `GameItem.GetNegociatedValue()` calculation, so the adjusted displayed value is the amount paid
- **Starting cash:** +50 Credits
- **Faction reputation:** -5 reputation with every faction present in the new run's reputation list

The game renders negotiation price details from `ItemFeature` entries grouped by `ValueStage`. A Harmony postfix on the native `NegociationUIManager.AddFeatureSection` adds a presentation-only Market feature for player-owned items being sold to a customer. Separately, a Harmony postfix on `GameItem.GetNegociatedValue()` applies the numeric 5% increase. This is the same method used by the negotiation item-row price, the total, affordability checks, and settlement. Buying goods from customers is unchanged.

The displayed Market entry is not attached to the saved item; it is inserted into the UI's temporary breakdown list. The numeric bonus is applied once per negotiated item value and rounded to a whole Credit, midpoint away from zero.

Starting Credits and reputation are granted once from `ModHook.OnNewGameStarted`, only when this perk is selected. A per-run mod-data guard prevents duplicate application. Loading an existing save does not grant the starting effects again.

## Build

Extract the repository's `Managed.zip` into `references/Managed/`, then run:

    dotnet build GianThuongPerk/GianThuongPerk.csproj -c Release -p:ManagedDir="C:\\Path\\To\\references\\Managed"

The project references the game's `Assembly-CSharp.dll` and `0Harmony.dll`. The GitHub Actions workflow builds and verifies an installable ZIP.

## Verification notes

CI verifies compilation and ZIP contents. The numeric price and breakdown hooks are based on the game assembly's decompiled negotiation flow, but a live in-game test is still required to verify the exact rendered line and paid amount in a running save.
