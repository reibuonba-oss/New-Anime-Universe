# Vũ Trụ Anime — Trạm Sạc Pin Nâng Cao

Mod độc lập dành cho Probably Stolen.

## Tốc độ sạc

Máy Sạc Pin tiêu chuẩn nạp 3 kJ cho mỗi viên pin được lắp vào sau mỗi chu kỳ qua đêm. Khi mở khóa Điện Thương Mại, tốc độ tiêu chuẩn là 6 kJ.

Trạm Sạc Pin Nâng Cao tăng gấp đôi cả hai mức:

- **6 kJ cho mỗi viên pin mỗi đêm** khi chưa mở khóa Điện Thương Mại.
- **12 kJ cho mỗi viên pin mỗi đêm** khi đã mở khóa Điện Thương Mại.

Máy sử dụng phương thức gốc `PowerHelper.ChargeBattery`, vì vậy các giới hạn dung lượng tối đa và điều kiện mất điện của game vẫn được áp dụng. Máy cũng tuân theo quy tắc gốc: điện lưới phải đang bật.

## Giao diện và cài đặt

- Sử dụng lại bảng pin và lưới kho đồ của máy Sạc Pin tiêu chuẩn.
- Chấp nhận vật phẩm có thẻ gốc `power_source_item`.
- **Không** tặng máy miễn phí khi bắt đầu game. Hãy mua từ Kaito khi anh ấy ghé cửa hàng; cần bật cả mod NPC Kaito.
- Có mục tạo máy thử nghiệm bằng phím F9.
- Biên dịch dựa trên bộ thư viện tham chiếu trong `Managed.zip` của game.

Giải nén `AdvancedCharger.zip` vào thư mục `Mods` của game, bật **Vũ Trụ Anime — Trạm Sạc Pin Nâng Cao** và **Kaito — Nhà Sưu Tầm**, sau đó khởi động lại game. Hãy mua máy từ Kaito; người chơi không được cấp máy miễn phí.

CI xác nhận DLL biên dịch được và ZIP có đủ tệp. Vẫn cần kiểm tra chu kỳ sạc trong một bản lưu thực tế với đúng phiên bản game đang cài.
