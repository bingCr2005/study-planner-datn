# 2026-11-10 — Nối luồng tự học, bấm giờ và cập nhật tiến độ

## Yêu cầu và mục đích

- Nối các màn lịch lớp, giờ rảnh, gợi ý, phiên tự học và nhiệm vụ để sinh viên biết học việc gì, vào lúc nào và làm gì tiếp theo.
- Thêm đồng hồ đếm lên theo phiên và mở tài liệu bằng liên kết đã dán trong nhiệm vụ. Xem [kịch bản demo học tập](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md).

## Code và logic thay đổi

- Trước đây áp dụng gợi ý chỉ hiện hướng dẫn bằng chữ. Trong repo `Study_Planner_FE`, `LapLichView.tsx` lấy mã lần áp dụng từ phản hồi và chuyển qua **Phiên tự học**. `HocTapPage.tsx` giữ mã lần áp dụng trong URL, `LichView.tsx` tải đúng các phiên của lần đó và cho mở tuần có phiên đầu tiên trên thời khóa biểu. Các màn lịch lớp và giờ rảnh có nút sang bước kế tiếp.
- [PhienTuHocController](../../backend/StudyPlanner.Api/Controllers/PhienTuHocController.cs) bổ sung truy vấn `maLanApDung`. [PhienTuHocService](../../backend/StudyPlanner.Application/HocTap/PhienTuHoc/PhienTuHocService.cs) kiểm tra nhóm thuộc tài khoản đang đăng nhập. Danh sách trả thêm tiêu đề và phạm vi của nhiệm vụ, giúp FE lọc và hiển thị tên ngay từ phản hồi danh sách.
- Trước đây bắt đầu/kết thúc chỉ đổi trạng thái. Nay `start` mở một bản ghi `DoanThucHoc` với giờ máy chủ, `finish` đóng đoạn tương ứng. Đoạn thực học, trạng thái và phiên bản được ghi trong cùng giao dịch. Giới hạn mỗi người một phiên đang học vẫn áp dụng. [PhienTuHocResponse](../../backend/StudyPlanner.Application/HocTap/PhienTuHoc/PhienTuHocResponse.cs) trả mốc đang bấm giờ và số giây đã ghi. FE tính đồng hồ theo mốc đã lưu, có thể mở lại sau khi tải lại trang. Xác nhận thủ công phiên quá giờ vẫn có thể hoàn thành mà không suy ra thời lượng.
- `NhiemVuView.tsx` hướng dẫn dán mỗi URL HTTP(S) trên một dòng trong nội dung nhiệm vụ. `LichView.tsx` nhận các dòng này và mở tài liệu ở tab mới. Sau khi kết thúc, FE dùng lại biểu mẫu cập nhật tiến độ nhiệm vụ. Với phiên bỏ lỡ, FE đưa sinh viên về màn gợi ý để xếp lại. Số giây bấm giờ và phần trăm nhiệm vụ là hai kết quả riêng.
- Tận dụng bảng `doan_thuc_hoc` và nội dung nhiệm vụ có sẵn. Phần bổ sung này không cần migration riêng.

## Kiểm chứng

- Đã đối chiếu thay đổi với quy tắc trạng thái, khóa phiên bản, ràng buộc `DoanThucHoc`, luồng áp dụng có khóa chống trùng và các màn đã có.
- Build Release của API đạt với 0 cảnh báo và 0 lỗi. TypeScript `--noEmit`, phân tích CSS bằng PostCSS và `git diff --check` trên mã thay đổi đạt.
- Bổ sung vào bộ CoreChecks hiện có các tình huống lọc theo lần áp dụng, từ chối tài khoản khác, đọc lại giờ bắt đầu và gọi lại `start`/`finish` mà thời gian vẫn giữ nguyên. Build Release của CoreChecks đạt với 0 cảnh báo và 0 lỗi. Cập nhật kịch bản FE cho điều hướng sau khi áp dụng và kiểm tra cú pháp bằng `node --check`.
- Chưa chạy luồng trên trình duyệt hoặc database theo yêu cầu người dùng tự thử giao diện.

## Việc tiếp theo

- Khởi động lại BE và tải lại FE. Tạo rồi áp dụng một gợi ý, mở phiên trong giờ đã đặt, bấm bắt đầu, tải lại và mở lại phiên để đối chiếu đồng hồ. Kết thúc rồi cập nhật tiến độ nhiệm vụ.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu mã và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan.
