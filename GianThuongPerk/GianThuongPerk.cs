using System;

using HarmonyLib;

namespace NewAnimeUniverse.GianThuongPerk
{
    public sealed class GianThuongPerkMod : IMod
    {
        public const string ModId = "new_anime_universe.gian_thuong";
        public const string PerkId = "new_anime_universe.gian_thuong.perk";

        internal const string TradeFeatureId = "new_anime_universe_gian_thuong_trade_price";
        private const int StartingCredits = 50;
        private const double StartingFactionReputationPenalty = 5d;
        private const int PerkPointCost = 1;

        private static ModLog sharedLog;
        private static bool loggedSaleFeature;
        private static bool loggedBuyFeature;

        private ModLog log;
        private Harmony harmony;

        public void Init(ModManifest manifest)
        {
            log = new ModLog(manifest);
            sharedLog = log;
            loggedSaleFeature = false;
            loggedBuyFeature = false;
        }

        public void OnEnable()
        {
            ModHelper.AddStartingPerk(
                ModId,
                PerkId,
                cost: PerkPointCost,
                type: StartingPerk.StartingPerkType.NEUTRAL,
                name: () => "Gian Thương",
                description: () => "Perk Vàng, tốn 1 điểm perk. Khi khách mua hàng của bạn, giá bán tăng 5%. Khi bạn mua hàng từ khách, giá mua giảm 5%. Cả hai mức điều chỉnh đều hiện trong phần phân tích giá. Khởi đầu thêm 50 Credits, nhưng giảm 5 điểm hảo cảm với tất cả các phe.",
                iconKey: ModId + ":gian_thuong_icon");

            ModHook.OnNewGameStarted += ApplyStartingEffects;

            try
            {
                harmony = new Harmony(ModId);
                harmony.PatchAll();
                log.Log("Đã cài đặt Gian Thương: +5% giá bán và -5% giá mua bằng ItemFeature gốc của game.");
            }
            catch (Exception ex)
            {
                log.Error("Không thể cài các hiệu ứng giao dịch của Gian Thương: " + ex);
                harmony = null;
            }

            log.Log("Đã đăng ký Gian Thương: +5% khi bán cho khách, -5% khi mua từ khách, +50 Credits khởi đầu, -5 hảo cảm với mọi phe; tốn 1 điểm perk.");
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
                log?.Warning("Không thể gỡ Harmony patch của Gian Thương: " + ex.Message);
            }

            if (ReferenceEquals(sharedLog, log))
                sharedLog = null;

            log?.Log("Đã tắt Gian Thương.");
        }

        internal static bool IsPerkActive()
        {
            return ModHelper.IsStartingPerkActive(PerkId);
        }

        internal static void AddTradeFeature(GameItem item, int percent, ItemFeature.FeatureType featureType)
        {
            if (item == null || !IsPerkActive())
                return;

            if (item.IsAlreadyContainFeatureWithId(TradeFeatureId))
                return;

            try
            {
                ItemFeature feature = new ItemFeature(featureType);
                feature.identifier = TradeFeatureId;
                feature.category = TradeFeatureId;
                feature.valueStage = ItemFeature.ValueStage.Market;
                feature.SetPublicDisplay("Gian Thương");
                feature.SetActualDisplay("Gian Thương");
                feature.isFeatureMatch = true;
                feature.SetPreExposedValueMod(percent);

                // This is a native Market-stage ItemFeature. The game's own price
                // calculation and price-breakdown UI both read this same feature.
                item.AddItemFeature(feature);
            }
            catch (Exception ex)
            {
                sharedLog?.Error("Không thể thêm điều chỉnh giá Gian Thương vào vật phẩm " +
                    item.identifier + ": " + ex);
            }
        }

        private void ApplyStartingEffects(string scenarioId)
        {
            if (!IsPerkActive())
                return;

            if (ModHelper.GetModDataBool(ModId, "starting_effects_applied"))
                return;

            PlayerStore store = PlayerStore.Instance;
            if (store == null)
            {
                log?.Warning("Đã chọn Gian Thương nhưng PlayerStore chưa sẵn sàng; không thể áp dụng hiệu ứng khởi đầu.");
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
            log?.Log("Đã áp dụng hiệu ứng khởi đầu Gian Thương: +" + StartingCredits +
                " Credits và -" + StartingFactionReputationPenalty +
                " hảo cảm với " + factionsAdjusted + " phe.");
        }

        internal static void LogFeatureApplied(bool selling)
        {
            if (selling && !loggedSaleFeature)
            {
                loggedSaleFeature = true;
                sharedLog?.Log("Gian Thương đã thêm ItemFeature +5% vào hàng của người chơi đang bán cho khách.");
            }
            else if (!selling && !loggedBuyFeature)
            {
                loggedBuyFeature = true;
                sharedLog?.Log("Gian Thương đã thêm ItemFeature -5% vào hàng khách đang bán cho người chơi.");
            }
        }
    }

    /// <summary>
    /// The game itself adds player-owned items to its weighted negotiation area.
    /// This is the same point where the game adds RetailMarkUp and other
    /// TemporarySelling features. Gian Thuong adds a native +5% Market feature.
    /// </summary>
    [HarmonyPatch(typeof(GameItem), "OnAddedToWeightedArea")]
    internal static class GianThuongPlayerSaleFeaturePatch
    {
        private static void Postfix(GameItem __instance)
        {
            if (__instance == null || !GianThuongPerkMod.IsPerkActive())
                return;

            PlayerStore store = PlayerStore.Instance;
            StoreClient client = store?.currentClientInstance?.GetClientBlueprint();
            if (client == null)
                return;

            if (!GeneralHelper.IsItemOwned(__instance))
                return;

            if (client.clientIntent != StoreClient.ClientIntent.BUY &&
                client.clientIntent != StoreClient.ClientIntent.SELLNBUY)
                return;

            GianThuongPerkMod.AddTradeFeature(
                __instance,
                5,
                ItemFeature.FeatureType.TemporarySelling);
            GianThuongPerkMod.LogFeatureApplied(true);
        }
    }

    /// <summary>
    /// The game itself adds customer-owned items to its client area.
    /// This is the same point where the game adds ClientTemporary buying
    /// penalties/discounts. Gian Thuong adds a native -5% Market feature.
    /// </summary>
    [HarmonyPatch(typeof(GameItem), "OnAddedToClientArea")]
    internal static class GianThuongCustomerPurchaseFeaturePatch
    {
        private static void Postfix(GameItem __instance)
        {
            if (__instance == null || !GianThuongPerkMod.IsPerkActive())
                return;

            PlayerStore store = PlayerStore.Instance;
            StoreClient client = store?.currentClientInstance?.GetClientBlueprint();
            if (client == null)
                return;

            if (GeneralHelper.IsItemOwned(__instance))
                return;

            if (client.clientIntent != StoreClient.ClientIntent.SELL &&
                client.clientIntent != StoreClient.ClientIntent.SELLNBUY)
                return;

            GianThuongPerkMod.AddTradeFeature(
                __instance,
                -5,
                ItemFeature.FeatureType.ClientTemporary);
            GianThuongPerkMod.LogFeatureApplied(false);
        }
    }
}
