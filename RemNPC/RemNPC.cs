using System;
using UnityEngine;

namespace NewAnimeUniverse.RemNPC
{
    /// <summary>
    /// Rem: an upper-level household merchant who sells luxury goods and buys food/water.
    /// Appearance chance is rolled once per in-game day (11%).
    /// </summary>
    public sealed class RemNPCMod : IMod
    {
        private const string ModId = "new_anime_universe.rem_merchant";
        private const string ClientId = "new_anime_universe_rem";
        private const string PortraitKey = "new_anime_universe_rem";
        private const string PortraitTextureKey = "rem";

        // Vanilla water products explicitly accepted by Rem. FOOD is accepted through the FOOD type.
        private static readonly string[] WaterItemIds =
        {
            "bottled_water",
            "bottled_water_premium",
            "small_bottled_water",
            "mini_bottle",
            "large_bottled_water",
            "water_jug",
            "water_pitcher",
            "water_ration",
            "water_ration_small"
        };

        // Vanilla items tagged as luxury goods in the game's item catalogue.
        private static readonly string[] LuxuryItemIds =
        {
            "box_tampon",
            "water_filter_adv",
            "portable_water_purifier",
            "skincare_cream",
            "neuroactive_perfume",
            "pheromone_perfume",
            "bottled_water_premium",
            "black_injector",
            "blue_blood_bag",
            "cassette1",
            "cassette2",
            "cassette3",
            "cassette4",
            "cassette5",
            "cassette6",
            "cassette7"
        };

        private ModLog log;
        private Sprite remPortrait;
        private Sprite previousPortrait;
        private bool hadPreviousPortrait;
        private bool portraitInjected;

        public void Init(ModManifest manifest)
        {
            log = new ModLog(manifest);
            log.Log("Rem merchant initialized.");
        }

        public void OnEnable()
        {
            ModHook.OnModAssetsLoaded += OnModAssetsLoaded;
            ModHook.OnGameLoadedNormal += OnGameLoaded;
            ModHook.OnGenerateCustomerVeryLate += OnGenerateCustomerVeryLate;
        }

        public void OnDisable()
        {
            ModHook.OnModAssetsLoaded -= OnModAssetsLoaded;
            ModHook.OnGameLoadedNormal -= OnGameLoaded;
            ModHook.OnGenerateCustomerVeryLate -= OnGenerateCustomerVeryLate;
            RestorePortrait();
        }

        private void OnModAssetsLoaded(LoadedMod loadedMod)
        {
            if (loadedMod == null || loadedMod.Manifest == null || loadedMod.Manifest.ID != ModId)
                return;

            try
            {
                if (loadedMod.Textures == null || !loadedMod.Textures.ContainsKey(PortraitTextureKey))
                {
                    log.Error("Textures/rem.png was not loaded. Rem will not be queued without her portrait.");
                    return;
                }

                Texture2D texture = loadedMod.Textures[PortraitTextureKey];
                if (texture == null)
                {
                    log.Error("The Rem portrait texture is null.");
                    return;
                }

                remPortrait = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                UnityEngine.Object.DontDestroyOnLoad(remPortrait);
                RegisterPortrait();
                log.Log("Rem portrait loaded and registered.");
            }
            catch (Exception ex)
            {
                log.Error("Could not register Rem portrait: " + ex);
            }
        }

        private void OnGameLoaded()
        {
            RegisterPortrait();
        }

        private bool RegisterPortrait()
        {
            if (remPortrait == null || SpriteDict.Instance == null || SpriteDict.Instance.spriteDictionary == null)
                return false;

            try
            {
                if (!portraitInjected)
                {
                    hadPreviousPortrait = SpriteDict.Instance.spriteDictionary.ContainsKey(PortraitKey);
                    if (hadPreviousPortrait)
                        previousPortrait = SpriteDict.Instance.spriteDictionary[PortraitKey];
                }

                SpriteDict.Instance.spriteDictionary[PortraitKey] = remPortrait;
                portraitInjected = true;
                return true;
            }
            catch (Exception ex)
            {
                log.Warning("Portrait registration will be retried: " + ex.Message);
                return false;
            }
        }

