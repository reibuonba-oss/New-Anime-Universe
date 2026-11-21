# Rei — Thương nhân Nước Tăng Lực Rồng Đỏ

Mod NPC độc lập cho Probably Stolen, thuộc dự án New Anime Universe.

## Hoạt động của Rei

- **Tỷ lệ xuất hiện:** một lần tung xác suất độc lập **25% mỗi ngày trong game**, tại `OnGenerateCustomerVeryLate`.
- **Chu kỳ xuất hiện:** mỗi ngày chỉ tung một lần. Nếu Rei xuất hiện vào ngày N, cô ấy có thể được tung xác suất xuất hiện lại từ ngày N+1.
- **Bán:** một lon **Nước Tăng Lực Rồng Đỏ** trong mỗi lần ghé thăm.
- **Giá cơ bản:** **15 Credits** mỗi lon.
- **Giá bán sỉ:** giảm **8%** trên giá cơ bản; lon 15 Credits được làm tròn thành **14 Credits** tại quầy.
- **Ướp lạnh:** vật phẩm được đăng ký với loại đồ uống gốc `BREVAGE` và `FOOD`, để hệ thống tủ lạnh của game nhận diện. Tủ lạnh tự thêm các ItemFeature lạnh và áp dụng điều chỉnh giá trị thị trường gốc. Mod không tự cộng thêm một khoản lạnh riêng, tránh cộng trùng với công thức của game.
- **Thu mua:** chỉ các mặt hàng mang loại `LUXURY_ITEM`.
- **Ngân sách thu mua:** **250 Credits** trước ngày 30; tăng lên **450 Credits** từ ngày 30 trở đi.
- **Giao dịch:** `StoreClient.ClientIntent.SELLNBUY`, cho phép mua và bán trong cùng một lần ghé thăm.
- **Chân dung:** dùng `Textures/rei.png`; lon nước dùng `Textures/Items/red_dragon_energy.png`.

## Giá bán và làm lạnh

`unitValue` gốc của vật phẩm được đặt là 15 Credits. Khi Rei bày lon lên quầy, bản sao hàng hóa nhận mức chiết khấu bán sỉ 8% (giá quầy làm tròn thành 14 Credits). Khi người chơi đưa lon vào tủ lạnh có điện, game tự quản lý trạng thái `chilled` hoặc `iceCold` và các ItemFeature tương ứng. Mod không cộng thêm khoản thưởng lạnh thủ công.

Điều này giữ cho cơ chế làm lạnh tương thích với hệ thống tủ lạnh gốc và tránh áp dụng hai lần cùng một phần trăm tăng giá.

## Biên dịch

Giải nén `Managed.zip` ở thư mục gốc repository vào `references/Managed/`, sau đó chạy:

```powershell
dotnet build ReiNPC/ReiNPC.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"
```

Workflow `.github/workflows/build-rei-npc.yml` sẽ biên dịch DLL và tạo artifact ZIP cài đặt.

## Cài đặt

Giải nén `ReiNPC.zip` vào thư mục `Mods/` của game, bật **Rei — Red Dragon Merchant** trong menu Mods và khởi động lại game.

CI kiểm tra biên dịch và cấu trúc ZIP. Vẫn cần kiểm tra trong game để xác nhận Rei xuất hiện đúng xác suất, giao dịch hàng xa xỉ, giá 15 Credits và lon nước nhận được ItemFeature lạnh từ tủ lạnh.
