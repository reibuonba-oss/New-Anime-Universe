# Gian Thương — Perk khởi đầu

Mod độc lập cho Probably Stolen.

## Hiệu ứng

- **Tên:** Gian Thương
- **Loại / chi phí:** Perk Vàng (NEUTRAL), tốn 1 điểm perk
- **Khi bán hàng:** khách mua hàng thuộc về người chơi thì giá tăng 5%
- **Khi mua hàng:** người chơi mua hàng thuộc về khách thì giá giảm 5%
- **Phân tích giá:** dòng Gian Thương (+5%) hiện khi bán; dòng Gian Thương (-5%) hiện khi mua
- **Giá và thanh toán:** cả hai hiệu ứng được thêm bằng ItemFeature gốc ở ValueStage.Market nên đi vào cùng công thức giá của game
- **Tiền khởi đầu:** +50 Credits
- **Hảo cảm:** -5 với từng phe có trong danh sách quan hệ của lượt chơi mới

## Cách hoạt động

Game tự tạo các ItemFeature giao dịch ở hai điểm:

- `GameItem.OnAddedToWeightedArea`: vật phẩm thuộc người chơi được khách mua. Mod thêm feature `TemporarySelling` với `+5` ở giai đoạn Market.
- `GameItem.OnAddedToClientArea`: vật phẩm thuộc khách được người chơi mua. Mod thêm feature `ClientTemporary` với `-5` ở giai đoạn Market.

Đây là cùng hai loại feature mà game dùng cho Retail Markup và các hiệu ứng mua/bán khác. Giá từng món, tổng cộng, kiểm tra ngân sách, phần phân tích giá và thanh toán đều đọc từ danh sách ItemFeature này. Mod không còn cộng/trừ hậu kỳ trong `GetNegociatedValue()`, vì vậy tránh được tình trạng con số UI và tiền thanh toán đi theo hai luồng khác nhau.

Feature có ID riêng và được kiểm tra trước khi thêm để tránh bị nhân đôi khi game làm mới khu vực thương lượng. Các feature dùng kiểu tạm thời gốc của game để được dọn theo vòng đời giao dịch.

## Biên dịch

Giải nén `Managed.zip` vào `references/Managed/`, sau đó chạy:

    dotnet build GianThuongPerk/GianThuongPerk.csproj -c Release -p:ManagedDir="C:\\Path\\To\\references\\Managed"

GitHub Actions biên dịch và xác minh ZIP cài đặt. Cần kiểm tra trong game cả hai chiều mua/bán để xác nhận phần trăm áp dụng đúng trên bản save thực tế.
