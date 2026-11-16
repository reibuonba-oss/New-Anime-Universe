using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewAnimeUniverse.KaitoNPC
{
    /// <summary>
    /// Kaito buys raw materials and batteries, and sells only the Recycler
    /// and Advanced Battery Charger from the Anime Universe machine mods.
    /// </summary>
    public sealed class KaitoNPCMod : IMod
    {
        private const string ModId = "new_anime_universe.kaito_collector";
        private const string ClientId = "new_anime_universe_kaito";
        private const string PortraitKey = "new_anime_universe_kaito";
        private const string PortraitTextureKey = "kaito";
        private const int DailyAppearanceChance = 8;

        // Buyer whitelist: material classes, energy cells, plus the exact raw
        // recycling inputs used by the Anime Universe Recycler.
        private static readonly string[] BuyingTagIds =
        {
            "BASIC_MATERIAL",
            "REFINED_MATERIAL",
            "HAND_MATERIAL",
            "ENERGY_CELL"
        };

        private static readonly string[] BuyingItemIds =
        {
            "junk",
            "scrap_metal"
        };

        // Kaito sells only these two machines. They are supplied by the
        // separate Anime Universe Recycler and Advanced Charger mods.
        private static readonly string[] SaleItemIds =
        {
            "nau_recycler",
            "nau_advanced_charger"
        };

        // Fixed Kaito counter prices. These values are applied to the stock copies only;
        // the standalone machine items keep their own default item values elsewhere.
        private const long RecyclerSalePrice = 399L;
        private const long AdvancedChargerSalePrice = 799L;

        private ModLog log;
        private Sprite kaitoPortrait;
        private Sprite previousPortrait;
        private bool hadPreviousPortrait;
        private bool portraitInjected;

        public void Init(ModManifest manifest)
        {
            log = new ModLog(manifest);
            log.Log("Kaito collector initialized.");
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
                    log.Error("Textures/kaito.png was not loaded. Kaito will not be queued without his portrait.");
                    return;
                }

                Texture2D texture = loadedMod.Textures[PortraitTextureKey];
                if (texture == null)
                {
                    log.Error("Kaito portrait texture is null.");
                    return;
                }

                kaitoPortrait = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                UnityEngine.Object.DontDestroyOnLoad(kaitoPortrait);
                RegisterPortrait();
                log.Log("Kaito portrait loaded and registered.");
            }
            catch (Exception ex)
            {
                log.Error("Could not register Kaito portrait: " + ex);
            }
        }

        private void OnGameLoaded()
        {
            RegisterPortrait();
        }

        private bool RegisterPortrait()
        {
            if (kaitoPortrait == null || SpriteDict.Instance == null || SpriteDict.Instance.spriteDictionary == null)
                return false;

            try
            {
                if (!portraitInjected)
                {
                    hadPreviousPortrait = SpriteDict.Instance.spriteDictionary.ContainsKey(PortraitKey);
                    if (hadPreviousPortrait)
                        previousPortrait = SpriteDict.Instance.spriteDictionary[PortraitKey];
                }

                SpriteDict.Instance.spriteDictionary[PortraitKey] = kaitoPortrait;
                portraitInjected = true;
                return true;
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.Warning("Kaito portrait registration will be retried: " + ex.Message);
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
                    log.Warning("Could not restore Kaito portrait registration: " + ex.Message);
            }

            portraitInjected = false;
            previousPortrait = null;
            hadPreviousPortrait = false;
            if (kaitoPortrait != null)
            {
                UnityEngine.Object.Destroy(kaitoPortrait);
                kaitoPortrait = null;
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

            // One 8% roll per in-game day; at most one visit per day.
            if (lastRollDay == day || lastVisitDay == day)
                return;

            ModHelper.SetModDataInt(ModId, "last_roll_day", day);
            if (RNG.GetRandomInt(1, 100) > DailyAppearanceChance)
                return;

            try
            {
                StoreClient kaito = CreateKaito(day);
                manager.AddClient(kaito);
                ModHelper.SetModDataInt(ModId, "last_visit_day", day);
                log.Log("Kaito was queued for day " + day + " (8% daily roll succeeded).");
            }
            catch (Exception ex)
            {
                log.Error("Could not create Kaito: " + ex);
            }
        }

        private StoreClient CreateKaito(int day)
        {
            StoreClient kaito = new StoreClient();
            kaito.identifier = ClientId;
            kaito.displayName = "Kaito";

            StoreClientFactionSetup.InitUpperLevelCitizen(kaito);
            kaito.spriteName = PortraitKey;
            kaito.clientFaction = StoreClient.FACTION_UPPER;
            kaito.clientIntent = StoreClient.ClientIntent.SELLNBUY;

            // A moderate buying budget limits the economic impact of a rare collector.
            kaito.SetBudget(4800 + Math.Max(0, day) * 25);

            kaito.clientBuyingIdList.Clear();
            for (int i = 0; i < BuyingItemIds.Length; i++)
                kaito.clientBuyingIdList.Add(BuyingItemIds[i]);

            kaito.clientBuyingTagList.Clear();
            for (int i = 0; i < BuyingTagIds.Length; i++)
                kaito.clientBuyingTagList.Add(BuyingTagIds[i]);

            kaito.mainDialogue = new Dialogue();
            kaito.mainDialogue
                .SetText("Kaito", Text("kaito.greeting", "Hello. I'm Kaito. I deal in useful raw materials and power cells."))
                .NextDialogue()
                .SetText("Kaito", Text("kaito.buy", "I buy raw materials and batteries only. No weapons, modules or luxury goods."))
                .NextDialogue()
                .SetText("Kaito", Text("kaito.stock", "I sell two machines: the Recycler and the Advanced Battery Charger."))
                .NextDialogue()
                .SetText("Kaito", Text("kaito.farewell", "Good equipment lasts. Take care of it, and it will take care of you."))
                .SetEndAction(PlaceKaitoStock);

            kaito.AddBasicDialog();
            kaito.CompleteClientCreation();
            return kaito;
        }

        private void PlaceKaitoStock()
        {
            PlayerStore store = PlayerStore.Instance;
            if (store == null)
            {
                log.Warning("PlayerStore was unavailable when Kaito's stock was placed.");
                return;
            }

            int placed = 0;

            // Only modded machine stock is allowed. If a machine mod is disabled,
            // DirectoryMaster.Item returns null and that item is simply skipped.
            for (int i = 0; i < SaleItemIds.Length; i++)
                placed += PlaceMachineItem(store, SaleItemIds[i]);

            log.Log("Kaito's machine-only stock was placed: " + placed + " item(s).");
        }

        private int PlaceMachineItem(PlayerStore store, string itemId)
        {
            try
            {
                GameItem item = DirectoryMaster.Item(itemId, true);
                if (item == null)
                {
                    log.Warning("Kaito machine stock is unavailable (is its mod enabled?): " + itemId);
                    return 0;
                }

                long salePrice;
                if (itemId == "nau_recycler")
                    salePrice = RecyclerSalePrice;
                else if (itemId == "nau_advanced_charger")
                    salePrice = AdvancedChargerSalePrice;
                else
                {
                    item.Destroy();
                    log.Warning("Refused to place an item without a configured Kaito sale price: " + itemId);
                    return 0;
                }

                // AddDirectSellingItemToTable takes the stock item, not a separate price argument.
                // Set the value on this individual stock copy before placing it on the counter.
                item.unitValue = salePrice;
                store.AddDirectSellingItemToTable(item, false, false, false, 0);
                log.Log("Placed " + itemId + " on Kaito's table for " + salePrice + " credits.");
                return 1;
            }
            catch (Exception ex)
            {
                log.Warning("Could not place machine " + itemId + " on Kaito's table: " + ex.Message);
                return 0;
            }
        }

        private static string Text(string key, string fallback)
        {
            string value = ModHelper.GetLocalized(ModId, key, null);
            if (string.IsNullOrEmpty(value) || value == key)
                return fallback;
            return value;
        }
    }
}