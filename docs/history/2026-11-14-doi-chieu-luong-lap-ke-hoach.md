# 2026-11-14 — Đối chiếu luồng lập kế hoạch tự học

## Yêu cầu và mục đích

- Người dùng đưa luồng nhiệm vụ → lịch lớp → giờ rảnh → gợi ý phiên → đồng hồ → tiến độ, kèm góp ý cần thời lượng dự kiến và giờ rảnh lặp; muốn đánh giá phù hợp hệ thống. Người dùng cho phép tiếp tục và yêu cầu báo cáo đã sửa/tạo gì.
- Đã đọc mục lục, [tiến độ mới](2026-11-13-tong-hop-tien-do-nhiem-vu.md), [kịch bản tự học](2026-11-11-kiem-thu-luong-tu-hoc-dau-cuoi.md), module 07 và code BE/FE. Tài liệu còn nói chưa có thuật toán, chỉ xếp hạng; code đã có xếp giờ/preview/apply nên cần sửa mô tả.
- Nhánh `docs/doi-chieu-luong-lap-ke-hoach`, giữ các thay đổi nghiệp vụ đang chờ; không commit/push/triển khai.

## Code và logic thay đổi

- Sửa [module 07](../modules/07-goi-y-cong-viec.md), [mục lục module](../modules/README.md); tạo bản ghi này và liên kết trong [mục lục lịch sử](README.md). Không sửa ứng dụng/database.
- Trước: dự thảo GET recommendations/top N, quá hạn trước, chưa tự xếp giờ. Sau: mô tả đúng POST preview/apply, ước lượng, lá/cha, lặp giờ rảnh, tránh lịch bận, heuristic theo hạn/ưu tiên/thời lượng, phiên thường 25–50 phút/nghỉ 10 phút và ngoại lệ phần cuối dưới 25 phút.
- Luồng đối chiếu: tải lá/ước lượng còn lại → trừ phần đã dành chỗ → sinh giờ rảnh tuần/múi giờ → trừ lịch lớp/sự kiện/phiên/nghỉ → xếp trước hạn → preview/cảnh báo → chọn/apply → kiểm tra lại/ghi → đồng hồ → cập nhật tiến độ riêng.
- `ThoiLuongDuKienPhut` đã có; không thêm cột trùng. Không trừ thực học lần nữa khi phần còn lại đã tính theo ước lượng × tiến độ.
- Giờ rảnh đã lặp trong ngày hiệu lực, không cần job sao chép bản ghi từng tuần. Chưa có thông báo tuần mới/chọn nhiều thứ cùng form/ngoại lệ một lần. Sự kiện cá nhân có giờ có thể tạo khoảng bận được xếp lịch tránh.
- Đề xuất chưa triển khai: chọn nhiều thứ, sửa/bỏ riêng một ngày, tùy chọn phiên/nghỉ đồng bộ với đồng hồ, rà soát tiến độ/ước lượng rồi tạo gợi ý mới. Thuật toán hiện là tham lam xác định, chưa chứng minh tối ưu toàn cục hoặc cá nhân hóa. Đánh giá cần số phút xếp trước hạn/xung đột/phần chưa xếp trên bộ dữ liệu cụ thể.

## Kiểm chứng

- Đã đọc `NhiemVu`, `KhungGioRanh`, `QuyTacThoiGian`, dịch vụ giờ rảnh/thời khóa biểu, `QuyTacLapLich`, `LapLichService`, `QuyTacDatPhien`, DTO và controller lập lịch.
- FE đối chiếu `LichView.tsx` (một thứ mỗi form, hiệu lực mặc định 90 ngày), `LapLichView.tsx` (preview/chọn/apply), `FocusPage.tsx` (phiên kế hoạch bắt đầu Stopwatch).
- Đọc lại diff, kiểm tra liên kết tương đối và `git diff --check` đạt. Không chạy lại build/test vì chỉ sửa tài liệu; kết quả 13/11 không ghi thành lượt chạy mới.
- Không ghi dữ liệu/migration, không thao tác database ứng dụng hoặc gửi thông báo.

## Việc tiếp theo

- Các cải tiến nêu trên mới là đề xuất. Khi triển khai cần kiểm tra giao dịch nhiều khung, chỉnh một lần lặp và ảnh hưởng các phiên đã đặt.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
