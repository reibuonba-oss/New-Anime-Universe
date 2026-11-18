using System;
using HarmonyLib;

namespace NewAnimeUniverse.GianThuongPerk
{
    public sealed class GianThuongPerkMod : IMod
    {
        public const string ModId = "new_anime_universe.gian_thuong";
        public const string PerkId = "new_anime_universe.gian_thuong.perk";

        private const int StartingCredits = 50;
        private const double StartingFactionReputationPenalty = 5d;
        private const int PerkPointCost = 1;

        private static ModLog sharedLog;
        private static bool warnedPricePatch;

        private ModLog log;
        private Harmony harmony;

        public void Init(ModManifest manifest)
        {
            log = new ModLog(manifest);
            sharedLog = log;
            warnedPricePatch = false;
        }

        public void OnEnable()
        {
            ModHelper.AddStartingPerk(
                ModId,
                PerkId,
                // Keep the native amber/gold Neutral appearance while charging one perk point.
                cost: PerkPointCost,
                type: StartingPerk.StartingPerkType.NEUTRAL,
                name: () => ModHelper.GetLocalized(ModId, "perk_gian_thuong_name"),
                description: () => ModHelper.GetLocalized(ModId, "perk_gian_thuong_desc"),
                iconKey: ModId + ":gian_thuong_icon");

            ModHook.OnNewGameStarted += ApplyStartingEffects;
            ModHook.OnCreateTooltipLate += AddSalePriceTooltip;

            try
            {
                harmony = new Harmony(ModId);
                harmony.PatchAll();
                log.Log("Patched GameItem.GetNegociatedValue so the +5% appears in the negotiation list and is paid on settlement.");
            }
            catch (Exception ex)
            {
                log.Error("Could not install the Gian Thương sale-price patch: " + ex);
                harmony = null;
            }

            log.Log("Registered Gian Thương: +5% on customer purchases, +50 starting Credits, -5 reputation with every faction; costs 1 perk point.");
        }

        public void OnDisable()
        {
            ModHook.OnNewGameStarted -= ApplyStartingEffects;
            ModHook.OnCreateTooltipLate -= AddSalePriceTooltip;
            ModHelper.RemoveStartingPerks(ModId);

            try
            {
                Harmony.UnpatchAll(ModId);
                harmony = null;
            }
            catch (Exception ex)
            {
                log?.Warning("Could not remove Gian Thương Harmony patches: " + ex.Message);
            }

            if (ReferenceEquals(sharedLog, log))
                sharedLog = null;

            log?.Log("Unregistered Gian Thương.");
        }

        internal static bool IsPerkActive()
        {
            return ModHelper.IsStartingPerkActive(PerkId);
        }

        internal static void LogPricePatchFailure(Exception ex)
        {
            if (warnedPricePatch)
                return;

            warnedPricePatch = true;
            sharedLog?.Warning("Gian Thương could not apply its sale-price bonus: " + ex.Message);
        }

        private void ApplyStartingEffects(string scenarioId)
        {
            if (!IsPerkActive())
                return;

            // Mod data is per-run. The guard prevents accidental duplicate grants if
            // another mod or a future game update fires the new-game hook more than once.
            if (ModHelper.GetModDataBool(ModId, "starting_effects_applied"))
                return;

            PlayerStore store = PlayerStore.Instance;
            if (store == null)
            {
                log?.Warning("Gian Thương was selected, but PlayerStore was unavailable; starting effects were not applied.");
                return;
            }

            ModHelper.AddCredit(StartingCredits, false);

            int factionsAdjusted = 0;
            if (store.storeReputations != null)
            {
                foreach (StoreReputation faction in store.storeReputations)
                {
                    if (faction == null)
                        continue;

                    faction.ModReputation(-StartingFactionReputationPenalty);
                    factionsAdjusted++;
                }
            }

            ModHelper.SetModDataBool(ModId, "starting_effects_applied", true);
            log?.Log(
                "Applied Gian Thương starting effects: +" + StartingCredits +
                " Credits and -" + StartingFactionReputationPenalty +
                " reputation with " + factionsAdjusted + " factions.");
        }

        private void AddSalePriceTooltip(RichTextBuilder builder, GameItem item)
        {
            if (builder == null || item == null || !IsPerkActive())
                return;

            try
            {
                PlayerStore store = PlayerStore.Instance;
                if (store == null || store.currentClientInstance == null)
                    return;

                // Only the player's goods are being sold to the current customer.
                // Items offered by a customer are not eligible for the +5% bonus.
                if (!GeneralHelper.IsItemOwned(item))
                    return;

                string line = ModHelper.GetLocalized(ModId, "perk_gian_thuong_trade_line", null);
                if (string.IsNullOrEmpty(line) || line == "perk_gian_thuong_trade_line")
                    line = "Gian Thương: +5% giá bán (đã tính vào giá hiển thị)";

                builder.AddLine(line, true, RenderHandler.ColorPalette.LightCyan);
            }
            catch (Exception ex)
            {
                log?.Warning("Could not add the Gian Thương trade tooltip: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// The negotiation list and the cash settlement both call
    /// GameItem.GetNegociatedValue(). Changing the shared result keeps
    /// the quoted price and the Credits actually received in sync.
    /// </summary>
    [HarmonyPatch(typeof(GameItem), "GetNegociatedValue")]
    [HarmonyPriority(Priority.Last)]
    internal static class GianThuongSalePricePatch
    {
        private static void Postfix(GameItem __instance, ref long __result)
        {
            try
            {
                if (__instance == null || __result <= 0 || !GianThuongPerkMod.IsPerkActive())
                    return;

                PlayerStore store = PlayerStore.Instance;
                if (store == null || store.currentClientInstance == null)
                    return;

                // Only add the bonus when a customer buys an item owned by the player.
                // Purchases made by the player from a customer remain unchanged.
                if (!GeneralHelper.IsItemOwned(__instance))
                    return;

                decimal bonus = decimal.Round(
                    (decimal)__result * 0.05m,
                    0,
                    MidpointRounding.AwayFromZero);

                if (bonus <= 0m)
                    return;

                decimal adjusted = (decimal)__result + bonus;
                __result = adjusted >= long.MaxValue ? long.MaxValue : (long)adjusted;
            }
            catch (Exception ex)
            {
                GianThuongPerkMod.LogPricePatchFailure(ex);
            }
        }
    }
}
