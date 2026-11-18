# Gian Thương — Perk khởi đầu

Mod độc lập cho Probably Stolen.

## Hiệu ứng

- **Tên:** Gian Thương
- **Loại / chi phí:** Perk Vàng (NEUTRAL), tốn 1 điểm perk
- **Khi bán hàng:** khách mua hàng thuộc về người chơi thì giá bán tăng 5%
- **Khi mua hàng:** người chơi mua hàng thuộc về khách thì giá mua giảm 5%
- **Phân tích giá:** dòng `Gian Thương (+5%)` xuất hiện khi bán; dòng `Gian Thương (-5%)` xuất hiện khi mua
- **Giá và thanh toán:** giá từng món, tổng cộng, kiểm tra ngân sách và số Credits thanh toán dùng chung hàm tính giá đã điều chỉnh
- **Tiền khởi đầu:** +50 Credits
- **Hảo cảm:** -5 với từng phe có trong danh sách quan hệ của lượt chơi mới

## Cách hoạt động

- Harmony prefix/postfix trên `GameItem.GetNegociatedValue()` điều chỉnh giá giao dịch: nhân 1.05 khi bán cho khách và nhân 0.95 khi mua từ khách.
- Prefix guard ngăn việc áp dụng phần trăm nhiều lần nếu game gọi lồng nhau hàm tính giá cho cùng một món.
- Harmony postfix trên `NegociationUIManager.AddFeatureSection` thêm một `ItemFeature` tạm thời ở `ValueStage.Market` để hiện đúng dòng cộng hoặc trừ trong phần phân tích.
- Dòng phân tích chỉ phục vụ giao diện, không cộng/trừ tiền lần thứ hai.

## Ngôn ngữ

Tên perk, mô tả và dòng phân tích giá dùng tiếng Việt trực tiếp để không phụ thuộc locale nội bộ của game.

## Build

Giải nén `Managed.zip` vào `references/Managed/`, sau đó chạy:

    dotnet build GianThuongPerk/GianThuongPerk.csproj -c Release -p:ManagedDir="C:\\Path\\To\\references\\Managed"

CI build và xác minh ZIP cài đặt. Cần kiểm thử trực tiếp trong game để xác nhận cả hai chiều mua/bán và giao diện thực tế.
