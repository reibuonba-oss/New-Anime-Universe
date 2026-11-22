using System;
using System.Reflection;
using HarmonyLib;

namespace NewAnimeUniverse.RoomInventoryIconRenderFix
{
    /// <summary>
    /// One-shot render-order correction for inventories as they are attached
    /// to the shared room surface. This deliberately does not patch Apply(),
    /// which may run repeatedly during UI layout/scroll operations.
    /// </summary>
    public sealed class RoomInventoryIconRenderFixMod : IMod
    {
        private const string HarmonyId = "new_anime_universe.room_inventory_icon_render_fix";
        private const string SurfaceTypeName = "Yexiaoyang.InteriorExpansion.RoomClusterSurface";

        private static Harmony harmony;
        private static bool patched;
        private static bool installing;

        public void Init(ModManifest manifest)
        {
            UnityEngine.Debug.Log("[RoomInventoryIconRenderFix] Initialized.");
        }

        public void OnEnable()
        {
            ModHook.OnGameLoadedNormal += TryInstall;
            TryInstall();
        }

        public void OnDisable()
        {
            ModHook.OnGameLoadedNormal -= TryInstall;

            try
            {
                harmony?.UnpatchAll(HarmonyId);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning(
                    "[RoomInventoryIconRenderFix] Unpatch failed: " + ex.Message);
            }

            harmony = null;
            patched = false;
            installing = false;
        }

        private static void TryInstall()
        {
            if (patched || installing)
                return;

            installing = true;
            try
            {
                Type surfaceType = AccessTools.TypeByName(SurfaceTypeName);
                if (surfaceType == null)
                    return;

                // These are the two entry points through which room inventories
                // are attached to the common surface in Claims & Uprisings.
                // Each method receives the actual inventory as PixelElement arg 0.
                MethodInfo borrow = AccessTools.Method(surfaceType, "Borrow");
                MethodInfo borrowClipped = AccessTools.Method(surfaceType, "BorrowClipped");

                if (borrow == null && borrowClipped == null)
                {
                    UnityEngine.Debug.LogWarning(
                        "[RoomInventoryIconRenderFix] RoomClusterSurface has no supported Borrow methods.");
                    return;
                }

                harmony = harmony ?? new Harmony(HarmonyId);
                MethodInfo postfix = AccessTools.Method(
                    typeof(RoomInventoryIconRenderFixMod), nameof(AfterBorrow));

                if (borrow != null)
                    harmony.Patch(borrow, postfix: new HarmonyMethod(postfix));

                if (borrowClipped != null)
                    harmony.Patch(borrowClipped, postfix: new HarmonyMethod(postfix));

                patched = true;
                UnityEngine.Debug.Log(
                    "[RoomInventoryIconRenderFix] Installed one-shot inventory icon ordering hook(s).");
            }
            catch (Exception ex)
            {
                try { harmony?.UnpatchAll(HarmonyId); } catch { }
                harmony = null;
                patched = false;
                UnityEngine.Debug.LogWarning(
                    "[RoomInventoryIconRenderFix] Patch installation failed: " + ex);
            }
            finally
            {
                installing = false;
            }
        }

        /// <summary>
        /// Runs once after Borrow/BorrowClipped attaches an inventory panel.
        /// GameGridInventory and GameSlotInventory both inherit GameInventory
        /// and implement PixelElement, so arg 0 can be safely checked here.
        /// </summary>
        private static void AfterBorrow(PixelElement __0)
        {
            if (__0 == null)
                return;

            GameInventory inventory = __0 as GameInventory;
            if (inventory == null)
                return;

            try
            {
                // childItems is the native List<GameItem> exposed by GameInventory.
                // Only UI-backed GameItemElements implement PixelElement.
                var items = inventory.childItems;
                if (items == null)
                    return;

                for (int i = 0; i < items.Count; i++)
                {
                    GameItem item = items[i];
                    PixelElement itemElement = item as PixelElement;
                    if (itemElement != null)
                        itemElement.ToFront();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning(
                    "[RoomInventoryIconRenderFix] Could not reorder borrowed inventory icons: " +
                    ex.Message);
            }
        }
    }
}
