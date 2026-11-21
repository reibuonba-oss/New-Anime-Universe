using System;
using UnityEngine;

namespace NewAnimeUniverse.ReiNPC
{
    /// <summary>
    /// Rei is a rare merchant who sells Red Dragon energy drinks and buys luxury goods.
    /// The drink uses the game's native beverage and fridge value features.
    /// </summary>
    public sealed class ReiNPCMod : IMod
    {
        private const string ModId = "new_anime_universe.rei_red_dragon";
        private const string ClientId = "new_anime_universe_rei";
        private const string PortraitKey = "new_anime_universe_rei";
        private const string PortraitTextureKey = "rei";

        public const string DrinkItemId = "nau_red_dragon_energy";
        private const long BaseDrinkPrice = 15L;
        private const double WholesaleDiscount = 0.08d;

        private const int DailyAppearanceChance = 25;
        private const int EarlyPurchaseBudget = 250;
        private const int LatePurchaseBudget = 450;
        private const int LateBudgetStartsDay = 30;

        private ModLog log;
        private Sprite reiPortrait;
        private Sprite previousPortrait;
        private bool hadPreviousPortrait;
        private bool portraitInjected;

        public void Init(ModManifest manifest)
        {
            log = new ModLog(manifest);
            log.Log("Rei merchant initialized.");
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

            directory.Add(DrinkItemId, CreateRedDragonEnergyDrink);
            log.Log("Registered Red Dragon Energy Drink.");
        }

        private static GameItem CreateRedDragonEnergyDrink()
        {
            GameItem item = ItemDirectory.CreateEmptyItem(DrinkItemId);
            item.name = Text("rei.drink.name", "Red Dragon Energy Drink");
            item.shortDescription = Text("rei.drink.description", "A strawberry energy drink. It can be chilled in a refrigerator.");
            item.flavorText = Text("rei.drink.flavor", "A vivid red can with a fierce dragon logo.");

            // Keep 15 Credits as the base value. The game's fridge applies its own
            // chilled / ice-cold ItemFeatures and market-stage value modifiers.
            item.unitValue = BaseDrinkPrice;

            // BREVAGE is the game's beverage type used by its native chilling logic.
            // FOOD also lets ordinary food-buying customers recognize this as a drink.
            item.SetGameItemType("BREVAGE");
            item.SetGameItemType("FOOD");
            item.SetSpriteAndShapeFromMod(ModId, "red_dragon_energy");
            ModHelper.SetSource(item, ModId, "Anime Universe — Rei");
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
                    log.Error("Textures/rei.png was not loaded. Rei will not be queued without her portrait.");
                    return;
                }

                Texture2D texture = loadedMod.Textures[PortraitTextureKey];
                if (texture == null)
                {
                    log.Error("Rei portrait texture is null.");
                    return;
                }

                reiPortrait = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                UnityEngine.Object.DontDestroyOnLoad(reiPortrait);
                RegisterPortrait();
                log.Log("Rei portrait loaded and registered.");
            }
            catch (Exception ex)
            {
                log.Error("Could not register Rei portrait: " + ex);
            }
        }

        private void OnGameLoaded()
        {
            RegisterPortrait();
        }

        private bool RegisterPortrait()
        {
            if (reiPortrait == null || SpriteDict.Instance == null || SpriteDict.Instance.spriteDictionary == null)
                return false;

            try
            {
                if (!portraitInjected)
                {
                    hadPreviousPortrait = SpriteDict.Instance.spriteDictionary.ContainsKey(PortraitKey);
                    if (hadPreviousPortrait)
                        previousPortrait = SpriteDict.Instance.spriteDictionary[PortraitKey];
                }

                SpriteDict.Instance.spriteDictionary[PortraitKey] = reiPortrait;
                portraitInjected = true;
                return true;
            }
            catch (Exception ex)
            {
                log.Warning("Rei portrait registration will be retried: " + ex.Message);
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
                log?.Warning("Could not restore Rei portrait registration: " + ex.Message);
            }

            portraitInjected = false;
            previousPortrait = null;
            hadPreviousPortrait = false;
            if (reiPortrait != null)
            {
                UnityEngine.Object.Destroy(reiPortrait);
                reiPortrait = null;
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

            // One 25% roll per in-game day. A successful visit on day N does not
            // prevent a fresh roll on day N+1, and the hook cannot reroll the same day.
            if (lastRollDay == day || lastVisitDay == day)
                return;

            ModHelper.SetModDataInt(ModId, "last_roll_day", day);
            if (RNG.GetRandomInt(1, 100) > DailyAppearanceChance)
                return;

            try
            {
                StoreClient rei = CreateRei(day);
                manager.AddClient(rei);
                ModHelper.SetModDataInt(ModId, "last_visit_day", day);
                log.Log("Rei was queued for day " + day + " (25% daily roll succeeded).");
            }
            catch (Exception ex)
            {
                log.Error("Could not create Rei: " + ex);
            }
        }

        private StoreClient CreateRei(int day)
        {
            StoreClient rei = new StoreClient();
            rei.identifier = ClientId;
            rei.displayName = "Rei";

            StoreClientFactionSetup.InitUpperLevelCitizen(rei);
            rei.spriteName = PortraitKey;
            rei.clientFaction = StoreClient.FACTION_UPPER;
            rei.clientIntent = StoreClient.ClientIntent.SELLNBUY;

            int purchaseBudget = day >= LateBudgetStartsDay
                ? LatePurchaseBudget
                : EarlyPurchaseBudget;
            rei.SetBudget(purchaseBudget);

            rei.clientBuyingIdList.Clear();
            rei.clientBuyingTagList.Clear();
            rei.clientBuyingTagList.Add("LUXURY_ITEM");

            rei.mainDialogue = new Dialogue();
            rei.mainDialogue
                .SetText("Rei", Text("rei.greeting", "Hi. I'm Rei. I brought a drink called Red Dragon Energy Drink today."))
                .NextDialogue()
                .SetText("Rei", Text("rei.offer", "One can has a base value of 15 Credits. Rei sells at an 8% wholesale discount; chilling it in a refrigerator raises its market value."))
                .NextDialogue()
                .SetText("Rei", Text("rei.buy", "I also buy luxury goods. My purchase budget is 250 Credits before day 30, then 450 Credits."))
                .SetEndAction(PlaceReiStock);

            rei.AddBasicDialog();
            rei.CompleteClientCreation();
            return rei;
        }

        private void PlaceReiStock()
        {
            PlayerStore store = PlayerStore.Instance;
            if (store == null)
            {
                log.Warning("PlayerStore was unavailable when Rei's stock was placed.");
                return;
            }

            try
            {
                GameItem drink = DirectoryMaster.Item(DrinkItemId, true);
                if (drink == null)
                {
                    log.Warning("Red Dragon Energy Drink could not be created for Rei's stock.");
                    return;
                }

                // Keep the registered item at its 15-Credit base value. The counter copy
                // receives Rei's 8% wholesale discount; the fridge feature remains native.
                long wholesalePrice = (long)Math.Round(
                    BaseDrinkPrice * (1.0d - WholesaleDiscount),
                    MidpointRounding.AwayFromZero);
                drink.unitValue = wholesalePrice;
                store.AddDirectSellingItemToTable(drink, false, false, false, 0);
                log.Log("Placed Red Dragon Energy Drink on Rei's table at the 8% wholesale price of " + wholesalePrice + " Credits.");
            }
            catch (Exception ex)
            {
                log.Warning("Could not place Red Dragon Energy Drink on Rei's table: " + ex.Message);
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
