# NPC Kurumi — Vũ Trụ Anime

Mod NPC độc lập cho Probably Stolen. Kurumi là nhà buôn vũ khí thuộc tầng lớp thượng lưu, chuyên thu mua vũ khí và bán pin năng lượng cao cấp cùng module.

## Hoạt động

- **Tỷ lệ xuất hiện:** mỗi ngày trong game có một lần tung xác suất độc lập **10%** tại sự kiện OnGenerateCustomerVeryLate; Kurumi xuất hiện tối đa một lần mỗi ngày.
- **Thu mua:** súng, vũ khí cận chiến và phụ kiện súng. Khách sử dụng ý định giao dịch SELLNBUY của game, cùng các thẻ WEAPON và GUN_MOD và danh sách ID vũ khí gốc dự phòng.
- **Bày bán:** một Pin Năng Lượng Mk. II, một Pin Tự Sạc và bốn module chuyên dụng được chọn ngẫu nhiên trong mỗi lần ghé thăm.
- **Hội thoại:** lời thoại riêng của Kurumi bằng tiếng Việt.
- **Chân dung:** dùng ảnh Textures/kurumi.png, đăng ký bằng khóa sprite riêng.

Ngân sách thu mua bắt đầu từ 6.000 Credits và tăng thêm 50 Credits cho mỗi ngày trong game. Đây là ngân sách chào mua của NPC, không phải mức giá mua được đảm bảo.

## Biên dịch

Repository có Managed.zip ở thư mục gốc. Giải nén vào references/Managed/, sau đó chạy:

    dotnet build KurumiNPC.csproj -c Release

Hoặc chỉ định một thư mục thư viện tham chiếu khác:

    dotnet build KurumiNPC.csproj -c Release -p:ManagedDir="C:\Path\To\Probably Stolen_Data\Managed"

Workflow .github/workflows/build-kuruminpc.yml sẽ biên dịch DLL và xuất ZIP cài đặt.

## Cài đặt

Giải nén KurumiNPC.zip để thư mục game có cấu trúc Mods/KurumiNPC/ gồm manifest.xml, KurumiNPC.dll, Textures/kurumi.png và Localization/. Bật Kurumi trong menu Mods và khởi động lại game.

## Ghi chú kỹ thuật

- Sử dụng vòng đời IMod và các sự kiện khách hàng/tài nguyên gốc.
- Thêm NPC bằng StoreClientManager.AddClient.
- Lưu trạng thái tung xác suất mỗi ngày bằng ModData riêng của lượt chơi.
- Đăng ký chân dung thông qua SpriteDict.Instance.spriteDictionary. Đây là bề mặt tương thích nhạy cảm với cập nhật game, nên cần kiểm tra lại sau mỗi bản cập nhật.
- CI xác nhận biên dịch với Managed.zip của repository; vẫn cần kiểm tra trong game việc NPC xuất hiện và giao dịch hai chiều.