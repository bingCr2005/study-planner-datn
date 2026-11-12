# 2026-11-10 — Lọc và nhóm nhiệm vụ đã hoàn thành theo ngày

## Yêu cầu và mục đích

Người dùng muốn bước nhỏ đã tick rời danh sách đang làm và có thể xem lại các việc đã làm theo từng ngày như ảnh TickTick. API danh sách đã tách việc hoàn thành, nhưng cây dùng khi mở nhiệm vụ cha vẫn trả cả bước đã xong. Xem [bản ghi làm mới trong nền](2026-11-10-lam-moi-danh-sach-trong-nen.md).

## Code và logic thay đổi

- Trên nhánh BE `feat/loc-nhiem-vu-da-hoan-thanh` tạo từ `main`, [request danh sách](../../backend/StudyPlanner.Application/HocTap/NhiemVu/DanhSachNhiemVuRequest.cs) nhận khoảng hoàn thành hoặc một ngày cụ thể. [NhiemVuService](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs) đổi ngày theo múi giờ tài khoản sang khoảng UTC nửa mở, lọc trên `NgayHoanThanh` và sắp xếp mới nhất trước khi đếm/phân trang theo thứ tự được FE yêu cầu. Ngày hoàn thành đã có trong DTO, không cần migration.
- Trong `Study_Planner_FE`, `src/hoc-tap/HocTapPage.tsx` đặt bộ chọn ngay đầu mục Đã hoàn thành. Các lựa chọn là tất cả, hôm nay, hôm qua, 7 ngày gần đây hoặc một ngày cụ thể. `boLoc.ts` giữ lựa chọn qua URL và gửi tham số phù hợp. Bộ lọc học kỳ/môn vẫn dùng được.
- `src/hoc-tap/NhiemVuView.tsx` ẩn bước đã xong trong cây ở danh sách đang làm, giữ toàn bộ cây trong khung chi tiết để xem lại và tính tiến độ cha. Khi tick, FE giữ màn hiện tại và làm mới dữ liệu trong nền. Mục Đã hoàn thành nhóm nhiệm vụ lá theo ngày thực tế, hiển thị số dòng của nhóm trên trang. Nhiệm vụ cũ thiếu thời điểm nằm ở nhóm chưa rõ ngày nếu xem tất cả. `types.ts` và `hoc-tap.css` bổ sung trường phản hồi và kiểu nhóm ngày.

## Kiểm chứng

Đã đọc lại diff BE và luồng query, nhóm ngày của FE. Build Release của BE Application đạt với 0 cảnh báo và 0 lỗi. Vite Oxc, TypeScript `--noEmit` và `git diff --check` đạt. Chưa kiểm tra HTTP với database hoặc kiểm thử bằng trình duyệt.

## Việc tiếp theo

Khởi động lại BE từ nhánh mới để FE gọi được bộ lọc và kiểu sắp xếp mới. Người dùng thử tick một bước con rồi mở Đã hoàn thành và lọc theo ngày thực tế.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã nguồn và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code/diff.
