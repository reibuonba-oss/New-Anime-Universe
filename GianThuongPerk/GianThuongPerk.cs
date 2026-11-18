using System;
using System.Collections.Generic;
using HarmonyLib;

namespace NewAnimeUniverse.GianThuongPerk
{
    public sealed class GianThuongPerkMod : IMod
    {
        public const string ModId = "new_anime_universe.gian_thuong";
        public const string PerkId = "new_anime_universe.gian_thuong.perk";

        internal const string BreakdownFeatureId = "new_anime_universe_gian_thuong_markup";
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
                name: () => "Gian Thương",
                description: () => "Perk Vàng, tốn 1 điểm perk. Giá bán hàng của bạn tăng 5% khi khách mua; giá mua hàng từ khách giảm 5%. Cả hai mức điều chỉnh đều hiển thị trong phần phân tích giá. Khởi đầu thêm 50 Credits, nhưng giảm 5 điểm hảo cảm với tất cả các phe.",
                iconKey: ModId + ":gian_thuong_icon");

            ModHook.OnNewGameStarted += ApplyStartingEffects;

            try
            {
                harmony = new Harmony(ModId);
                harmony.PatchAll();
                log.Log("Installed Gian Thuong: +5% player sale prices and -5% customer purchase prices, with Market breakdown entries.");
            }
            catch (Exception ex)
            {
                log.Error("Could not install the Gian Thuong trade patches: " + ex);
                harmony = null;
            }

