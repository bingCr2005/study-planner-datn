# 2026-11-10 — Làm mới danh sách nhiệm vụ trong nền

## Yêu cầu và mục đích

Sau khi thêm, sửa hoặc xóa, người dùng thấy “Đang tải nhiệm vụ…” và danh sách xuất hiện lại. FE trước đó ẩn danh sách khi có lượt đọc API và dùng số lần tải làm khóa React, nên cả cây đang mở bị dựng lại. Xem [bản ghi đồng bộ](2026-11-09-dong-bo-va-thong-bao-nhiem-vu.md).

## Code và logic thay đổi

- `Study_Planner_FE/src/hoc-tap/HocTapPage.tsx` giữ dữ liệu đã đọc cho đúng tài khoản và bộ lọc hiện tại trong lúc lấy kết quả mới từ API. Chỉ hiện trạng thái tải đầy đủ khi bộ lọc đang xem chưa có dữ liệu. Nếu đọc lại lỗi, giữ các dòng cũ và hiện nút thử lại. Các lệnh ghi từ danh sách chờ phiên bản mới để tránh ghi dựa trên bản cũ.
- Tạo nhiệm vụ trong màn danh sách hoặc bảng trạng thái giữ bộ lọc, cách sắp xếp và trang hiện tại, rồi mở chi tiết nhiệm vụ vừa tạo. Việc mới chỉ hiện trong danh sách nếu khớp bộ lọc và thuộc trang đó. Đổi phạm vi vẫn có luồng điều hướng sang phạm vi mới.
- `src/hoc-tap/NhiemVuView.tsx` giữ trạng thái mở nhánh khi làm mới cùng bộ lọc và đọc lại cây của các nhiệm vụ gốc đang mở. Dùng cây đầy đủ để hiện các cấp con. Khóa lệnh ghi trên dòng con trong lúc cây đang đợi phiên bản mới. Ô thêm bước tiếp theo được khôi phục sau khi cây mới có dữ liệu.
- Tải hồ sơ để lấy múi giờ khi đổi phiên đăng nhập, bấm đồng bộ hoặc thử lại hồ sơ. Mỗi lần lưu nhiệm vụ chỉ kích hoạt các lượt làm mới dữ liệu học tập liên quan. Hợp đồng phiên bản và quy tắc xác nhận của BE tiếp tục được dùng.

## Kiểm chứng

Đã đọc lại luồng tạo, cập nhật, xóa và cách hủy yêu cầu khi đổi bộ lọc. Vite Oxc, TypeScript `--noEmit` và `git diff --check` đều đạt. Chưa thử hiệu ứng cuộn và giữ nhánh bằng trình duyệt.

## Việc tiếp theo

Người dùng có thể mở một nhánh có nhiều bước rồi sửa hoặc hoàn thành một bước để kiểm tra danh sách giữ nguyên trong lúc đợi API.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã nguồn và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code/diff.
