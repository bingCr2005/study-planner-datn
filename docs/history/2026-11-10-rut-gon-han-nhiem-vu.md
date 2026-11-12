# 2026-11-10 — Rút gọn hạn nhiệm vụ

## Yêu cầu và mục đích

Người dùng muốn rút gọn nhãn hạn như ảnh TickTick và xem ngày giờ đầy đủ khi rê chuột. Trước đó dòng nhiệm vụ và đầu khung chi tiết luôn hiện cả ngày lẫn giờ. Xem [bản ghi đưa hạn lên đầu chi tiết](2026-11-09-dua-han-len-dau-chi-tiet.md).

## Code và logic thay đổi

- `src/hoc-tap/NhiemVuView.tsx` dùng chung `HanNhiemVu` cho dòng danh sách, bảng trạng thái và đầu chi tiết. Dùng ngày hiện tại theo múi giờ tài khoản. Hạn trong ngày hiện giờ, riêng mốc 23:59 hiện “Hôm nay”. Ngày liền trước hoặc liền sau hiện “Hôm qua” hoặc “Ngày mai”. Ngày khác hiện ngày/tháng và thêm năm khi khác năm hiện tại.
- Tooltip gốc của trình duyệt hiển thị `HH:mm · dd/MM/yyyy`. Nội dung đầy đủ có sẵn cho trình đọc màn hình. `src/hoc-tap/hoc-tap.css` giữ nhãn trên một dòng và dùng con trỏ trợ giúp. Hạn hiệu lực, nguồn hạn kế thừa, màu quá hạn và trạng thái hoàn thành vẫn lấy theo luồng sẵn có. Thay đổi chỉ định dạng hạn nhiệm vụ trong FE.

## Kiểm chứng

Đã đọc lại nơi sử dụng nhãn và đối chiếu hợp đồng hạn hiệu lực từ BE. `git diff --check` đạt. Chưa chạy build hoặc test theo yêu cầu để người dùng tự kiểm tra.

## Việc tiếp theo

Người dùng thử rê chuột lên nhãn giờ trong ngày, hạn kế thừa và hạn ở năm khác để xem định dạng đầy đủ.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã nguồn và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code/diff.
