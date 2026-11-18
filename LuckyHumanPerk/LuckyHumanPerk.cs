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
                name: () => "Con Người May Mắn",
                description: () => "Mở thêm 4 ô perk khởi đầu và cộng 15 điểm May Mắn khi bắt đầu lượt chơi mới. Tốn 2 điểm perk.",
                iconKey: ModId + ":lucky_human_icon",
                extraPerkSlots: ExtraPerkSlots);

            ModHook.OnNewGameStarted += ApplyStartingLuck;

            log.Log("Đã đăng ký Con Người May Mắn: +4 ô perk khởi đầu và +15 điểm May Mắn, tiêu tốn 2 điểm perk.");
        }

        public void OnDisable()
        {
            ModHook.OnNewGameStarted -= ApplyStartingLuck;
            ModHelper.RemoveStartingPerks(ModId);
            log?.Log("Đã gỡ đăng ký Con Người May Mắn.");
        }

        private void ApplyStartingLuck(string scenarioId)
        {
            if (!ModHelper.IsStartingPerkActive(PerkId))
                return;

            PlayerStore store = PlayerStore.Instance;
            if (store == null || store.healthData == null)
            {
                log?.Warning("Đã chọn thiên phú nhưng dữ liệu sức khỏe chưa sẵn sàng. Không thể cộng điểm May Mắn khởi đầu.");
                return;
            }

            // May Mắn được lưu trong dữ liệu của lượt chơi; chỉ cộng khi bắt đầu run mới.
            store.healthData.luck += StartingLuckBonus;
            log?.Log("Đã cộng 15 điểm May Mắn cho Con Người May Mắn. Tổng May Mắn hiện tại: " + store.healthData.luck + ".");
        }
    }
}
