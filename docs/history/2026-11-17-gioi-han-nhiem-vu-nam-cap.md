# 2026-11-17 — Giới hạn nhiệm vụ ở 5 cấp

## Yêu cầu và mục đích

- Áp dụng cách đếm của TickTick. Gốc là cấp 1, nhiệm vụ cấp 4 có thể thêm con cấp 5, và từ cấp 5 ẩn **Thêm bước nhỏ**. Ảnh tham khảo mới cho thấy các bước lồng nhau xuất hiện trong khung chi tiết bên dưới tệp đính kèm. [Cách TickTick đếm cấp](https://blog.ticktick.com/2020/08/10/how-task-nesting-helps-you-with-gtd/)
- Web đã có dữ liệu sâu hơn 5 cấp do [quy tắc 10 cấp trước đây](2026-11-06-tich-hop-fe-nhiem-vu-va-thoi-khoa-bieu.md). Thay đổi áp dụng khi tạo con hoặc đổi cha và cho phép tiếp tục mở, sửa, tách nhánh cũ.

## Code và logic thay đổi

- [QuyTacCay.cs](../../backend/StudyPlanner.Application/HocTap/NhiemVu/QuyTacCay.cs) đếm từ 1 và giới hạn 5 cấp cho các mã được chọn kiểm tra độ sâu mới. Vượt giới hạn trả `409 TASK_DEPTH_LIMIT`. Kiểm tra chung vẫn đọc được cây đã lưu đến 10 cấp và giữ giới hạn 1.000 nhiệm vụ mỗi cây. Cách này duy trì việc đọc cây, dùng hạn kế thừa và chỉnh sửa nhánh đã tồn tại.
- [NhiemVuService.cs](../../backend/StudyPlanner.Application/HocTap/NhiemVu/NhiemVuService.cs) đưa các bước vừa tạo và các bước sinh để giữ checklist/tiến độ cũ vào kiểm tra trước khi ghi. Khi đổi cha, backend kiểm tra cả nhánh được chuyển, gồm các hậu duệ. Cùng quy tắc chạy ở xem trước và khi lưu. Khi khôi phục từ thùng rác, nhánh trở lại thành nhiệm vụ gốc nếu gắn vào cha cũ làm vượt giới hạn.
- Frontend `src/hoc-tap/NhiemVuView.tsx` lấy cấp thực từ `duongDanNhiemVuCha`, kể cả khi nhiệm vụ được ghim hoặc xuất hiện riêng trên danh sách. Cấp 5 trở sâu hơn ẩn mục thêm con trong menu và biểu mẫu. Có thể thêm nhiệm vụ cùng cấp 5 dưới cha cấp 4. Khung chi tiết gom các bước theo mã cha, hiển thị có thụt lề dưới phần tệp đính kèm và cung cấp nút thu gọn/mở rộng từng nhánh. `hoc-tap.css` định dạng nút mở nhánh, `api.ts` hiển thị thông báo API khi vượt độ sâu.
- Mục Q05 và kịch bản demo trước đó ghi giới hạn chưa chốt hoặc 10 cấp. Hai tài liệu đã được cập nhật theo quy tắc tạo và chuyển nhánh mới.

## Kiểm chứng

- Build Release CoreChecks đạt với 0 lỗi và 0 cảnh báo. Chạy toàn bộ CoreChecks trên PostgreSQL tạm, đạt 24 kiểm tra cây/hạn/tiến độ và 680 kiểm tra HTTP lõi. Các ca mới kiểm tra tạo đến cấp 5, từ chối cấp 6, chặn chuyển nhánh khi hậu duệ vượt giới hạn và mở/chỉnh sửa nhiệm vụ cũ ở cấp 6. PostgreSQL tạm đã dừng.
- Frontend build và lint thoát thành công, có cảnh báo. `CORE_TASK_DEPTH_ONLY=1 node tests/browser-hoc-tap.mjs` đạt trên Chromium với dữ liệu giả lập. Kiểm tra mở/thu gọn cây trong chi tiết, vị trí sau tệp đính kèm, cấp 4 có form thêm, cấp 5 ghim không hiện mục thêm và nhiệm vụ cũ cấp 6 vẫn mở được. Hai lượt `CORE_PROGRESS_ONLY=1` và `CORE_ATTACHMENTS_ONLY=1` cũng đạt.
- Bộ browser tổng hợp dừng ở assertion kiểm tra vị trí thanh điều hướng Học tập. Thay đổi này chỉ chạm giao diện danh sách và chi tiết nhiệm vụ. `git diff --check` đạt ở hai repo.

## Việc tiếp theo

- Các thay đổi ở hai repo nằm trên nhánh `fix/gioi-han-nhiem-vu-nam-cap`. Các chỉnh sửa local có từ trước được giữ trong working tree khi chuyển nhánh.

## Đọc lại trước khi đưa code lên

- [x] AI đã đọc lại code/diff và ghi kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.
