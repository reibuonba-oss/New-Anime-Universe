# Anime Universe — Advanced Battery Charger

Standalone mod for Probably Stolen.

## Charging rate

The vanilla Recharger applies 3 kJ to each inserted power-source item per nightly cycle. With Commercial Power unlocked, the vanilla rate is 6 kJ.

The Advanced Battery Charger doubles both rates:

- **6 kJ per inserted battery per night** without Commercial Power.
- **12 kJ per inserted battery per night** with Commercial Power unlocked.

It uses the game's native `PowerHelper.ChargeBattery` method, so the normal maximum-capacity handling and blackout restrictions remain in place. It also follows the vanilla rule that the grid must be powered.

## Interface and installation

- Reuses the vanilla Recharger battery panel and inventory grid.
- Accepts items carrying the vanilla `power_source_item` tag.
- Does **not** grant a free station on game start. Purchase it from Kaito when his shop visit is available; enable Kaito NPC as well.
- Includes an F9 debug spawn entry for testing.
- Build against the game's `Managed.zip` reference assemblies.

Extract the generated `AdvancedCharger.zip` into the game's `Mods` directory, enable **Anime Universe — Advanced Battery Charger** and **Kaito — The Collector**, and restart the game. Purchase the station from Kaito; no free starter copy is given.

The CI build/package check verifies compilation and ZIP contents. A real in-game save test is still required to confirm the charging cycle on the exact installed game version.
