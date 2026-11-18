# Kaito — Nhà Sưu Tầm

Mod NPC độc lập cho Probably Stolen, sử dụng các hệ thống và vật phẩm gốc của game.

## Hoạt động

- **Tỷ lệ xuất hiện:** mỗi ngày trong game có một lần tung xác suất 8%. Kaito xuất hiện tối đa một lần mỗi ngày.
- **Thu mua:** một danh sách vũ khí gốc được chọn lọc, cùng vật phẩm mang thẻ MODULE, ENERGY_CELL và LUXURY_ITEM.
- **Ngân sách:** 4.800 Credits cộng thêm 25 Credits cho mỗi ngày trong game.
- **Bày bán:** Bộ Lọc Nước Cao Cấp, Máy Lọc Nước HydraTech, Pin Năng Lượng Mk. II, Pin Tự Sạc và hai module gốc khác nhau trong mỗi lần ghé thăm.
- **Hội thoại:** lời thoại riêng của Kaito bằng tiếng Việt.
- **Chân dung:** sử dụng ảnh PNG nền trong suốt kích thước 242 × 300.

## Lựa chọn triển khai an toàn

- Không tạo định nghĩa vật phẩm tùy chỉnh và không can thiệp vào thư mục vật phẩm.
- Không dùng Harmony, reflection, truy cập hệ thống tệp hoặc quyền bổ sung.
- Sử dụng vòng đời IMod gốc, sự kiện tạo khách và ID vật phẩm gốc.
- Nếu vật phẩm trong danh mục không có sẵn, mod bỏ qua vật phẩm đó và ghi cảnh báo thay vì làm hỏng quá trình tạo khách.
- Trạng thái xác suất xuất hiện hằng ngày được lưu bằng ModData riêng của lượt chơi.

## Biên dịch

Giải nén Managed.zip của repository vào references/Managed/, sau đó chạy:

    dotnet build KaitoNPC/KaitoNPC.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"

Workflow .github/workflows/build-kaitonpc.yml sẽ tạo ZIP cài đặt.

## Cài đặt

Giải nén ZIP vào thư mục Mods/ của game, bật Kaito trong menu Mods và khởi động lại game.

Đây là mod đã được biên dịch, nhưng vẫn cần kiểm tra trong game để xác nhận khách xuất hiện, giao dịch được chấp nhận và hàng hóa được bày đúng trên phiên bản game đang dùng.
