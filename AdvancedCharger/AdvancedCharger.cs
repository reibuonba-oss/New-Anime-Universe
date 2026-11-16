using System;

namespace NewAnimeUniverse.AdvancedCharger
{
    public sealed class AdvancedChargerMod : IMod
    {
        public const string ModId = "new_anime_universe.advanced_charger";
        public const string ItemId = "nau_advanced_charger";

        // Vanilla Recharger: 3 kJ per night, or 6 kJ with Commercial Power.
        // This machine doubles those values without bypassing the game's power checks.
        private const int VanillaChargeRate = 3;
        private const int AdvancedChargeRate = VanillaChargeRate * 2;
        private const int AdvancedCommercialChargeRate = VanillaChargeRate * 4;
        private const long StationValue = 300L;

        private static ModLog sharedLog;
        private ModLog log;

        public void Init(ModManifest manifest)
        {
            log = new ModLog(manifest);
            sharedLog = log;
            log.Log("Advanced Battery Charger initialized.");
        }

        public void OnEnable()
        {
            ModHook.OnModItemDirectoryInit += RegisterItems;
            ModHook.OnGameLoadedLate += EnsureStarterCharger;
            ModHook.OnCreateTooltipLate += AddTooltip;

            ModHelper.AddDebugFunction(
                ModId,
                ModHelper.DEBUG_CATEGORY_SPAWN,
                "Spawn Advanced Charger",
                SpawnChargerForDebug);
        }

        public void OnDisable()
        {
            ModHook.OnModItemDirectoryInit -= RegisterItems;
            ModHook.OnGameLoadedLate -= EnsureStarterCharger;
            ModHook.OnCreateTooltipLate -= AddTooltip;
            ModHelper.RemoveDebugFunctions(ModId);

            if (ReferenceEquals(sharedLog, log))
                sharedLog = null;
        }

        private void RegisterItems(ModItemDirectory directory)
        {
            if (directory == null) return;

            directory.Add(ItemId, CreateAdvancedCharger);
            log.Log("Registered Advanced Battery Charger.");
        }

        private static GameItem CreateAdvancedCharger()
        {
            // Reuse the vanilla Recharger's own panel and battery-grid layout.
            var inventoryWindow = MachineRecharger.CreateMachineInventoryWindow(true, true);
            GameInventory batteries = inventoryWindow.Item2;

            batteries.mayInventoryAddItemFunc =
                (GameItem item, GameInventory _) =>
                    item != null && item.IsTag(PowerHelper.POWER_SOURCE_ITEM_TAG);

            GameItem machine = ItemDirectory.CreateEmptyItem(ItemId)
                .SetContentWindow(inventoryWindow.Item1);

            machine.name = Text("advanced_charger.name");
            machine.shortDescription = Text("advanced_charger.description");
            machine.flavorText = Text("advanced_charger.flavor");
            machine.SetSpriteAndShape("Items/items_tool", "recharger_base");

            machine.EnableTag("IMPORTANT_TAG", false);
            machine.unitValue = StationValue;
            machine.SetGameItemType("MACHINE");
            machine.EnableTag("CONTAINER_TAG", false);
            machine.EnableTag("manufacture", false);

            AudioHelper.InitSmallMachine(machine);

            machine.onCycleEndSlotItemFunc = delegate(
                GameItem _, GameInventory __, SlotMarker ___)
            {
                try
                {
                    ChargeInsertedBatteries(machine, batteries);
                }
                catch (Exception ex)
                {
                    sharedLog?.Error("Advanced Charger cycle failed: " + ex);
                }
            };

            ModHelper.SetSource(machine, ModId, "Anime Universe — Advanced Battery Charger");
            return machine;
        }

        private static void ChargeInsertedBatteries(
            GameItem machine,
            GameInventory batteryInventory)
        {
            if (machine == null || batteryInventory == null) return;

            // Match vanilla Recharger behavior: the station only operates while grid power is on.
            PlayerStore store = PlayerStore.Instance;
            if (store == null || !store.isPowerOn) return;

            bool commercialPower = NetworkUpgrade.IsCommercialPowerUnlocked();
            int chargeAmount = commercialPower
                ? AdvancedCommercialChargeRate
                : AdvancedChargeRate;

            int chargedCount = 0;
            foreach (GameItem battery in batteryInventory.childItems)
            {
                if (battery == null || !battery.IsTag(PowerHelper.POWER_SOURCE_ITEM_TAG))
                    continue;

                // PowerHelper enforces max capacity and vanilla blackout/battery restrictions.
                PowerHelper.ChargeBattery(battery, chargeAmount, false);
                chargedCount++;
            }

            machine.Validate();
            sharedLog?.Log(
                "Advanced Charger applied " + chargeAmount +
                " kJ to " + chargedCount + " inserted battery item(s).");
        }

        private void EnsureStarterCharger()
        {
            try
            {
                if (PlayerStore.Instance == null) return;
                if (ModHelper.GetModDataBool(ModId, "starter_charger_given")) return;

                GameItem machine = DirectoryMaster.Item(ItemId, true);
                if (machine == null)
                {
                    log.Warning("Could not create the starter Advanced Charger.");
                    return;
                }

                ModHelper.MoveItemToInventory(machine);
                if (machine.parentInventory != null)
                {
                    ModHelper.SetModDataBool(ModId, "starter_charger_given", true);
                    log.Log("Granted one starter Advanced Battery Charger.");
                }
                else
                {
                    log.Warning("Player inventory is full; the starter charger will be retried on next load.");
                }
            }
            catch (Exception ex)
            {
                log.Error("Could not grant the starter Advanced Charger: " + ex);
            }
        }

        private void SpawnChargerForDebug()
        {
            try
            {
                GameItem machine = DirectoryMaster.Item(ItemId, true);
                if (machine == null) return;
                ModHelper.MoveItemToInventory(machine);
            }
            catch (Exception ex)
            {
                log.Error("Could not spawn an Advanced Charger: " + ex);
            }
        }

        private void AddTooltip(RichTextBuilder builder, GameItem item)
        {
            if (builder == null || item == null || item.identifier != ItemId) return;

            try
            {
                builder.AddLine(
                    Text("advanced_charger.tooltip.rate"),
                    true,
                    RenderHandler.ColorPalette.LightCyan);
                builder.AddLine(
                    Text("advanced_charger.tooltip.power"),
                    true,
                    RenderHandler.ColorPalette.LightCyan);
            }
            catch (Exception ex)
            {
                log.Warning("Could not build Advanced Charger tooltip: " + ex.Message);
            }
        }

        private static string Text(string key)
        {
            string value = ModHelper.GetLocalized(ModId, key, null);
            return string.IsNullOrEmpty(value) || value == key ? key : value;
        }
    }
}