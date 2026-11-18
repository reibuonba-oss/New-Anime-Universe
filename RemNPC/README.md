# NPC Rem — Vũ Trụ Anime

Mod cho Probably Stolen, bổ sung Rem làm thương nhân thuộc tầng lớp thượng lưu và xuất hiện khá hiếm.

## Hoạt động

- **Tỷ lệ xuất hiện:** mỗi ngày trong game có một lần tung xác suất độc lập **11%** tại sự kiện OnGenerateCustomerVeryLate. Rem xuất hiện tối đa một lần mỗi ngày.
- **Bày bán:** bốn vật phẩm gốc được chọn ngẫu nhiên từ danh mục LUXURY_ITEM của game trong mỗi lần ghé thăm.
- **Thu mua:** vật phẩm gốc thuộc loại FOOD và các ID vật phẩm nước được hỗ trợ, liệt kê trong RemNPC.cs.
- **Hội thoại:** lời chào riêng, lời mời xem hàng và câu thoại thu mua thực phẩm/nước bằng tiếng Việt.
- **Chân dung:** dùng ảnh Textures/rem.png, đăng ký bằng khóa sprite khách hàng riêng.

Khách sử dụng StoreClient.ClientIntent.SELLNBUY nên có thể mua và bán trong cùng một lần ghé thăm. Ngân sách thu mua bắt đầu từ 1.600 Credits và tăng thêm 20 Credits mỗi ngày trong game.

## Biên dịch

Repository có Managed.zip ở thư mục gốc. Giải nén vào references/Managed/, sau đó chạy:

    dotnet build RemNPC.csproj -c Release

Hoặc chỉ định thư mục thư viện tham chiếu khác:

    dotnet build RemNPC.csproj -c Release -p:ManagedDir="C:\Path\To\Probably Stolen_Data\Managed"

Workflow .github/workflows/build-remnpc.yml sẽ biên dịch DLL với Managed.zip của repository và xuất ZIP cài đặt RemNPC.

## Cài đặt

Giải nén thư mục RemNPC được build vào Mods/ của game, bật mod trong menu Mods và khởi động lại game.

## Ghi chú kỹ thuật

- Sử dụng vòng đời IMod và các sự kiện ModHook về khách hàng/tài nguyên.
- Thêm khách hàng bằng StoreClientManager.AddClient từ OnGenerateCustomerVeryLate.
- Lưu chốt tung xác suất mỗi ngày và giới hạn một lượt ghé thăm mỗi ngày bằng dữ liệu riêng của mod.
- Chân dung khách hàng được đăng ký qua SpriteDict.Instance.spriteDictionary. Cơ chế này nhạy cảm với thay đổi của game, nên cần kiểm tra sau các bản cập nhật.
- CI chỉ xác nhận biên dịch và đóng gói; cần kiểm thử trong game để xác nhận Rem xuất hiện và giao dịch hai chiều hoạt động.