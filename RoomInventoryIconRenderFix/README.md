# Room Inventory Icon Render Fix 0.1.1

Standalone visual-only add-on for the shared room inventory surface in **Probably Stolen: Claims & Uprisings**.

## Fixed symptom

Module and ingredient icons appear underneath inventory cell lines in multiple room types.

## What changed in 0.1.1

- Removed the postfix on `RoomClusterSurface.Apply`. That method may run repeatedly during layout and scrolling, so walking every inventory after each call was unnecessary work on the UI thread.
- The patch now runs only after `Borrow` or `BorrowClipped` attaches an inventory panel.
- Uses the native `GameInventory.childItems` list and `PixelElement.ToFront()` directly; no reflective traversal of inventory collections.
- It does not change item sprites, scale, shapes, slot counts, recipes, module effects, production, or drag/drop checks.
- All Harmony hooks are removed when the add-on is disabled.

## Install

1. Remove the older `RoomInventoryIconRenderFix` folder completely.
2. Create `Mods/RoomInventoryIconRenderFix/`.
3. Extract the contents of `RoomInventoryIconRenderFix.zip` into that folder.
4. Enable **Room Inventory Icon Render Fix** in the in-game Mods menu.

This is a separate add-on; it does not overwrite the Claims & Uprisings workshop mod.

## Verification

The build workflow compiles against the supplied game assemblies and verifies the installable ZIP. A live in-game test is still required to verify the appearance of modules and input/output items in all room types.
