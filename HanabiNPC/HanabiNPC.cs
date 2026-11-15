using System;
using UnityEngine;

namespace NewAnimeUniverse.HanabiNPC
{
    /// <summary>
    /// Hanabi is a rare merchant: she sells a custom mask and gun,
    /// and buys power-source items and modules from the player.
    /// </summary>
    public sealed class HanabiNPCMod : IMod
    {
        private const string ModId = "new_anime_universe.hanabi_merchant";
        private const string ClientId = "new_anime_universe_hanabi";
        private const string PortraitKey = "new_anime_universe_hanabi";
        private const string PortraitTextureKey = "hanabi";

        public const string MaskItemId = "hanabi_mask";
        public const string GunItemId = "hanabi_gun";

        // These are the requested base market values. Hanabi's own stock is marked up by 5%.
        private const long MaskMarketValue = 888L;
        private const long GunMarketValue = 999L;
        private const double HanabiMarkup = 1.05d;
        private const int DailyAppearanceChance = 10;
        private const int PurchaseBudget = 9999;

        private ModLog log;
        private Sprite hanabiPortrait;
        private Sprite previousPortrait;
        private bool hadPreviousPortrait;
        private bool portraitInjected;

        public void Init(ModManifest manifest)
        {
            log = new ModLog(manifest);
            log.Log("Hanabi merchant initialized.");
        }

        public void OnEnable()
        {
            ModHook.OnModItemDirectoryInit += RegisterItems;
            ModHook.OnModAssetsLoaded += OnModAssetsLoaded;
            ModHook.OnGameLoadedNormal += OnGameLoaded;
            ModHook.OnGenerateCustomerVeryLate += OnGenerateCustomerVeryLate;
        }

        public void OnDisable()
        {
            ModHook.OnModItemDirectoryInit -= RegisterItems;
            ModHook.OnModAssetsLoaded -= OnModAssetsLoaded;
            ModHook.OnGameLoadedNormal -= OnGameLoaded;
            ModHook.OnGenerateCustomerVeryLate -= OnGenerateCustomerVeryLate;
            RestorePortrait();
        }

        private void RegisterItems(ModItemDirectory directory)
        {
            if (directory == null)
                return;

            directory.Add(MaskItemId, CreateHanabiMask);
            directory.Add(GunItemId, CreateHanabiGun);
            log.Log("Registered Hanabi Mask and Hanabi Gun.");
        }

        private static GameItem CreateHanabiMask()
        {
            GameItem item = ItemDirectory.CreateEmptyItem(MaskItemId);
            item.name = Text("hanabi.mask.name", "Hanabi Mask");
            item.shortDescription = Text("hanabi.mask.description", "A decorative mask designed for Hanabi.");
            item.flavorText = Text("hanabi.mask.flavor", "A striking mask with a delicate floral motif.");
            item.unitValue = MaskMarketValue;

            // The mask deliberately belongs to both requested categories.
            item.SetGameItemType("HOUSEHOLD_GOOD");
            item.SetGameItemType("LUXURY_ITEM");
            item.SetSpriteAndShapeFromMod(ModId, "hanabi_mask");
            ModHelper.SetSource(item, ModId, "New Anime Universe");
            return item;
        }

        private static GameItem CreateHanabiGun()
        {
            GameItem item = ItemDirectory.CreateEmptyItem(GunItemId);
            item.name = Text("hanabi.gun.name", "Hanabi Gun");
            item.shortDescription = Text("hanabi.gun.description", "A custom pink revolver made for Hanabi.");
            item.flavorText = Text("hanabi.gun.flavor", "A bright pink finish and a small cherry-blossom emblem.");
            item.unitValue = GunMarketValue;

            // This adds the WEAPON classification. It is a custom trade item, not a scripted firing mechanic.
            item.SetGameItemType("WEAPON");
            item.SetSpriteAndShapeFromMod(ModId, "hanabi_gun");
            ModHelper.SetSource(item, ModId, "New Anime Universe");
            return item;
        }

        private void OnModAssetsLoaded(LoadedMod loadedMod)
        {
            if (loadedMod == null || loadedMod.Manifest == null || loadedMod.Manifest.ID != ModId)
                return;

            try
            {
                if (loadedMod.Textures == null || !loadedMod.Textures.ContainsKey(PortraitTextureKey))
                {
                    log.Error("Textures/hanabi.png was not loaded; Hanabi's portrait is unavailable.");
                    return;
                }

                Texture2D texture = loadedMod.Textures[PortraitTextureKey];
                if (texture == null)
                {
                    log.Error("Hanabi portrait texture is null.");
                    return;
                }

                hanabiPortrait = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                UnityEngine.Object.DontDestroyOnLoad(hanabiPortrait);
                RegisterPortrait();
                log.Log("Hanabi portrait loaded.");
            }
            catch (Exception ex)
            {
                log.Error("Could not load Hanabi portrait: " + ex);
            }
        }

