# Vũ Trụ Anime — Máy Tái Chế Rác

Máy móc độc lập cho Probably Stolen, sử dụng giao diện theo phong cách máy móc gốc của game.

## Công thức

- Lưới đầu vào: **12×4**, đủ chỗ cho tối đa tám vật phẩm Rác tiêu chuẩn kích thước 3×2 cùng lúc.
- Chu kỳ cơ bản: **4 Rác** (`junk`) → **1 Sắt Vụn** (`scrap_metal`), hoàn tất qua đêm.
- Điện tiêu thụ cơ bản: **5 năng lượng mỗi chu kỳ**.
- Khi đạt **từ 50% Hiệu năng trở lên** và có ít nhất 8 Rác: máy xử lý **8 Rác → 2 Sắt Vụn** trong cùng chu kỳ.
- Nếu Hiệu năng dưới 50%, máy chỉ xử lý mẻ cơ bản.
- Module Hiệu Suất sử dụng phép tính điện tiêu thụ gốc của máy móc trong game và giúp giảm điện.
- Module Chất Lượng không thay đổi công thức, sản phẩm hoặc cách tiêu thụ điện của máy.

Ô đầu vào chỉ nhận vật phẩm gốc có ID `junk`; máy sẽ không tiêu thụ những vật phẩm khác chỉ mang loại JUNK, ví dụ vũ khí tự chế.

## Giao diện và tương thích

- Sử dụng cửa sổ Máy Móc, ô pin, kho module đa năng và sự kiện xử lý qua đêm gốc của game.
- Dùng hình ảnh lò nung gốc và ID vật phẩm riêng.
- Không vá phương thức game bằng Harmony và không khai báo quyền truy cập bổ sung.
- Cấp một máy khởi đầu vào kho đồ người chơi trong lần tải game đầu tiên khi mod được bật. Nếu kho đồ đầy, mod sẽ thử cấp lại ở lần tải tiếp theo.
- Có nút tạo máy thử nghiệm bằng phím F9.

## Biên dịch

Giải nén `Managed.zip` của repository vào `references/Managed/`, sau đó chạy:

```powershell
dotnet build RecyclerMachine/RecyclerMachine.csproj -c Release -p:ManagedDir="C:\Path\To\references\Managed"
```

## Cài đặt

Giải nén ZIP được tạo ra vào thư mục `Mods/` của game. Bật **Vũ Trụ Anime — Máy Tái Chế Rác** trong menu Mods và khởi động lại game.

Việc DLL biên dịch và ZIP được đóng gói thành công không thay thế cho kiểm thử trong bản lưu thực tế. Hãy xác nhận chu kỳ qua đêm, mức tiêu thụ năng lượng, dung lượng đầu ra và ngưỡng module trên đúng phiên bản game đang dùng.
