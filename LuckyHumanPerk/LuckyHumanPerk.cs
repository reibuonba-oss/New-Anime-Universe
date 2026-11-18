using System;

namespace NewAnimeUniverse.LuckyHumanPerk
{
    public sealed class LuckyHumanPerkMod : IMod
    {
        private const string ModId = "new_anime_universe.lucky_human";
        private const string PerkId = "new_anime_universe.lucky_human.perk";
        private const int StartingLuckBonus = 15;
        private const int ExtraPerkSlots = 4;
        private const int PerkPointCost = 2;

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
                cost: PerkPointCost,
                type: StartingPerk.StartingPerkType.POSITIVE,
                name: () => ModHelper.GetLocalized(ModId, "perk_lucky_human_name"),
                description: () => ModHelper.GetLocalized(ModId, "perk_lucky_human_desc"),
                iconKey: ModId + ":lucky_human_icon",
                extraPerkSlots: ExtraPerkSlots);

            ModHook.OnNewGameStarted += ApplyStartingLuck;

            log.Log("Registered Con Người May Mắn: +4 starting perk slots, +15 starting Luck for 2 perk points, with its custom clover icon.");
        }

        public void OnDisable()
        {
            ModHook.OnNewGameStarted -= ApplyStartingLuck;
            ModHelper.RemoveStartingPerks(ModId);

            log?.Log("Unregistered Con Người May Mắn.");
        }

        private void ApplyStartingLuck(string scenarioId)
        {
            if (!ModHelper.IsStartingPerkActive(PerkId))
                return;

            PlayerStore store = PlayerStore.Instance;
            if (store == null || store.healthData == null)
            {
                log?.Warning("The starting perk was selected, but HealthData was unavailable when OnNewGameStarted fired. Starting Luck was not changed.");
                return;
            }

            // Luck is stored on HealthData and saved with the run. Apply only at new-run
            // creation; applying again on a load would incorrectly stack the bonus.
            store.healthData.luck += StartingLuckBonus;

            log?.Log("Applied +15 starting Luck for Con Người May Mắn. HealthData.luck is now " + store.healthData.luck + ".");
        }
    }
}
