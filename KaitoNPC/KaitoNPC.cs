using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewAnimeUniverse.KaitoNPC
{
    /// <summary>
    /// Kaito is a cautious collector who buys selected weapons, modules,
    /// energy cells and luxury goods, and sells practical technology supplies.
    /// </summary>
    public sealed class KaitoNPCMod : IMod
    {
        private const string ModId = "new_anime_universe.kaito_collector";
        private const string ClientId = "new_anime_universe_kaito";
        private const string PortraitKey = "new_anime_universe_kaito";
        private const string PortraitTextureKey = "kaito";
        private const int DailyAppearanceChance = 8;

        // Curated vanilla weapon IDs. Avoids adding custom weapon code.
        private static readonly string[] WeaponItemIds =
        {
            "printed_gun",
            "handmade_pistol",
            "heavy_handmade_pistol",
            "glock_receiver",
            "smg",
            "shotgun",
            "revolver",
            "stun_gun",
            "tazer",
            "kitchen_knife",
            "box_cutter",
            "combat_knife",
            "kitchen_cleaver",
            "combat_machete",
            "pipe_weapon",
            "stun_baton",
            "crowbar",
            "hatchet",
            "glass_shard_shiv"
        };

        // Vanilla module catalogue entries known to exist in the game's item catalogue.
        private static readonly string[] ModuleItemIds =
        {
            "system_module_performance",
            "system_module_efficiancy",
            "system_module_quality",
            "system_module_overclock",
            "system_module_eco",
            "system_module_fineness",
            "system_module_streamlining",
            "farm_module_nutrifruit",
            "farm_module_dream_cap",
            "farm_module_kotton_loom",
            "farm_module_hydroreed_filter",
            "evaporator_module_solvent_recovery",
            "printer_module_metal",
            "alarm_module_transmitter",
            "furnace_module_blast",
            "furnace_module_junk",
            "chem_module"
        };

        // Fixed vanilla utility stock; no custom item definitions are registered.
        private static readonly string[] FixedStockIds =
        {
            "water_filter_adv",
            "portable_water_purifier",
            "energy_credit_ext",
            "energy_credit_breeder"
        };

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
            for (int i = 0; i < WeaponItemIds.Length; i++)
                kaito.clientBuyingIdList.Add(WeaponItemIds[i]);

            kaito.clientBuyingTagList.Clear();
            kaito.clientBuyingTagList.Add("MODULE");
            kaito.clientBuyingTagList.Add("ENERGY_CELL");
            kaito.clientBuyingTagList.Add("LUXURY_ITEM");

            kaito.mainDialogue = new Dialogue();
            kaito.mainDialogue
                .SetText("Kaito", Text("kaito.greeting", "Hello. I'm Kaito. I collect useful technology and well-made rare goods."))
                .NextDialogue()
                .SetText("Kaito", Text("kaito.buy", "I'm buying selected weapons, modules, energy cells and luxury goods. I only carry what I can properly inspect."))
                .NextDialogue()
                .SetText("Kaito", Text("kaito.stock", "Today I brought advanced water filters, energy cells and a small selection of modules. Practical things, chosen carefully."))
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

            for (int i = 0; i < FixedStockIds.Length; i++)
                placed += PlaceVanillaItem(store, FixedStockIds[i]);

            // Two non-contraband modules, selected from enabled vanilla catalogue IDs.
            List<string> availableModules = new List<string>(ModuleItemIds);
            for (int i = 0; i < 2 && availableModules.Count > 0; i++)
            {
                string moduleId = RNG.GetRandomElement(availableModules.ToArray());
                availableModules.Remove(moduleId);
                placed += PlaceVanillaItem(store, moduleId);
            }

            log.Log("Kaito's stock was placed: " + placed + " item(s).");
        }

        private int PlaceVanillaItem(PlayerStore store, string itemId)
        {
            try
            {
                GameItem item = DirectoryMaster.Item(itemId, true);
                if (item == null)
                {
                    log.Warning("Vanilla stock item was unavailable: " + itemId);
                    return 0;
                }

                store.AddDirectSellingItemToTable(item, false, false, false, 0);
                return 1;
            }
            catch (Exception ex)
            {
                log.Warning("Could not place " + itemId + " on Kaito's table: " + ex.Message);
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