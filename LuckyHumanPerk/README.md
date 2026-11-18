# Con Người May Mắn — Thiên phú khởi đầu

Mod độc lập bổ sung thiên phú khởi đầu cho Probably Stolen.

## Hiệu ứng

- **Tên:** Con Người May Mắn
- **Chi phí:** 2 điểm perk
- **Ô perk:** mở thêm 4 ô perk khởi đầu khi chọn thiên phú này
- **May Mắn khởi đầu:** +15 điểm khi bắt đầu lượt chơi mới
- **Biểu tượng:** hình cỏ bốn lá màu trắng, kích thước 32×32, tại `Textures/Items/lucky_human_icon.png`; game tự tô màu xanh vì đây là thiên phú tích cực

Thiên phú dùng API gốc `ModHelper.AddStartingPerk` với `iconKey: "new_anime_universe.lucky_human:lucky_human_icon"`. Điểm May Mắn được cộng vào `PlayerStore.Instance.healthData.luck` trong sự kiện `ModHook.OnNewGameStarted`, vì vậy giá trị được lưu cùng lượt chơi. Khi tải bản lưu, mod không cộng lại điểm May Mắn.

Biểu tượng perk phải nằm trong `Textures/Items/`. Quy trình build kiểm tra ảnh được đóng gói vào ZIP cài đặt.

## Biên dịch

Giải nén `Managed.zip` vào `references/Managed/`, sau đó chạy:

    dotnet build LuckyHumanPerk/LuckyHumanPerk.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"

GitHub Actions sẽ tạo ZIP cài đặt.

## Kiểm tra

CI xác nhận biên dịch và đóng gói ZIP. Vẫn cần kiểm tra trong game để xác nhận biểu tượng cỏ bốn lá hiển thị đúng, thiên phú mở thêm 4 ô và lượt chơi mới bắt đầu với +15 điểm May Mắn.
