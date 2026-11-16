using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NewAnimeUniverse.RecyclerMachine
{
    public sealed class RecyclerMachineMod : IMod
    {
        public const string ModId = "new_anime_universe.recycler_machine";
        public const string ItemId = "nau_recycler";
        private const int BasePower = 5;
        private const int BaseInput = 4;
        private const int DoubleBatchPerformance = 50;

        private sealed class Slots
        {
            public GameSlotInventory Battery;
            public GameGridInventory Modules;
            public GameGridInventory Input;
            public GameGridInventory Output;
        }

        private static readonly ConditionalWeakTable<GameItem, Slots> SlotMap =
            new ConditionalWeakTable<GameItem, Slots>();
        private static ModLog sharedLog;
        private ModLog log;

        public void Init(ModManifest manifest)
        {
            log = new ModLog(manifest);
            sharedLog = log;
            log.Log("Recycler Machine initialized.");
        }

        public void OnEnable()
        {
            ModHook.OnModItemDirectoryInit += RegisterItems;
            ModHook.OnCreateTooltipLate += AddTooltip;
            ModHelper.AddDebugFunction(
                ModId,
                ModHelper.DEBUG_CATEGORY_SPAWN,
                "Spawn Recycler",
                SpawnRecyclerForDebug);
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
            directory.Add(ItemId, CreateRecycler);
            log.Log("Registered Recycler.");
        }

        private static GameItem CreateRecycler()
        {
            Slots slots = new Slots
            {
                Battery = new GameSlotInventory(),
                Modules = new GameGridInventory(
                    MachineHelper.DEFAULT_MODULE_BAY_CONFIG,
                    MachineHelper.DEFAULT_MODULE_BAY_WIDTH),
                Input = new GameGridInventory(12, 4), // 3x2 Junk fits 4 across x 2 rows = 8 items.
                Output = new GameGridInventory(6, 4)
            };

            PixelWindow window = MachineHelper.CreateMachineryWindow(
                canMove: true,
                MachineHelper.PowerPanel(slots.Battery),
                MachineHelper.Panel(Text("recycler.modules"), slots.Modules, "module"),
                MachineHelper.Panel(Text("recycler.input"), slots.Input, "input"),
                MachineHelper.Panel(Text("recycler.output"), slots.Output, "output"));

            GameItem machine = ItemDirectory.CreateEmptyItem(ItemId).SetContentWindow(window);
            SlotMap.Remove(machine);
            SlotMap.Add(machine, slots);

            machine.name = Text("recycler.name");
            machine.shortDescription = Text("recycler.description");
            machine.flavorText = Text("recycler.flavor");
            machine.unitValue = 0L;
            machine.SetSpriteAndShape("Items/items_tool2", "furnace");
            machine.SetGameItemType("MACHINE");
            machine.EnableTag("CONTAINER_TAG", false);
            machine.EnableTag(MachineHelper.STANDARD_MACHINE_TAG, false);
            AudioHelper.InitMetalMachine(machine);

            // Only the game's actual Junk item is accepted; weapons carrying a JUNK type are not.
            slots.Input.mayInventoryAddItemFunc =
                (GameItem item, GameInventory _) => item != null && item.identifier == "junk";
            // Players remove products from this panel; only the machine can put products into it.
            slots.Output.mayInventoryAddItemFunc = (GameItem _, GameInventory __) => false;

            MachineHelper.SetupBatterySlot(slots.Battery, machine);
            MachineHelper.SetupModuleBay(
                slots.Modules, machine, null, new string[] { "MODULE_TYPE_UNIVERSAL" });

            MachineryHelper.InitMachinery(machine, BasePower, false);
            machine.onCycleEndSlotItemFunc = delegate(
                GameItem item, GameInventory inventory, SlotMarker marker)
            {
                try { ProcessOvernightCycle(item); }
                catch (Exception ex) { sharedLog?.Error("Recycler cycle failed: " + ex); }
            };

            ModHelper.SetSource(machine, ModId, "Anime Universe — Recycler");
            return machine;
        }

        private static void ProcessOvernightCycle(GameItem machine)
        {
            if (machine == null || PlayerStore.Instance == null) return;

            Slots slots;
            if (!SlotMap.TryGetValue(machine, out slots) ||
                slots.Battery == null || slots.Input == null ||
                slots.Output == null || slots.Modules == null)
                return;

            GameItem battery = slots.Battery.childItem;
            if (battery == null) return;

            List<GameItem> junk = slots.Input.childItems
                .Where(x => x != null && x.identifier == "junk").ToList();
            int available = junk.Sum(StackUnits);
            if (available < BaseInput) return;

            int performance = MachineryHelper.GetCurrentPerformanceBonus(machine);
            bool doubleBatch =
                performance >= DoubleBatchPerformance && available >= BaseInput * 2;
            int inputCount = doubleBatch ? 8 : 4;
            int outputCount = doubleBatch ? 2 : 1;

            // The native machinery/module calculation supplies the actual draw:
            // Efficiency modules reduce it; Performance is used for the 50% batch threshold.
            // No quality bonus/tag is read or applied by this machine.
            int power = Math.Max(1, MachineryHelper.GetMachinePowerUsage(machine));
            if (!PowerHelper.CanDrawPowerSource(battery, power)) return;

            List<GameItem> products = new List<GameItem>();
            try
            {
                bool owned = GeneralHelper.IsItemOwned(machine);
                for (int i = 0; i < outputCount; i++)
                {
                    GameItem scrap = DirectoryMaster.Item("scrap_metal", owned);
                    if (scrap == null)
                        throw new InvalidOperationException("Could not create vanilla scrap_metal.");
                    products.Add(scrap);
                }

                // Secure room in the output grid before taking input or battery charge.
                slots.Output.mayInventoryAddItemFunc = (GameItem _, GameInventory __) => true;
                foreach (GameItem product in products)
                {
                    GraphUtils.TryAcceptAll(slots.Output, product);
                    if (!ReferenceEquals(product.parentInventory, slots.Output))
                        throw new InvalidOperationException("Recycler output is full.");
                }
            }
            catch (Exception ex)
            {
                RemoveProducts(slots.Output, products);
                if (ex is InvalidOperationException)
                    sharedLog?.Warning(ex.Message);
                else
                    sharedLog?.Warning("Could not prepare Recycler output: " + ex.Message);
                return;
            }
            finally
            {
                slots.Output.mayInventoryAddItemFunc = (GameItem _, GameInventory __) => false;
            }

            if (!PowerHelper.DrawPowerSource(battery, power))
            {
                RemoveProducts(slots.Output, products);
                return;
            }

            if (!ConsumeJunk(slots.Input, junk, inputCount))
            {
                RemoveProducts(slots.Output, products);
                sharedLog?.Warning("Recycler input changed during processing; provisional output was removed.");
                return;
            }

            MachineHelper.OnMachineActioned(machine, slots.Modules);
            sharedLog?.Log(
                "Recycler processed " + inputCount + " Junk into " + outputCount +
                " Scrap Metal; used " + power + " energy; performance " + performance + "%.");
        }

        private static int StackUnits(GameItem item)
        {
            return item == null ? 0 : Math.Max(1, item.unitCount);
        }

        private static bool ConsumeJunk(
            GameGridInventory input, List<GameItem> items, int required)
        {
            int remaining = required;
            foreach (GameItem item in items)
            {
                if (remaining <= 0) break;
                if (item == null || !ReferenceEquals(item.parentInventory, input)) continue;

                int count = StackUnits(item);
                if (count <= remaining)
                {
                    if (!input.Expel(item)) return false;
                    item.Destroy();
                    remaining -= count;
                }
                else
                {
                    GameItem split = item.GetSplitItem(remaining);
                    if (split == null) return false;
                    split.Destroy();
                    remaining = 0;
                }
            }
            return remaining == 0;
        }

        private static void RemoveProducts(GameGridInventory output, List<GameItem> products)
        {
            if (products == null) return;
            foreach (GameItem item in products)
            {
                if (item == null) continue;
                try
                {
                    if (ReferenceEquals(item.parentInventory, output))
                        output.Expel(item);
                    item.Destroy();
                }
                catch { }
            }
        }

        private void SpawnRecyclerForDebug()
        {
            try
            {
                GameItem machine = DirectoryMaster.Item(ItemId, true);
                if (machine == null) return;
                ModHelper.MoveItemToInventory(machine);
            }
            catch (Exception ex)
            {
                log.Error("Could not spawn a Recycler: " + ex);
            }
        }

        private void AddTooltip(RichTextBuilder builder, GameItem item)
        {
            if (builder == null || item == null || item.identifier != ItemId) return;
            try
            {
                int performance = MachineryHelper.GetCurrentPerformanceBonus(item);
                int power = MachineryHelper.GetMachinePowerUsage(item);
                builder.AddLine(Text("recycler.tooltip.recipe"), true, RenderHandler.ColorPalette.LightCyan);
                builder.AddLine(
                    ModHelper.GetLocalized(ModId, "recycler.tooltip.power", power),
                    true, RenderHandler.ColorPalette.LightCyan);
                builder.AddLine(
                    ModHelper.GetLocalized(ModId, "recycler.tooltip.performance", performance,
                        performance >= DoubleBatchPerformance ? Text("recycler.unlocked") : Text("recycler.locked")),
                    true, RenderHandler.ColorPalette.LightCyan);
                builder.AddLine(Text("recycler.tooltip.modules"), true, RenderHandler.ColorPalette.LightCyan);
            }
            catch (Exception ex)
            {
                log.Warning("Could not build Recycler tooltip: " + ex.Message);
            }
        }

        private static string Text(string key)
        {
            string value = ModHelper.GetLocalized(ModId, key, null);
            return string.IsNullOrEmpty(value) || value == key ? key : value;
        }
    }
}