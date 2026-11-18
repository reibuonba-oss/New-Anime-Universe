using System;

namespace NewAnimeUniverse.ExtraPrivilegePerk
{
    public sealed class ExtraPrivilegePerkMod : IMod
    {
        private const string ModId = "new_anime_universe.extra_privilege";
        private const string PerkId = "new_anime_universe.extra_privilege.perk";
        private const int StartingCreditPenalty = 100;
        private const int ExtraPerkSlots = 4;

        private ModLog log;

        public void Init(ModManifest manifest)
        {
            log = new ModLog(manifest);
        }

        public void OnEnable()
        {
            ModHelper.AddStartingPerk(
                ModId,
                PerkId,
                cost: 0,
                type: StartingPerk.StartingPerkType.NEUTRAL,
                name: () => ModHelper.GetLocalized(ModId, "perk_extra_privilege_name"),
                description: () => ModHelper.GetLocalized(ModId, "perk_extra_privilege_desc"),
                iconKey: ModId + ":extra_privilege_icon",
                extraPerkSlots: ExtraPerkSlots);

            ModHook.OnNewGameStarted += ApplyStartingCreditPenalty;
            log.Log("Registered Thêm Tí Đặc Quyền: +4 starting perk slots, -100 Credits on new-run start.");
        }

        public void OnDisable()
        {
            ModHook.OnNewGameStarted -= ApplyStartingCreditPenalty;
            ModHelper.RemoveStartingPerks(ModId);
            log?.Log("Unregistered Thêm Tí Đặc Quyền.");
        }

        private void ApplyStartingCreditPenalty(string scenarioId)
        {
            if (!ModHelper.IsStartingPerkActive(PerkId))
                return;

            PlayerStore store = PlayerStore.Instance;
            if (store == null)
            {
                log?.Warning("The perk was selected, but PlayerStore was unavailable. The 100-Credit penalty was not applied.");
                return;
            }

            // OnNewGameStarted fires after the new run's starting scenario and perks are
            // placed, and is not fired when loading a save. This deducts once per new run.
            store.ModCash(-StartingCreditPenalty, true);
            log?.Log("Deducted 100 starting Credits for Thêm Tí Đặc Quyền.");
        }
    }
}
