using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewAnimeUniverse.KurumiNPC
{
    /// <summary>
    /// Kurumi is an upper-level arms dealer. She buys weapons and gun mods,
    /// and places advanced energy cells and modules on the player's sale table.
    /// </summary>
    public sealed class KurumiNPCMod : IMod
    {
        private const string ModId = "new_anime_universe.kurumi_arms_dealer";
        private const string ClientId = "new_anime_universe_kurumi";
        private const string PortraitKey = "new_anime_universe_kurumi";
        private const string PortraitTextureKey = "kurumi";

        // Explicit vanilla weapon IDs supplement the WEAPON buying tag.
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

        // Advanced, rechargeable energy cells only; the basic cell is intentionally excluded.
        private static readonly string[] AdvancedEnergyCellIds =
        {
            "energy_credit_ext",
            "energy_credit_breeder"
        };

        // Enabled vanilla modules. Ruined, corrupted and contraband-only variants are excluded.
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

        private ModLog log;
        private Sprite kurumiPortrait;
        private Sprite previousPortrait;
        private bool hadPreviousPortrait;
        private bool portraitInjected;

        public void Init(ModManifest manifest)
        {
            log = new ModLog(manifest);
            log.Log("Kurumi arms dealer initialized.");
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
                    log.Error("Textures/kurumi.png was not loaded. Kurumi will not be queued without her portrait.");
                    return;
                }

                Texture2D texture = loadedMod.Textures[PortraitTextureKey];
                if (texture == null)
                {
                    log.Error("Kurumi portrait texture is null.");
                    return;
                }

                kurumiPortrait = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                UnityEngine.Object.DontDestroyOnLoad(kurumiPortrait);
                RegisterPortrait();
                log.Log("Kurumi portrait loaded and registered.");
            }
            catch (Exception ex)
            {
                log.Error("Could not register Kurumi portrait: " + ex);
            }
        }

        private void OnGameLoaded()
        {
            RegisterPortrait();
        }

        private bool RegisterPortrait()
        {
            if (kurumiPortrait == null || SpriteDict.Instance == null || SpriteDict.Instance.spriteDictionary == null)
                return false;

            try
            {
                if (!portraitInjected)
                {
                    hadPreviousPortrait = SpriteDict.Instance.spriteDictionary.ContainsKey(PortraitKey);
                    if (hadPreviousPortrait)
                        previousPortrait = SpriteDict.Instance.spriteDictionary[PortraitKey];
                }

                SpriteDict.Instance.spriteDictionary[PortraitKey] = kurumiPortrait;
                portraitInjected = true;
                return true;
            }
            catch (Exception ex)
            {
                log.Warning("Kurumi portrait registration will be retried: " + ex.Message);
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
                    log.Warning("Could not restore Kurumi portrait dictionary entry: " + ex.Message);
            }

            portraitInjected = false;
            previousPortrait = null;
            hadPreviousPortrait = false;
            if (kurumiPortrait != null)
            {
                UnityEngine.Object.Destroy(kurumiPortrait);
                kurumiPortrait = null;
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

            // One independent 10% roll per in-game day; Kurumi can visit at most once per day.
            if (lastRollDay == day || lastVisitDay == day)
                return;

            ModHelper.SetModDataInt(ModId, "last_roll_day", day);
            if (RNG.GetRandomInt(1, 100) > 10)
                return;

            try
            {
                StoreClient kurumi = CreateKurumi(day);
                manager.AddClient(kurumi);
                ModHelper.SetModDataInt(ModId, "last_visit_day", day);
                log.Log("Kurumi was queued for day " + day + " (10% daily roll succeeded).");
            }
            catch (Exception ex)
            {
                log.Error("Could not create Kurumi: " + ex);
            }
        }

        private StoreClient CreateKurumi(int day)
        {
            StoreClient kurumi = new StoreClient();
            kurumi.identifier = ClientId;
            kurumi.displayName = "Kurumi";

            StoreClientFactionSetup.InitUpperLevelCitizen(kurumi);
            kurumi.spriteName = PortraitKey;
            kurumi.clientFaction = StoreClient.FACTION_UPPER;
            kurumi.clientIntent = StoreClient.ClientIntent.SELLNBUY;
            kurumi.SetBudget(6000 + Math.Max(0, day) * 50);

            kurumi.clientBuyingIdList.Clear();
            for (int i = 0; i < WeaponItemIds.Length; i++)
                kurumi.clientBuyingIdList.Add(WeaponItemIds[i]);

            kurumi.clientBuyingTagList.Clear();
            kurumi.clientBuyingTagList.Add("WEAPON");
            kurumi.clientBuyingTagList.Add("GUN_MOD");

            kurumi.mainDialogue = new Dialogue();
            kurumi.mainDialogue
                .SetText("Kurumi", Text("kurumi.greeting", "Chào buổi tối. Tôi là Kurumi. Tôi kinh doanh vũ khí, pin năng lượng cao cấp và các module chuyên dụng."))
                .NextDialogue()
                .SetText("Kurumi", Text("kurumi.buy", "Tôi đang thu mua súng, vũ khí cận chiến và phụ kiện súng. Có món nào đáng giữ thì cứ đưa ra, tôi sẽ trả giá hợp lý."))
                .NextDialogue()
                .SetText("Kurumi", Text("kurumi.stock", "Hãy xem hàng của tôi. Pin tự sạc và những module tinh chỉnh này không dễ tìm đâu."))
                .SetEndAction(PlaceKurumiStock);

            kurumi.AddBasicDialog();
            kurumi.CompleteClientCreation();
            return kurumi;
        }

        private void PlaceKurumiStock()
        {
            PlayerStore store = PlayerStore.Instance;
            if (store == null)
            {
                log.Warning("PlayerStore was unavailable when Kurumi's stock was placed.");
                return;
            }

            int placed = 0;

            // Offer one Energy Cell Mk. II and one rare self-charging cell.
            for (int i = 0; i < AdvancedEnergyCellIds.Length; i++)
            {
                GameItem cell = DirectoryMaster.Item(AdvancedEnergyCellIds[i], true);
                if (cell == null)
                {
                    log.Warning("Advanced energy cell could not be created: " + AdvancedEnergyCellIds[i]);
                    continue;
                }

                store.AddDirectSellingItemToTable(cell, false, false, false, 0);
                placed++;
            }

            // Four specialist modules are selected randomly for each visit.
            for (int i = 0; i < 4; i++)
            {
                string moduleId = RNG.GetRandomElement(ModuleItemIds);
                GameItem module = DirectoryMaster.Item(moduleId, true);
                if (module == null)
                {
                    log.Warning("Module could not be created: " + moduleId);
                    continue;
                }

                store.AddDirectSellingItemToTable(module, false, false, false, 0);
                placed++;
            }

            log.Log("Kurumi's stock was placed: " + placed + " item(s).");
        }

        private static string Text(string key, string fallback)
        {
            return fallback;
        }
    }
}