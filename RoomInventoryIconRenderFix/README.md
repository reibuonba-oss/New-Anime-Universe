# Room Inventory Icon Render Fix

Standalone visual-only add-on for the shared room inventory surface in **Probably Stolen: Claims & Uprisings**.

## Fixed symptom

Module and ingredient icons appear underneath the inventory cell lines in room windows. The room mod uses the same `RoomClusterSurface` for module grids, input/output grids, and single-slot inventories across room types.

## Patch

- Hooks `RoomClusterSurface.Borrow`, `BorrowClipped`, and `Apply`.
- Finds each borrowed `GameInventory` and its `childItems`.
- Calls the game's native `PixelElement.ToFront()` on each rendered item icon, after the room surface has applied its positions.
- Does not change item sprites, item scale, inventory dimensions, item shapes, slot count, recipe rules, module effects, production, or drag/drop checks.
- Removes its Harmony patches when this add-on is disabled.

## Install

1. Create `Mods/RoomInventoryIconRenderFix/`.
2. Extract the contents of `RoomInventoryIconRenderFix.zip` into that folder.
3. Confirm `Mods/RoomInventoryIconRenderFix/manifest.xml` and `RoomInventoryIconRenderFix/RoomInventoryIconRenderFix.dll` exist.
4. Enable **Room Inventory Icon Render Fix** in the in-game Mods menu.

This add-on does not overwrite or modify the Claims & Uprisings workshop package.

## Verification

The GitHub Actions workflow compiles against the game's managed assemblies and checks the installable ZIP contents. A live in-game check is still required to confirm the visual result and existing drag/drop interactions.
