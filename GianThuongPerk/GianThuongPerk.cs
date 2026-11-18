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
        private const decimal SaleBonusRate = 0.05m;

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

            try
            {
                harmony = new Harmony(ModId);
                harmony.PatchAll();
                log.Log("Installed Gian Thuong price and Market-breakdown patches.");
            }
            catch (Exception ex)
            {
                log.Error("Could not install the Gian Thuong trade patches: " + ex);
                harmony = null;
            }

            log.Log("Registered Gian Thuong: +5% to the displayed sale price, +50 starting Credits, -5 reputation with every faction; costs 1 perk point.");
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

        internal static bool IsPlayerSellingToCustomer(GameItem item)
        {
            if (item == null || !IsPerkActive())
                return false;

            PlayerStore store = PlayerStore.Instance;
            if (store == null || store.currentClientInstance == null)
                return false;

            // Only player-owned stock sold to the current customer receives the bonus.
            return GeneralHelper.IsItemOwned(item);
        }

        internal static void LogPricePatchFailure(Exception ex)
        {
            if (warnedPricePatch)
                return;

            warnedPricePatch = true;
            sharedLog?.Warning("Gian Thuong could not apply its sale-price bonus: " + ex.Message);
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
    /// The negotiation list, total, affordability check, and settlement all use
    /// GameItem.GetNegociatedValue(). Adjusting this shared result updates the
    /// displayed item price and the amount paid, without changing customer-to-player buys.
    /// </summary>
    [HarmonyPatch(typeof(GameItem), "GetNegociatedValue")]
    [HarmonyPriority(Priority.Last)]
    internal static class GianThuongSalePricePatch
    {
        private static void Postfix(GameItem __instance, ref long __result)
        {
            try
            {
                if (__result <= 0 || !GianThuongPerkMod.IsPlayerSellingToCustomer(__instance))
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

    /// <summary>
    /// The negotiation UI builds its "Market and Brokers" breakdown from
    /// ItemFeature entries at ValueStage.Market. Add a presentation-only feature
    /// there so the 5% line appears next to the increased value.
    /// The numeric increase itself is applied once by GetNegociatedValue above.
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
            if (stage != ItemFeature.ValueStage.Market ||
                sections == null ||
                !GianThuongPerkMod.IsPlayerSellingToCustomer(item))
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

                ItemFeature feature = new ItemFeature(ItemFeature.FeatureType.TemporarySelling);
                feature.identifier = GianThuongPerkMod.BreakdownFeatureId;
                feature.category = GianThuongPerkMod.BreakdownFeatureId;
                feature.valueStage = ItemFeature.ValueStage.Market;

                string displayName = ModHelper.GetLocalized(
                    GianThuongPerkMod.ModId,
                    "perk_gian_thuong_market_feature",
                    null);
                if (string.IsNullOrEmpty(displayName) || displayName == "perk_gian_thuong_market_feature")
                    displayName = "Gian Thương";

                feature.SetPublicDisplay(displayName);
                feature.SetActualDisplay(displayName);
                feature.isFeatureMatch = true;
                feature.isExposable = false;
                feature.isPublicHidden = false;
                feature.SetPreExposedValueMod(5);

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
