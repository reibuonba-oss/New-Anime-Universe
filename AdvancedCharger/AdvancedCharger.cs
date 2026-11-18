using System;

namespace NewAnimeUniverse.AdvancedCharger
{
    public sealed class AdvancedChargerMod : IMod
    {
        public const string ModId = "new_anime_universe.advanced_charger";
        public const string ItemId = "nau_advanced_charger";

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
            log.Log("Trạm Sạc Pin Nâng Cao đã khởi tạo.");
        }

        public void OnEnable()
        {
            ModHook.OnModItemDirectoryInit += RegisterItems;
            ModHook.OnCreateTooltipLate += AddTooltip;

            ModHelper.AddDebugFunction(
                ModId,
                ModHelper.DEBUG_CATEGORY_SPAWN,
                "Tạo Trạm Sạc Pin Nâng Cao",
                SpawnChargerForDebug);
        }

        public void OnDisable()
        {
            ModHook.OnModItemDirectoryInit -= RegisterItems;
            ModHook.OnCreateTooltipLate -= AddTooltip;
            ModHelper.RemoveDebugFunctions(ModId);

            if (ReferenceEquals(sharedLog, log))
                sharedLog = null;
        }

        private void RegisterItems(ModItemDirectory directory)
        {
            if (directory == null) return;

            directory.Add(ItemId, CreateAdvancedCharger);
            log.Log("Đã đăng ký Trạm Sạc Pin Nâng Cao.");
        }

        private static GameItem CreateAdvancedCharger()
        {
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
                    sharedLog?.Error("Chu kỳ của Trạm Sạc Pin Nâng Cao gặp lỗi: " + ex);
                }
            };

            ModHelper.SetSource(machine, ModId, "Vũ Trụ Anime — Trạm Sạc Pin Nâng Cao");
            return machine;
        }

        private static void ChargeInsertedBatteries(
            GameItem machine,
            GameInventory batteryInventory)
        {
            if (machine == null || batteryInventory == null) return;

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

                PowerHelper.ChargeBattery(battery, chargeAmount, false);
                chargedCount++;
            }

            machine.Validate();
            sharedLog?.Log(
                "Trạm sạc đã nạp " + chargeAmount +
                " kJ cho " + chargedCount + " viên pin.");
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
                log.Error("Không thể tạo Trạm Sạc Pin Nâng Cao: " + ex);
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
                log.Warning("Không thể tạo mô tả Trạm Sạc Pin Nâng Cao: " + ex.Message);
            }
        }

        private static string Text(string key)
        {
            switch (key)
            {
                case "advanced_charger.name": return "Trạm Sạc Pin Nâng Cao";
                case "advanced_charger.description": return "Sạc pin năng lượng được lắp vào nhanh gấp đôi máy Sạc Pin tiêu chuẩn. Cần có điện lưới.";
                case "advanced_charger.flavor": return "Trạm sạc gia cố công suất cao, được thiết kế để nạp pin nhanh hơn.";
                case "advanced_charger.tooltip.rate": return "Tốc độ sạc: gấp 2 lần máy Sạc Pin tiêu chuẩn.";
                case "advanced_charger.tooltip.power": return "6 kJ mỗi đêm; 12 kJ khi đã mở khóa Điện Thương Mại.";
                default: return "Chưa có bản dịch: " + key;
            }
        }
    }
}