        private void OnGameLoaded()
        {
            RegisterPortrait();
        }

        private bool RegisterPortrait()
        {
            if (hanabiPortrait == null || SpriteDict.Instance == null || SpriteDict.Instance.spriteDictionary == null)
                return false;

            try
            {
                if (!portraitInjected)
                {
                    hadPreviousPortrait = SpriteDict.Instance.spriteDictionary.ContainsKey(PortraitKey);
                    if (hadPreviousPortrait)
                        previousPortrait = SpriteDict.Instance.spriteDictionary[PortraitKey];
                }

                SpriteDict.Instance.spriteDictionary[PortraitKey] = hanabiPortrait;
                portraitInjected = true;
                return true;
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.Warning("Hanabi portrait registration will be retried: " + ex.Message);
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
                    log.Warning("Could not restore Hanabi's portrait registration: " + ex.Message);
            }

            portraitInjected = false;
            previousPortrait = null;
            hadPreviousPortrait = false;
            if (hanabiPortrait != null)
            {
                UnityEngine.Object.Destroy(hanabiPortrait);
                hanabiPortrait = null;
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

            // Roll once per in-game day; a successful day can enqueue Hanabi only once.
            if (lastRollDay == day || lastVisitDay == day)
                return;

            ModHelper.SetModDataInt(ModId, "last_roll_day", day);
            if (RNG.GetRandomInt(1, 100) > DailyAppearanceChance)
                return;

            try
            {
                StoreClient hanabi = CreateHanabi(day);
                manager.AddClient(hanabi);
                ModHelper.SetModDataInt(ModId, "last_visit_day", day);
                log.Log("Hanabi was queued for day " + day + " (10% daily roll succeeded).");
            }
            catch (Exception ex)
            {
                log.Error("Could not create Hanabi: " + ex);
            }
        }

        private StoreClient CreateHanabi(int day)
        {
            StoreClient hanabi = new StoreClient();
            hanabi.identifier = ClientId;
            hanabi.displayName = "Hanabi";

            StoreClientFactionSetup.InitUpperLevelCitizen(hanabi);
            hanabi.spriteName = PortraitKey;
            hanabi.clientFaction = StoreClient.FACTION_UPPER;
            hanabi.clientIntent = StoreClient.ClientIntent.SELLNBUY;
            hanabi.SetBudget(PurchaseBudget + Math.Max(0, day) * 25);

            // Hanabi buys batteries/power sources and modules from the player.
            hanabi.clientBuyingIdList.Clear();
            hanabi.clientBuyingTagList.Clear();
            hanabi.clientBuyingTagList.Add("power_source_item");
            hanabi.clientBuyingTagList.Add("MODULE");

            hanabi.mainDialogue = new Dialogue();
            hanabi.mainDialogue
                .SetText("Hanabi", Text("hanabi.greeting", "Hello there. I'm Hanabi. I brought two special pieces for today's visit."))
                .NextDialogue()
                .SetText("Hanabi", Text("hanabi.buy", "I'm also buying power cells and modules. Show me what you have."))
                .NextDialogue()
                .SetText("Hanabi", Text("hanabi.stock", "The Hanabi Mask and Hanabi Gun are both available today. My prices include a small 5% premium over their base market values."))
                .SetEndAction(PlaceHanabiStock);

            hanabi.AddBasicDialog();
            hanabi.CompleteClientCreation();
            return hanabi;
        }

        private void PlaceHanabiStock()
        {
            PlayerStore store = PlayerStore.Instance;
            if (store == null)
            {
                log.Warning("PlayerStore was unavailable when Hanabi's stock was placed.");
                return;
            }

            int placed = 0;
            placed += PlaceMarkedUpItem(store, MaskItemId, MaskMarketValue);
            placed += PlaceMarkedUpItem(store, GunItemId, GunMarketValue);
            log.Log("Hanabi's stock was placed: " + placed + " item(s).");
        }

        private int PlaceMarkedUpItem(PlayerStore store, string itemId, long marketValue)
        {
            try
            {
                GameItem item = ModHelper.SpawnItem(itemId);
                if (item == null)
                {
                    log.Warning("Could not spawn Hanabi stock item: " + itemId);
                    return 0;
                }

                // StoreClient.sellPriceModifier is not relied on: apply the premium to the actual listed item's value.
                item.unitValue = (long)Math.Round(marketValue * HanabiMarkup, MidpointRounding.AwayFromZero);
                store.AddDirectSellingItemToTable(item, false, false, false, 0);
                return 1;
            }
            catch (Exception ex)
            {
                log.Warning("Could not place " + itemId + " on Hanabi's table: " + ex.Message);
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