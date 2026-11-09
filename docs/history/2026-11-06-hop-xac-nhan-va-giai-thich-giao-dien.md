# 2026-11-06 — Hộp xác nhận và giải thích giao diện học tập

## Yêu cầu và mục đích

- Người dùng chỉ ra hộp xác nhận xóa đang mang tiêu đề localhost và yêu cầu thông báo xóa đúng giao diện.
- Người dùng nhấn mạnh kịch bản trình bày phải giải thích được nội dung, mục đích và cách dùng của các thành phần trên từng màn, không chỉ liệt kê thao tác kiểm thử.
- Đã đọc [bản ghi menu/thanh kéo](2026-11-06-thanh-keo-va-menu-nhiem-vu-day-du.md), [bản ghi kịch bản](2026-11-06-kich-ban-demo-nhiem-vu-va-thoi-khoa-bieu.md), code FE hiện tại và các vị trí dùng confirm trong module.

## Code và logic thay đổi

- Repository FE là `C:/Users/ADMIN/Desktop/Study_Planner_FE`, các file thay đổi nằm trong `src/hoc-tap/` và hai file kiểm thử học tập.
- Thêm `HopXacNhan.tsx` và `xacNhan.tsx`. Hộp có tiêu đề, mô tả, số lượng ảnh hưởng, lưu ý, nút Hủy và nút xác nhận theo hành động. Dùng dialog có tên/mô tả cho công cụ hỗ trợ tiếp cận, đưa focus về Hủy, giữ focus trong hộp và xử lý Esc. Khi đóng trả lại focus nếu phần tử cũ còn tồn tại.
- `NhiemVuView.tsx` đọc deletion-preview và so phiên bản trước khi mở hộp. Số nhiệm vụ luôn hiển thị; các nhóm lịch nhắc/thông báo/tệp/phiên chỉ hiển thị khi có số lượng ảnh hưởng. Tên nhiệm vụ là văn bản React, không chèn HTML.
- Chỉ khi xác nhận mới gửi DELETE với phiên bản đã đọc. Đang xóa thì khóa nút và có cờ chống gửi lặp. Lỗi hiện trong hộp và giữ chi tiết. Thành công mới đóng hộp/chi tiết, tải lại và báo đã xóa. Nhiệm vụ con bị xóa cùng nhóm; lịch sử vẫn giữ theo BE hiện có.
- `HocTapPage.tsx` thêm thông báo thành công, có nút đóng và tự ẩn sau 7 giây. `hoc-tap.css` định dạng hộp, số lượng ảnh hưởng, nút đỏ và thông báo thành công, có bố cục màn nhỏ.
- `api.ts`, `LichView.tsx`, `LapLichView.tsx` đổi các confirm còn lại trong module thành cùng hộp giao diện, gồm ảnh hưởng phiên, trùng lịch lớp, xóa buổi học/giờ rảnh, bỏ yêu cầu chờ và áp dụng lịch. Hủy xác nhận trùng lớp trả về biểu mẫu đang mở thay vì phát sinh lỗi chung.
- Luồng BE, kiểm tra quyền, phiên bản, preview, giao dịch và khóa chống áp dụng lặp được giữ nguyên. Không thay đổi code tài khoản/học kỳ/môn học hoặc các file nguồn ngoài module trong lượt này.
- Bổ sung mục 10 của [kịch bản demo](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md): giải thích từng vùng, trường, bộ lọc, nút, lớp dữ liệu lịch và mối liên hệ nghiệp vụ, kèm lời trình bày.

## Kiểm chứng

- Build TypeScript/Vite bản cuối thành công. Bộ kiểm thử FE đạt 100/100; các kiểm thử xác nhận API được đổi sang quyết định bất đồng bộ thay cho window.confirm.
- Kiểm thử Chromium với dữ liệu giả lập chạy thành công ở bản cuối, có 260 request. Bao gồm Hủy/Esc không gửi DELETE, lỗi xóa giữ hộp và dữ liệu, gửi lại thành công, chống bấm lặp, trạng thái đang xóa, thông báo thành công, hộp ở màn rộng 390px và các xác nhận lịch lớp/giờ rảnh/áp dụng lịch. Không xuất hiện hộp JavaScript mặc định trong luồng thử.
- Một lượt đầu dừng ở phần xem phiên cá nhân sau F5 vì bộ lọc môn còn hiệu lực. Kịch bản kiểm thử được bổ sung Xóa bộ lọc trước khi xem chung phiên; các lượt sau đạt. Không sửa logic lọc để làm bài thử đạt.
- Đã mở ảnh hộp xóa desktop để kiểm tra bố cục. Ảnh nằm trong `backend/obj/fe-confirm-dialog/confirmation-delete.png`, được Git bỏ qua.
- Lint hai file hộp mới không có cảnh báo/lỗi. Lint các file module đã có không có lỗi nhưng còn cảnh báo React về thời gian trong render, effect và Fast Refresh; chưa mở rộng sửa các phần này.
- Đã rà code, kiểm tra không còn confirm/alert/prompt trong module học tập, kiểm tra khoảng trắng và so hash các file nguồn ngoài module với đầu lượt, không có thay đổi.
- Kiểm thử trình duyệt dùng dữ liệu giả lập, không xóa nhiệm vụ hoặc tạo dữ liệu trong database của người dùng. Chưa chạy thao tác xóa trên DB đang dùng của người dùng.

## Việc tiếp theo

- Người dùng tải lại FE để xem hộp mới. Phần trình bày theo màn nằm trong mục 10 của kịch bản demo.
- Thay đổi đang local; không commit/push, không đưa docs lên GitHub.

## Đọc lại trước khi đưa code lên

- [x] AI đã đọc lại code/tài liệu và ghi đúng kết quả kiểm chứng.
- [ ] Người dùng đã đọc và đối chiếu thay đổi. Chỉ người dùng xác nhận mục này.

Commit/PR cho thay đổi này chưa được tạo.
