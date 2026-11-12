# 2026-11-10 — Đổi tên và thêm bước ngay trên danh sách

## Yêu cầu và mục đích

Người dùng muốn sửa tên trực tiếp ở dòng nhiệm vụ như trong ảnh. Enter lưu và chuyển đến bước kế tiếp cùng cha. Bước mới mặc định hạn hôm nay. Xem [bản ghi thêm bước trong chi tiết](2026-11-10-them-buoc-nho-tai-cho.md).

## Code và logic thay đổi

- Trong `Study_Planner_FE`, `src/hoc-tap/NhiemVuView.tsx` chuyển tên thành ô nhập khi bấm lại dòng đã chọn hoặc nhấn F2. Enter gửi tên mới qua luồng xem ảnh hưởng và cập nhật hiện có, dùng các giá trị hiện tại cho những trường còn lại. Nếu tên giữ nguyên thì chỉ chuyển ô. Escape thoát ô nhập.
- Ở cây đang mở, Enter chuyển tới bước cùng cha tiếp theo. Tại bước cuối, Enter mở một ô tạo bước cùng cấp. Tên trống chưa được gửi. `src/hoc-tap/HocTapPage.tsx` chuyển đường dẫn và ô cần focus qua lần tải lại để khôi phục nhánh. CSS trong `src/hoc-tap/hoc-tap.css` giữ ô nhập trong dòng.
- Khi tạo bước trên danh sách, trong chi tiết hoặc qua biểu mẫu đầy đủ, hạn mặc định là 23:59 hôm nay theo múi giờ tài khoản. Nếu hạn hiệu lực của cha đến sớm hơn hoặc bằng mốc đó, để trống hạn riêng để dùng hạn của cha. BE vẫn áp dụng [quy tắc hạn theo cây](../../backend/StudyPlanner.Application/HocTap/QuyTacCay.cs).
- Đã xóa script hỗ trợ tạm `backend/obj/apply-inline-child-editor.py`. Lượt chỉnh sửa này ghi trực tiếp vào repo FE qua stdin.

## Kiểm chứng

Đã đọc lại mã và đối chiếu với quy tắc hạn của BE. `git diff --check` đạt. Đã cập nhật kỳ vọng hạn trong kịch bản Chromium. Chưa chạy build hoặc test theo yêu cầu để người dùng tự kiểm tra.

## Việc tiếp theo

Người dùng thử Enter từ bước đầu, từ bước cuối và khi nhiệm vụ cha có hạn sớm hơn hôm nay.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã nguồn và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code/diff.
