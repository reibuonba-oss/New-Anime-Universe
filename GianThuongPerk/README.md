# Gian Thương — Perk khởi đầu

Mod độc lập cho Probably Stolen.

## Hiệu ứng

- **Tên:** Gian Thương
- **Loại / chi phí:** Perk Vàng (NEUTRAL), tốn 1 điểm perk
- **Giá bán:** tăng 5% khi khách mua món hàng thuộc về người chơi
- **Giá hiển thị:** giá ở bên phải từng món đã bao gồm phần tăng 5%
- **Phân tích giá:** thêm dòng **Gian Thương (+5%)** trong mục **Thị Trường và Nhà Buôn**
- **Tiền nhận:** dùng cùng hàm tính giá với danh sách thương lượng nên số tiền thanh toán khớp giá đang hiển thị
- **Tiền khởi đầu:** +50 Credits
- **Hảo cảm:** -5 với từng phe có trong danh sách quan hệ của lượt chơi mới

## Ngôn ngữ

Tên, mô tả và dòng phân tích giá được đặt trực tiếp bằng tiếng Việt. Một số bản Việt hóa chỉ thay chữ giao diện hiển thị mà không đổi locale nội bộ của game khỏi English; vì vậy chỉ cung cấp `vi.csv` có thể vẫn khiến mod loader chọn `en.csv`. Bản này không phụ thuộc vào việc tự nhận diện locale cho các dòng chữ chính của perk.

## Kỹ thuật

- Harmony postfix trên `GameItem.GetNegociatedValue()` cộng 5% vào giá giao dịch. Đây là hàm dùng cho giá từng món, tổng tiền, kiểm tra khả năng thanh toán và thanh toán.
- Harmony postfix trên `NegociationUIManager.AddFeatureSection` thêm một `ItemFeature` tạm thời ở `ValueStage.Market`, để phần phân tích giá hiện dòng **Gian Thương (+5%)**.
- Dòng phân tích chỉ là phần hiển thị, không tự cộng tiền lần hai.
- Chỉ áp dụng khi khách mua hàng của người chơi; mua hàng từ khách không được cộng.

## Build

Giải nén `Managed.zip` vào `references/Managed/`, sau đó chạy:

    dotnet build GianThuongPerk/GianThuongPerk.csproj -c Release -p:ManagedDir="C:\\Path\\To\\references\\Managed"

CI build và xác minh ZIP cài đặt. Vẫn cần kiểm thử trực tiếp trong game để xác nhận giao diện thực tế và số tiền thanh toán.
