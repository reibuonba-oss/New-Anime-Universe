using System;

namespace NewAnimeUniverse.ExtraPrivilegePerk
{
    public sealed class ExtraPrivilegePerkMod : IMod
    {
        private const string ModId = "new_anime_universe.extra_privilege";
        private const string PerkId = "new_anime_universe.extra_privilege.perk";
        private const int StartingCreditPenalty = 100;
        private const int StartingPerkPointBonus = 1;
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
                cost: -StartingPerkPointBonus,
                type: StartingPerk.StartingPerkType.NEGATIVE,
                name: () => "Thêm Tí Đặc Quyền",
                description: () => "Cộng thêm 1 điểm perk để chọn thiên phú khởi đầu và mở thêm 4 ô perk. Khi bắt đầu lượt chơi mới, trừ 100 Credits đúng một lần; tải bản lưu không bị trừ lại.",
                iconKey: ModId + ":extra_privilege_icon",
                extraPerkSlots: ExtraPerkSlots);

            ModHook.OnNewGameStarted += ApplyStartingCreditPenalty;
            log.Log("Đã đăng ký Thêm Tí Đặc Quyền: +1 điểm perk, +4 ô perk khởi đầu, -100 Credits khi bắt đầu lượt chơi mới.");
        }

        public void OnDisable()
        {
            ModHook.OnNewGameStarted -= ApplyStartingCreditPenalty;
            ModHelper.RemoveStartingPerks(ModId);
            log?.Log("Đã gỡ đăng ký Thêm Tí Đặc Quyền.");
        }

        private void ApplyStartingCreditPenalty(string scenarioId)
        {
            if (!ModHelper.IsStartingPerkActive(PerkId))
                return;

            PlayerStore store = PlayerStore.Instance;
            if (store == null)
            {
                log?.Warning("Đã chọn thiên phú nhưng PlayerStore chưa sẵn sàng. Không thể trừ 100 Credits khởi đầu.");
                return;
            }

            // Sự kiện này chỉ chạy khi tạo lượt chơi mới, không chạy khi tải bản lưu.
            store.ModCash(-StartingCreditPenalty, true);
            log?.Log("Đã trừ 100 Credits khởi đầu cho thiên phú Thêm Tí Đặc Quyền.");
        }
    }
}