            log.Log("Registered Gian Thuong: +5% sale price, -5% purchase price, +50 starting Credits, -5 reputation with every faction; costs 1 perk point.");
        }

        public void OnDisable()
        {
            ModHook.OnNewGameStarted -= ApplyStartingEffects;
            ModHelper.RemoveStartingPerks(ModId);

            try
            {
                harmony?.UnpatchAll(ModId);
                harmony = null;
            }
            catch (Exception ex)
            {
                log?.Warning("Could not remove Gian Thuong Harmony patches: " + ex.Message);
            }

            if (ReferenceEquals(sharedLog, log))
                sharedLog = null;

            log?.Log("Unregistered Gian Thuong.");
        }

        internal static bool IsPerkActive()
        {
            return ModHelper.IsStartingPerkActive(PerkId);
        }

        /// <summary>
        /// Returns +5 for player-owned goods sold to a customer, -5 for customer-owned
        /// goods bought by the player, and 0 outside an active customer negotiation.
        /// This follows the same ownership test used by the native negotiation item lists.
        /// </summary>
        internal static int GetTradePriceModifierPercent(GameItem item)
        {
            if (item == null || !IsPerkActive())
                return 0;

            PlayerStore store = PlayerStore.Instance;
            if (store == null || store.currentClientInstance == null)
                return 0;

            return GeneralHelper.IsItemOwned(item) ? 5 : -5;
        }

        internal static void LogPricePatchFailure(Exception ex)
        {
            if (warnedPricePatch)
                return;

            warnedPricePatch = true;
            sharedLog?.Warning("Gian Thuong could not apply its trade price adjustment: " + ex.Message);
        }

        private void ApplyStartingEffects(string scenarioId)
        {
            if (!IsPerkActive())
                return;

            // Mod data is per-run. The guard prevents duplicate grants if a new-game hook
            // is accidentally fired more than once by another mod or a future game update.
            if (ModHelper.GetModDataBool(ModId, "starting_effects_applied"))
                return;

            PlayerStore store = PlayerStore.Instance;
            if (store == null)
            {
                log?.Warning("Gian Thuong was selected, but PlayerStore was unavailable; starting effects were not applied.");
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
                "Applied Gian Thuong starting effects: +" + StartingCredits +
                " Credits and -" + StartingFactionReputationPenalty +
                " reputation with " + factionsAdjusted + " factions.");
        }
    }

    /// <summary>
    /// The negotiation rows, total, affordability checks and settlement use
    /// GameItem.GetNegociatedValue(). Apply the modifier to the shared returned
    /// value so both displayed prices and actual Credits use the same result.
    /// A prefix guard prevents nested GetNegociatedValue calls for donation
    /// recalculation from applying the percentage more than once.
    /// </summary>
    [HarmonyPatch(typeof(GameItem), "GetNegociatedValue")]
    [HarmonyPriority(Priority.Last)]
    internal static class GianThuongTradePricePatch
    {
        [ThreadStatic]
        private static HashSet<GameItem> activeCalculations;

        private static void Prefix(GameItem __instance, out bool __state)
        {
            __state = false;
            if (GianThuongPerkMod.GetTradePriceModifierPercent(__instance) == 0)
                return;

            if (activeCalculations == null)
                activeCalculations = new HashSet<GameItem>();

            // Only the outermost calculation for this item receives the modifier.
            __state = activeCalculations.Add(__instance);
        }

        private static void Postfix(GameItem __instance, bool __state, ref long __result)
        {
            if (!__state)
                return;

            try
            {
                if (__result <= 0)
                    return;

                int percent = GianThuongPerkMod.GetTradePriceModifierPercent(__instance);
                if (percent == 0)
                    return;

                decimal multiplier = 1m + ((decimal)percent / 100m);
                decimal adjusted = decimal.Round(
                    (decimal)__result * multiplier,
                    0,
                    MidpointRounding.AwayFromZero);

                if (adjusted > long.MaxValue)
                    __result = long.MaxValue;
                else if (adjusted < 0m)
                    __result = 0L;
                else
                    __result = (long)adjusted;
            }
            catch (Exception ex)
            {
                GianThuongPerkMod.LogPricePatchFailure(ex);
            }
            finally
            {
                activeCalculations?.Remove(__instance);
            }
        }

        private static Exception Finalizer(GameItem __instance, bool __state, Exception __exception)
        {
            if (__state)
                activeCalculations?.Remove(__instance);
            return __exception;
        }
    }

    /// <summary>
    /// The negotiation UI builds its "Market and Brokers" breakdown from
    /// ItemFeature entries at ValueStage.Market. Add a presentation-only feature
    /// with +5% when selling and -5% when buying. The numeric adjustment is
    /// applied exactly once by GetNegociatedValue above.
    /// </summary>
    [HarmonyPatch(typeof(NegociationUIManager), "AddFeatureSection")]
    internal static class GianThuongMarketBreakdownPatch
    {
        private static void Postfix(
            GameItem item,
            ItemFeature.ValueStage stage,
            string title,
            List<(string, List<ItemFeature>)> sections)
        {
            if (stage != ItemFeature.ValueStage.Market || sections == null)
                return;

            int priceModifier = GianThuongPerkMod.GetTradePriceModifierPercent(item);
            if (priceModifier == 0)
                return;

            try
            {
                // Avoid inserting the same line twice if the UI rebuilds this section.
                foreach (var section in sections)
                {
                    if (section.Item2 == null)
                        continue;

                    foreach (ItemFeature existing in section.Item2)
                    {
                        if (existing != null && existing.identifier == GianThuongPerkMod.BreakdownFeatureId)
                            return;
                    }
                }

                ItemFeature feature = new ItemFeature(
                    priceModifier > 0
                        ? ItemFeature.FeatureType.TemporarySelling
                        : ItemFeature.FeatureType.ClientTemporary);
                feature.identifier = GianThuongPerkMod.BreakdownFeatureId;
                feature.category = GianThuongPerkMod.BreakdownFeatureId;
                feature.valueStage = ItemFeature.ValueStage.Market;

                // Use Vietnamese directly: some Vietnamese translation patches change
                // visible UI text without changing the game's internal locale from English.
                const string displayName = "Gian Thương";
                feature.SetPublicDisplay(displayName);
                feature.SetActualDisplay(displayName);
                feature.isFeatureMatch = true;
                feature.isExposable = false;
                feature.isPublicHidden = false;
                feature.SetPreExposedValueMod(priceModifier);

                // Add it to the existing Market section, or create that section if
                // this item has no other market modifiers.
                for (int i = 0; i < sections.Count; i++)
                {
                    if (sections[i].Item1 == title && sections[i].Item2 != null)
                    {
                        sections[i].Item2.Add(feature);
                        return;
                    }
                }

                sections.Add((title, new List<ItemFeature> { feature }));
            }
            catch (Exception ex)
            {
                GianThuongPerkMod.LogPricePatchFailure(ex);
            }
        }
    }
}
