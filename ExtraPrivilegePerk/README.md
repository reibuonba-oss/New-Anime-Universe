# Thêm Tí Đặc Quyền — Thiên phú khởi đầu

Mod độc lập bổ sung thiên phú khởi đầu cho Probably Stolen.

## Hiệu ứng

- **Tên:** Thêm Tí Đặc Quyền
- **Điểm perk:** hoàn lại **1 điểm perk** để chọn thiên phú khi bắt đầu lượt chơi mới
- **Ô perk:** mở thêm 4 ô perk khởi đầu khi chọn thiên phú này
- **Credits khởi đầu:** trừ 100 Credits đúng một lần khi bắt đầu lượt chơi mới
- **Biểu tượng:** hình khiên và dấu cộng màu trắng, kích thước 32×32, nằm trong `Textures/Items/extra_privilege_icon.png`; game tự tô màu đỏ vì đây là thiên phú tiêu cực

Thiên phú được đăng ký bằng API gốc `ModHelper.AddStartingPerk` với `cost: -1`, `type: StartingPerk.StartingPerkType.NEGATIVE`, `iconKey: "new_anime_universe.extra_privilege:extra_privilege_icon"` và `extraPerkSlots: 4`. Chi phí âm hoàn lại một điểm trong ngân sách chọn perk khởi đầu.

Khoản trừ 100 Credits dùng `PlayerStore.ModCash(-100, true)` trong sự kiện `ModHook.OnNewGameStarted`. Sự kiện này chạy khi tạo lượt chơi mới, không chạy khi tải bản lưu, nên không trừ lại khi tiếp tục save cũ.

Biểu tượng perk phải được đặt trong `Textures/Items/`. Quy trình build kiểm tra ảnh này có trong ZIP cài đặt.

## Biên dịch

Giải nén `Managed.zip` của repository vào `references/Managed/`, sau đó chạy:

    dotnet build ExtraPrivilegePerk/ExtraPrivilegePerk.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"

GitHub Actions sẽ biên dịch DLL và tạo ZIP cài đặt.

## Kiểm tra

CI xác nhận biên dịch và cấu trúc ZIP. Vẫn cần kiểm tra trong game để xác nhận biểu tượng hiển thị đúng kích thước, hoàn lại 1 điểm perk, mở thêm 4 ô và chỉ trừ 100 Credits khi tạo lượt chơi mới.
