using System;
using System.Reflection;
using HarmonyLib;

namespace NewAnimeUniverse.RoomInventoryIconRenderFix
{
    /// <summary>
    /// Reorders item sprites after Claims & Uprisings borrows native room inventories.
    /// Each item is moved to the last sibling using the game's own PixelElement.ToFront().
    /// </summary>
    public sealed class RoomInventoryIconRenderFixMod : IMod
    {
        private const string HarmonyId = "new_anime_universe.room_inventory_icon_render_fix";
        private const string SurfaceTypeName = "Yexiaoyang.InteriorExpansion.RoomClusterSurface";

        private static Harmony harmony;
        private static bool patched;

        public void Init(ModManifest manifest)
        {
            UnityEngine.Debug.Log("[RoomInventoryIconRenderFix] Initialized.");
        }

        public void OnEnable()
        {
            // The workshop assembly may be loaded after this add-on. Retry when
            // the normal game-load phase starts, and also try immediately.
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
                UnityEngine.Debug.LogWarning("[RoomInventoryIconRenderFix] Unpatch failed: " + ex.Message);
            }

            harmony = null;
            patched = false;
        }

        private static void TryInstall()
        {
            if (patched)
                return;

            try
            {
                Type surfaceType = AccessTools.TypeByName(SurfaceTypeName);
                if (surfaceType == null)
                    return;

                MethodInfo applyPostfix = AccessTools.Method(
                    typeof(RoomInventoryIconRenderFixMod), nameof(AfterSurfaceApply));
                MethodInfo borrowPostfix = AccessTools.Method(
                    typeof(RoomInventoryIconRenderFixMod), nameof(AfterBorrow));

                harmony = harmony ?? new Harmony(HarmonyId);
                int hookCount = 0;

                MethodInfo apply = AccessTools.Method(surfaceType, "Apply");
                if (apply == null)
                    throw new MissingMethodException(SurfaceTypeName, "Apply");

                harmony.Patch(apply, postfix: new HarmonyMethod(applyPostfix));
                hookCount++;

                MethodInfo[] methods = surfaceType.GetMethods(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);

                for (int i = 0; i < methods.Length; i++)
                {
                    string name = methods[i].Name;
                    if (name != "Borrow" && name != "BorrowClipped")
                        continue;

                    harmony.Patch(methods[i], postfix: new HarmonyMethod(borrowPostfix));
                    hookCount++;
                }

                patched = true;
                UnityEngine.Debug.Log(
                    "[RoomInventoryIconRenderFix] Installed native item-icon render-order fix (" +
                    hookCount + " hooks).");
            }
            catch (Exception ex)
            {
                try { harmony?.UnpatchAll(HarmonyId); } catch { }
                harmony = null;
                patched = false;
                UnityEngine.Debug.LogWarning(
                    "[RoomInventoryIconRenderFix] Could not install render-order fix: " + ex);
            }
        }

        /// <summary>
        /// RoomClusterSurface.positions contains tuples whose Item1 is the
        /// borrowed GameGridInventory or GameSlotInventory PixelElement.
        /// </summary>
        private static void AfterSurfaceApply(object __instance)
        {
            try
            {
                FieldInfo field = AccessTools.Field(__instance.GetType(), "positions");
                object positions = field?.GetValue(__instance);
                int count = GetCount(positions);

                for (int i = 0; i < count; i++)
                {
                    object tuple = GetAt(positions, i);
                    object root = GetTupleItem(tuple, "Item1");
                    GameInventory inventory = root as GameInventory;
                    if (inventory != null)
                        RaiseInventoryItems(inventory);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning(
                    "[RoomInventoryIconRenderFix] Surface inventory pass failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Borrow and BorrowClipped both receive the actual GameInventory as an
        /// argument. This first pass handles items already present when a room opens.
        /// </summary>
        private static void AfterBorrow(object[] __args)
        {
            if (__args == null)
                return;

            for (int i = 0; i < __args.Length; i++)
            {
                GameInventory inventory = __args[i] as GameInventory;
                if (inventory != null)
                    RaiseInventoryItems(inventory);
            }
        }

        private static void RaiseInventoryItems(GameInventory inventory)
        {
            if (inventory == null)
                return;

            try
            {
                PropertyInfo childItemsProperty =
                    AccessTools.Property(inventory.GetType(), "childItems") ??
                    AccessTools.Property(typeof(GameInventory), "childItems");
                object children = childItemsProperty?.GetValue(inventory, null);
                int count = GetCount(children);

                for (int i = 0; i < count; i++)
                {
                    object child = GetAt(children, i);
                    PixelElement pixel = child as PixelElement;

                    // GameItemElement normally implements PixelElement directly.
                    // Keep a fallback for item wrappers whose visible icon is exposed
                    // through the item's content window.
                    if (pixel == null)
                    {
                        object contentWindow = GetMemberValue(child, "contentWindow");
                        object childElement = GetMemberValue(contentWindow, "childElement");
                        pixel = childElement as PixelElement;
                    }

                    if (pixel != null && pixel.handler != null)
                        pixel.ToFront();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning(
                    "[RoomInventoryIconRenderFix] Could not raise inventory item icons: " + ex.Message);
            }
        }

        private static object GetTupleItem(object tuple, string name)
        {
            if (tuple == null)
                return null;

            Type type = tuple.GetType();
            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field != null)
                return field.GetValue(tuple);

            PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            return property?.GetValue(tuple, null);
        }

        private static object GetMemberValue(object instance, string name)
        {
            if (instance == null)
                return null;

            Type type = instance.GetType();
            PropertyInfo property = AccessTools.Property(type, name);
            if (property != null)
                return property.GetValue(instance, null);

            FieldInfo field = AccessTools.Field(type, name);
            return field?.GetValue(instance);
        }

        private static int GetCount(object collection)
        {
            if (collection == null)
                return 0;

            try
            {
                object count = GetMemberValue(collection, "Count");
                return count == null ? 0 : Convert.ToInt32(count);
            }
            catch
            {
                return 0;
            }
        }

        private static object GetAt(object collection, int index)
        {
            if (collection == null)
                return null;

            try
            {
                PropertyInfo indexer = AccessTools.Property(collection.GetType(), "Item");
                if (indexer != null)
                    return indexer.GetValue(collection, new object[] { index });

                MethodInfo getter = AccessTools.Method(collection.GetType(), "get_Item");
                return getter?.Invoke(collection, new object[] { index });
            }
            catch
            {
                return null;
            }
        }
    }
}