        private void RestorePortrait()
        {
            if (!portraitInjected)
                return;

            try
            {
                if (SpriteDict.Instance != null && SpriteDict.Instance.spriteDictionary != null)
                {
                    if (hadPreviousPortrait && previousPortrait != null)
                        SpriteDict.Instance.spriteDictionary[PortraitKey] = previousPortrait;
                    else
                        SpriteDict.Instance.spriteDictionary.Remove(PortraitKey);
                }
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.Warning("Could not restore the portrait dictionary entry: " + ex.Message);
            }

            portraitInjected = false;
            previousPortrait = null;
            hadPreviousPortrait = false;
            if (remPortrait != null)
            {
                UnityEngine.Object.Destroy(remPortrait);
                remPortrait = null;
            }
        }

        private void OnGenerateCustomerVeryLate(StoreClientManager manager)
        {
            if (manager == null || PlayerStore.Instance == null)
                return;

            if (!RegisterPortrait())
                return;

            int day = StoreStation.GetDayCounter();
            int lastRollDay = ModHelper.GetModDataInt(ModId, "last_roll_day", -1);
            int lastVisitDay = ModHelper.GetModDataInt(ModId, "last_visit_day", -1);

            // One independent 11% roll per in-game day; Rem can visit at most once per day.
            if (lastRollDay == day || lastVisitDay == day)
                return;

            ModHelper.SetModDataInt(ModId, "last_roll_day", day);
            if (RNG.GetRandomInt(1, 100) > 11)
                return;

            try
            {
                StoreClient rem = CreateRem(day);
                manager.AddClient(rem);
                ModHelper.SetModDataInt(ModId, "last_visit_day", day);
                log.Log("Rem was queued for day " + day + " (11% daily roll succeeded).");
            }
            catch (Exception ex)
            {
                log.Error("Could not create Rem: " + ex);
            }
        }

        private StoreClient CreateRem(int day)
        {
            StoreClient rem = new StoreClient();
            rem.identifier = ClientId;
            rem.displayName = "Rem";

            // Start with the game's upper-level citizen setup, then allow both selling and buying.
            StoreClientFactionSetup.InitUpperLevelCitizen(rem);
            rem.spriteName = PortraitKey;
            rem.clientFaction = StoreClient.FACTION_UPPER;
            rem.clientIntent = StoreClient.ClientIntent.SELLNBUY;
            rem.SetBudget(1600 + Math.Max(0, day) * 20);

            rem.clientBuyingIdList.Clear();
            for (int i = 0; i < WaterItemIds.Length; i++)
                rem.clientBuyingIdList.Add(WaterItemIds[i]);

            rem.clientBuyingTagList.Clear();
            rem.clientBuyingTagList.Add("FOOD");

            rem.mainDialogue = new Dialogue();
            rem.mainDialogue
                .SetText("Rem", Text("rem.greeting", "Xin chào. Em là Rem. Hôm nay em mang đến vài món hàng xa xỉ được tuyển chọn cẩn thận."))
                .NextDialogue()
                .SetText("Rem", Text("rem.offer", "Anh cứ thong thả xem hàng nhé. Mỗi món đều được Rem lựa chọn rất kỹ."))
                .NextDialogue()
                .SetText("Rem", Text("rem.buy_food_water", "Rem cũng thu mua thực phẩm và nước sạch. Nếu anh có hàng dư, em rất sẵn lòng mua lại ạ."))
                .SetEndAction(PlaceLuxuryGoods);

            rem.AddBasicDialog();
            rem.CompleteClientCreation();
            return rem;
        }

        private void PlaceLuxuryGoods()
        {
            PlayerStore store = PlayerStore.Instance;
            if (store == null)
            {
                log.Warning("PlayerStore was unavailable when Rem's stock was placed.");
                return;
            }

            // Four randomly selected luxury products are offered on each visit.
            int placed = 0;
            for (int i = 0; i < 4; i++)
            {
                string itemId = RNG.GetRandomElement(LuxuryItemIds);
                GameItem item = DirectoryMaster.Item(itemId, true);
                if (item == null)
                {
                    log.Warning("Luxury item could not be created: " + itemId);
                    continue;
                }

                store.AddDirectSellingItemToTable(item, false, false, false, 0);
                placed++;
            }

            log.Log("Rem's luxury stock was placed: " + placed + " item(s).");
        }

        private static string Text(string key, string fallback)
        {
            return fallback;
        }
    }
}