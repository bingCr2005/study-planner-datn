# 2026-11-11 — Đưa hoạt động nhiệm vụ vào menu chi tiết

## Yêu cầu và mục đích

- Khối Lịch sử thay đổi ở cuối chi tiết gây khó hiểu và chiếm chỗ. Lịch sử hữu ích khi sinh viên cần tra thời điểm đã hoàn thành, mở lại hoặc đổi thông tin của một việc.
- Đọc [bản ghi bố cục chi tiết](2026-11-10-lam-gon-giao-dien-nhiem-vu.md), [đặc tả lịch sử](../modules/04-lich-su-nhiem-vu.md) và đối chiếu với `LichSuAsync` cùng `Snapshot` trong [NhiemVuService](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs). Tài liệu trước đó còn ghi API chưa triển khai và mô tả tab lịch sử, nên đã cập nhật theo code.

## Code và logic thay đổi

- Trong `Study_Planner_FE`, `src/hoc-tap/NhiemVuView.tsx` chuyển khối dưới Thông tin nhiệm vụ sang mục **⋯ → Hoạt động nhiệm vụ** trong thanh đầu chi tiết. Mở hộp hoạt động riêng giúp giữ nhiệm vụ đang xem, kể cả khi chi tiết được mở từ lịch. Đóng hộp quay lại chi tiết đang mở.
- `src/hoc-tap/HoatDongNhiemVu.tsx` gọi `GET /task-history` theo mã nhiệm vụ khi người dùng mở hộp. Đọc 15 dòng mỗi lần, nhóm theo ngày và dùng múi giờ tài khoản. Có trạng thái tải, chưa có hoạt động, thử lại và xem thêm. Đóng hộp hủy yêu cầu đang chạy.
- Dòng hoạt động hiển thị hành động và giờ. Thay các mục Trước thay đổi và Sau thay đổi dạng JSON bằng nhãn dễ đọc như Đã đổi hạn hoặc Đã cập nhật tiến độ. Bản ghi thiếu dữ liệu so sánh dùng nhãn Đã chỉnh sửa. `src/hoc-tap/hoc-tap.css` tạo bố cục gọn với giờ căn phải và nhãn Tự động cho các dòng do hệ thống ghi.
- Dùng API và dữ liệu lịch sử hiện có. Cập nhật [kịch bản demo](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md) và kịch bản `tests/browser-hoc-tap.mjs` theo vị trí mở mới.

## Kiểm chứng

- Đã đọc lại diff FE, điều kiện mở và đóng hộp, yêu cầu phân trang và định dạng dữ liệu từ backend.
- TypeScript `--noEmit`, PostCSS, `node --check` của kịch bản trình duyệt và `git diff --check` đạt. Đã kiểm tra logic tóm tắt hành động với dữ liệu mẫu.
- Phần trình duyệt dùng dữ liệu thực để người dùng kiểm tra theo trao đổi trước đó.

## Việc tiếp theo

- Mở một nhiệm vụ có lịch sử sửa tên hoặc hạn và xem hoạt động từ menu trong chi tiết.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR điền khi đã có.
