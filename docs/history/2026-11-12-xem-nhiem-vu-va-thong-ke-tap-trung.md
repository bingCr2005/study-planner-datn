# 2026-11-12 — Xem nhiệm vụ và thống kê tập trung cơ bản

## Yêu cầu và mục đích

- Người dùng gửi ảnh lịch sử tập trung, cửa sổ nhiệm vụ và trang Statistics; yêu cầu áp dụng các phần đơn giản vào đồng hồ hiện có.
- Chọn hai phần có dữ liệu thực để dùng ngay: bấm tên nhiệm vụ xem nhanh và thống kê thời gian/Pomodoro/phiên theo ngày, khoảng ngày, nhiệm vụ. Không thêm điểm thành tích, xếp hạng người dùng, timeline thống kê nâng cao hoặc các tab thống kê hoàn thành công việc của sản phẩm tham khảo.
- Đã đọc [mục lục lịch sử](README.md), [bản ghi đồng hồ riêng](2026-11-12-man-dong-ho-tap-trung-rieng.md), tài liệu module và code BE/FE hiện hành. Tiếp tục nhánh `feat/bat-dau-phien-hoc-ngay` vì cùng nhóm chức năng; giữ các thay đổi trước.

## Code và logic thay đổi

### Cửa sổ nhiệm vụ và menu

- FE trong repository `C:/Users/ADMIN/Desktop/Study_Planner_FE`: thay `src/focus/FocusPage.tsx`, thêm `FocusDialog.tsx`, `FocusTaskPreview.tsx`, `FocusStatistics.tsx`, `focus-statistics.ts`, `focus-details.css` cùng thư mục.
- Trước: bấm bản ghi chỉ mở thông tin phiên/ghi chú; xem nhiệm vụ phải điều hướng khỏi màn đồng hồ. Nút ba chấm mở thẳng cài đặt, chưa có thống kê theo khoảng ngày.
- Sau: hàng lịch sử dùng article với các nút riêng, tránh lồng button. Bấm biểu tượng/giờ/thời lượng mở bản ghi; bấm tên nhiệm vụ mở xem nhanh. Tên nhiệm vụ và nút Xem nhiệm vụ trong chi tiết bản ghi cũng mở cửa sổ này. Bản ghi tự do vẫn mở chi tiết phiên.
- `FocusTaskPreview` đọc `/tasks/{id}`, tổng focus của nhiệm vụ và `/tasks/{id}/tree` nếu có các bước nhỏ. Hiện trạng thái, ưu tiên, hạn, tiến độ, mô tả, checklist chỉ đọc, các bước nhỏ trực tiếp và phạm vi học tập/cá nhân. Thời gian focus là toàn bộ lịch sử của đúng nhiệm vụ, không tự cộng nhánh con. Nút Chỉnh sửa trong Nhiệm vụ mở `/tasks?task={id}`, dùng luồng sửa hiện có.
- Đọc lỗi có thông báo và Thử lại; nhiệm vụ bị xóa/không thuộc tài khoản có thông báo không tìm thấy. Không giả số thời gian bằng 0 khi API lỗi. Khi đóng, hủy request đang chờ.
- Nút ba chấm mở menu Thống kê / Cài đặt đồng hồ; Escape hoặc bấm ngoài đóng menu. `FocusDialog` dùng dialog native chung; mở nhiệm vụ từ bản ghi/thống kê rồi đóng sẽ trở lại cửa sổ bên dưới.

### Thống kê và dữ liệu

- [ThongKeTapTrungService.cs](../../backend/StudyPlanner.Application/HocTap/TapTrung/ThongKeTapTrungService.cs), [ThongKeTapTrungResponse.cs](../../backend/StudyPlanner.Application/HocTap/TapTrung/ThongKeTapTrungResponse.cs): thêm hai truy vấn chỉ đọc, đăng ký trong [HocTapRegistration.cs](../../backend/StudyPlanner.Api/HocTap/HocTapRegistration.cs), công bố qua [TapTrungController.cs](../../backend/StudyPlanner.Api/Controllers/TapTrungController.cs) và [MoTaSwagger.cs](../../backend/StudyPlanner.Api/Administration/MoTaSwagger.cs).
- `GET /api/v1/focus/statistics?ngay=YYYY-MM-DD&soNgay=7`: chỉ nhận 1, 7, 30 ngày; ngày cuối mặc định hôm nay theo múi giờ tài khoản, không cho tương lai. Lấy Completed của tài khoản theo khoảng ngày kết thúc, đổi biên ngày từ múi giờ người dùng sang UTC trước khi lọc. Phiên qua nửa đêm thuộc ngày kết thúc, không chia sang hai ngày. Tạo đủ các ngày kể cả giá trị 0.
- Tổng thực học dùng mili giây đã lưu, loại phiên đang chạy/tạm dừng/bị bỏ và thời gian nghỉ. Pomodoro tính số mục tiêu đầy đủ như tổng quan hiện hành. Nhóm nhiệm vụ theo ID để đổi tên không tách thành nhiều nhóm; bản ghi không còn liên kết được nhóm theo tiêu đề đã lưu.
- Khoảng trên 10.000 phiên trả QUERY_LIMIT để chọn khoảng ngắn hơn, không trả tổng bị cắt. Truy vấn không chốt countdown và không tăng phiên bản lập lịch.
- `GET /api/v1/focus/tasks/{id}/summary`: kiểm tra nhiệm vụ chưa xóa và đúng chủ sở hữu, tổng hợp toàn bộ nhóm Completed của đúng ID. Không có dữ liệu trả tổng 0; nhiệm vụ đã xóa/khác chủ trả 404. Không ghi tiến độ/checklist.
- `FocusStatistics`: bộ lọc Ngày / 7 ngày / 30 ngày và ngày kết thúc; bốn số thời gian, Pomodoro, phiên, trung bình mỗi ngày; cột theo ngày, vòng phân bổ theo nhiệm vụ. Lấy 5 mục lớn nhất và gộp Các mục khác giữ nguyên tổng; nhãn nhiệm vụ có liên kết mở xem nhanh. Khoảng rỗng hiện dữ liệu 0 thực và vòng trung tính.
- Kết quả gắn khóa theo khoảng/ngày/lần tải; request cũ bị hủy khi đổi bộ lọc. Đang tải/lỗi không hiện biểu đồ của khoảng cũ. Có Thử lại khi thất bại.
- Xem các cửa sổ không gọi pause/resume. Sửa xử lý phản hồi trong `FocusPage` để auto finish chỉ đóng cửa sổ xác nhận kết thúc, giữ thống kê/nhiệm vụ đang mở. Khi state phát hiện phiên được chốt, phát sự kiện `studyplanner:focus-changed`; hai cửa sổ đọc lại số liệu. Không tự cập nhật phần trăm theo số phút.
- Không thay schema hoặc sinh migration trong phần này; dùng bảng focus của bản ghi trước. Migration focus trước đó vẫn là điều kiện chạy nếu database ứng dụng chưa có bảng. Không áp dụng migration vào database người dùng.
- Cập nhật [module 09](../modules/09-dong-ho-tap-trung.md) theo code đã xác minh, gồm API và ý nghĩa ngày kết thúc.

