# NPC Hanabi — Vũ Trụ Anime

Mod thương nhân độc lập cho Probably Stolen.

## Hoạt động mua bán của Hanabi

- **Tỷ lệ xuất hiện:** mỗi ngày trong game có một lần tung xác suất 10% tại sự kiện OnGenerateCustomerVeryLate. Hanabi xuất hiện tối đa một lần mỗi ngày.
- **Thu mua:** vật phẩm mang thẻ power_source_item (pin năng lượng) và MODULE.
- **Bày bán:** một Mặt Nạ Hanabi và một Súng Hanabi trong mỗi lần ghé thăm.
- **Đăng ký vật phẩm riêng:** dùng sự kiện OnModItemDirectoryInit và ModItemDirectory.Add.
- **Phân loại:** Mặt Nạ Hanabi thuộc cả HOUSEHOLD_GOOD và LUXURY_ITEM; Súng Hanabi thuộc WEAPON.
- **Hội thoại:** các câu thoại riêng của Hanabi bằng tiếng Việt.
- **Chân dung:** sử dụng ảnh Textures/hanabi.png.

## Giá bán

Giá trị thị trường cơ bản là **888 Credits** cho Mặt Nạ Hanabi và **999 Credits** cho Súng Hanabi. Hanabi cộng thêm 5% vào giá niêm yết, làm tròn đến Credit gần nhất:

| Vật phẩm | Giá thị trường cơ bản | Giá Hanabi niêm yết |
|---|---:|---:|
| Mặt Nạ Hanabi | 888 | 932 |
| Súng Hanabi | 999 | 1.049 |

Phần tăng giá được áp dụng trực tiếp vào unitValue của bản sao hàng hóa được bày bán; code không phụ thuộc vào StoreClient.sellPriceModifier.

## Biên dịch

Repository có Managed.zip ở thư mục gốc. Giải nén vào references/Managed/, sau đó chạy:

    dotnet build HanabiNPC/HanabiNPC.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"

Workflow .github/workflows/build-hanabinpc.yml sẽ biên dịch DLL, chuyển ảnh sang PNG RGBA 8-bit tiêu chuẩn và xuất ZIP cài đặt có thư mục HanabiNPC/ ở thư mục gốc.

## Cài đặt

Giải nén ZIP trực tiếp vào thư mục Mods/ của game. Mod sẽ tạo Mods/HanabiNPC/manifest.xml, HanabiNPC.dll, Textures/ và Localization/. Bật Hanabi trong menu Mods và khởi động lại game.

## Tương thích

Mod đăng ký hai bộ tạo vật phẩm tùy chỉnh thông qua sự kiện thư mục vật phẩm chính thức. Chân dung sử dụng cùng kỹ thuật đăng ký sprite khách hàng như Rem và Kurumi; cần kiểm tra lại sau mỗi bản cập nhật game.

Súng Hanabi được đăng ký là vật phẩm tùy chỉnh có loại WEAPON. Bản triển khai này không bổ sung cơ chế bắn, đạn, trang bị hoặc chiến đấu.