## Kiểm chứng

- Đã đọc lại code/diff controller, DI, service/DTO thống kê, Swagger; FE dialog, trang focus, menu/hàng lịch sử, cửa sổ nhiệm vụ/thống kê, helper phân bổ, CSS và fixture/tests. `git diff --check` đạt ở hai repository.
- BE: build Release API và CoreChecks đạt, 0 warning, 0 error. CoreChecks focus chạy qua HTTP và PostgreSQL kiểm thử riêng đạt **121 assertions**, gồm múi giờ qua nửa đêm, đủ 7/30 ngày, tổng theo nhiệm vụ, ngày rỗng, từ chối ngày/khoảng sai, 401/404 và không đọc chéo tài khoản/không tăng phiên bản. Các kiểm tra thuần của CoreChecks cũng đạt.
- Database kiểm thử tạm được tạo/xóa trong finally; cụm PostgreSQL kiểm thử riêng đã dừng sau khi chạy. Không dùng database ứng dụng và không ghi dữ liệu người dùng.
- FE: `node --test tests/focus-statistics.test.mjs tests/focus.test.mjs tests/hoc-tap.test.mjs` đạt **27/27**. Ba kiểm tra mới xác nhận vòng rỗng, gộp phần còn lại giữ tổng và tỷ lệ màu theo thời gian thực.
- `CORE_FOCUS_ONLY=1` với `node tests/browser-hoc-tap.mjs` đạt, **108 fixture requests**: popup từ tên lịch sử/bản ghi, hai dialog lồng nhau, mô tả/checklist/tiến độ và các bước nhỏ, bộ lọc 1/7/30, lỗi 503 rồi thử lại không giữ số cũ, ngày rỗng, nhiệm vụ từ phân bổ, mở thống kê lúc đồng hồ chạy, tự hết Pomodoro giữ cửa sổ và làm mới số liệu, bố cục điện thoại. Không có lệnh ghi tiến độ/hoàn thành nhiệm vụ.
- `CORE_SESSION_ONLY=1` cùng harness đạt, **66 fixture requests**, xác nhận luồng bắt đầu sớm từ phiên dự kiến, khôi phục đồng hồ và kết thúc không tự tăng tiến độ vẫn hoạt động.
- `npm run build` đạt TypeScript và production build. Vite còn cảnh báo chunk JS chính trên 500 kB; không phải lỗi build.
- Đã xem ảnh chụp kiểm thử tại `backend/obj/focus-simple-screenshots/`: xem nhiệm vụ tiến độ 50% và checklist tương ứng, thống kê desktop và mobile 390×844. Đây là dữ liệu fixture, không phải lịch sử học thật của tài khoản.
- Chưa chạy FE → API thật → PostgreSQL với tài khoản người dùng; browser dùng HTTP fixtures, BE được kiểm chứng HTTP/PostgreSQL riêng. Bộ browser toàn ứng dụng chưa chạy lại trong thay đổi này; bản ghi trước đã ghi nhận lỗi assertion bố cục menu học tập có sẵn, không coi bộ toàn ứng dụng là đã đạt.

## Việc tiếp theo

- Khởi động lại BE và tải lại FE để dùng hai API và giao diện mới. Nếu chưa áp dụng migration focus ở bước trước, làm theo module 09 khi chuẩn bị database ứng dụng; thay đổi này không có migration bổ sung.
- Người dùng xem thực tế và xác nhận bố cục/phạm vi. Chưa commit, push hoặc triển khai.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
