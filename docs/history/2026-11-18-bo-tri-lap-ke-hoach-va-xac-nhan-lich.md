# 2026-11-18 — Bố trí Lập kế hoạch và làm rõ xác nhận lịch

## Yêu cầu và mục đích

- Tham khảo bốn ảnh luồng lập kế hoạch; mỗi màn một việc, đường quay lại rõ và giữ bản nhập. Người dùng bổ sung yêu cầu tách hẳn sidebar Nhiệm vụ và Lập kế hoạch; kiểm tra tick hoàn thành ảnh hưởng hai phiên và đối chiếu 9 bài TickTick.
- Đã đọc README lịch sử, TEMPLATE, [bản ghi trước](2026-11-18-hoan-thien-lap-ke-hoach.md), module 07, source FE/BE hiện hành. Tiếp tục nhánh `feat/hoan-thien-lap-ke-hoach` ở hai repo; giữ các thay đổi sẵn có, không commit/push.
- Hai link chat mới chỉ trả trang tiêu đề. Computer Use dừng vì không xác định chắc URL hiện tại trong Chrome; không tiếp tục điều khiển hay coi nội dung chat là đã đọc. Dùng ảnh, lời người dùng trích trong hội thoại và tài liệu TickTick đọc được. Không có khẳng định đã áp dụng phần tư vấn chưa đọc được trong chat.

## Code và logic thay đổi

- FE `src/hoc-tap/PlanningOverview.tsx`, `planning-design.css`: tổng quan 5 lối vào, hướng dẫn ba việc chuẩn bị; dùng icon/CSS hiện có. Thêm route `/planning` ở `App.tsx`, đổi lối vào chính từ Lịch học sang Lập kế hoạch trong `AppLayout.tsx`. Route lịch cũ vẫn hoạt động, trạng thái sidebar/aria-current được đối chiếu.
- `HocTapPage.tsx`: menu Nhiệm vụ chỉ có nhiệm vụ, các nhóm lọc và bảng trạng thái/thùng rác. Menu Lập kế hoạch chỉ có tổng quan, thời khóa biểu, lớp, giờ rảnh, gợi ý và phiên. Chuyển phần bằng menu chính; không đặt hai bộ sidebar lẫn nhau. Giữ thu gọn menu mobile. Bảo vệ khởi tạo `matchMedia` khi render ngoài trình duyệt.
- `LapLichView.tsx`: **Thiết lập → Xem gợi ý → Hoàn tất**. Sắp form thành khối nhiệm vụ, ngày, cách lập và liên kết giờ rảnh/cấu hình. Giữ form/draft cũ khi quay lại. Hiển thị phút cần xếp thêm, phút đang chọn và phần chưa xếp sau khi lưu; danh sách đầy đủ có tổng phút đề xuất. Lý do từng phiên mở ở chi tiết riêng; cảnh báo có lối xử lý. Bước Hoàn tất chỉ xuất hiện sau API thành công, có nút xem phiên/lịch; retry dùng trường `daApDungTruoc` sẵn có, không tuyên bố tạo thêm phiên.
- `LichView.tsx`: giờ rảnh theo tuần đủ 7 ngày, mở từng ngày hoặc chuyển Danh sách; giữ thời hạn và hành động chỉnh lịch lặp/ngoại lệ. Phiên có bộ lọc trạng thái lưu trên URL, số phút và lối bắt đầu nhanh. Giữ trạng thái Bỏ lỡ vì nghiệp vụ có dùng. Thẻ Pomodoro/Đếm lên có chọn bằng chuột/phím mũi tên, sau đó bấm Bắt đầu học; chọn thẻ chưa gửi lệnh bắt đầu. Dùng lại trang Focus và cấu hình thực, không tạo đồng hồ khác.
- `HopXacNhan.tsx` thêm nhãn hủy tùy chọn và trả focus bằng `preventScroll: true`. `NhiemVuView.tsx` dùng nhãn **Giữ nguyên** / **Hoàn thành và hủy phiên** khi hoàn thành ảnh hưởng lịch. Trước đây nhãn chung Hủy phiên và tiếp tục dễ bị hiểu là bỏ thao tác. Xác nhận cấu trúc nhiệm vụ giữ nội dung riêng.
- `calendarPreferences.ts`, `LichView.tsx`, `lich.css`: đọc/kiểm tra/lưu tùy chọn hiển thị theo tài khoản ở localStorage. Nhớ chế độ, các lớp lịch, thu gọn hạn, hiện mục đã hoàn thành. Không lưu token hoặc thay đổi nghiệp vụ; dữ liệu hỏng/không có storage về mặc định. Ẩn mục hoàn thành chỉ lọc khi render, không xóa dữ liệu.
- `types.ts`: khai báo trường response `daApDungTruoc` vốn đã có ở backend. Không thêm migration, API hay sửa thuật toán ưu tiên trong lượt này.
- Đối chiếu backend `NhiemVuService.PhienBiAnhHuongAsync`: chỉ các phiên dự kiến còn liên quan bị hủy sau xác nhận; chặn nếu ảnh hưởng phiên đang học. Hoàn thành nhiệm vụ không xóa lịch sử đã học. Bấm Giữ nguyên không gửi lệnh hoàn thành; chỉ preview đọc ảnh hưởng.

## Kiểm chứng

- FE unit 183/183 đạt; build production đạt. Có test route/aria-current, hai sidebar không lẫn mục, lưu tùy chọn hợp lệ, dữ liệu hỏng, không có browser storage, tách tài khoản và cho phép ẩn mọi lớp lịch.
- Browser fixture lập lịch đạt: tổng quan, tuần giờ rảnh, cấu hình riêng, 3 bước, chọn 20+5 phút, chi tiết lý do, stale preview, xác nhận qua giờ bắt đầu, mất phản hồi/retry cùng khóa, quay lại/reload giữ ngày/chế độ/mã nhiệm vụ/tìm kiếm; desktop và 390px. Lượt cuối 337 yêu cầu fixture.
- Browser phiên/tập trung đạt 171 yêu cầu: chọn/hủy chế độ, chỉ bắt đầu sau xác nhận, chọn bằng bàn phím, giữ bộ lọc sau reload, Pomodoro/Đếm lên, tiến độ và sửa thực học/lịch sử. Đã xem ảnh chế độ desktop/mobile.
- Browser lịch đạt cả desktop và màn hẹp 980px, sau đó kiểm tra popover ở 390px: hai phiên Planned, Giữ nguyên không POST complete và không đổi checkbox/phiên; xác nhận mới gửi `huyPhienLienQuan=true`, hoàn thành và hủy đúng hai phiên. Vị trí cuộn trước/sau không đổi. Hai thẻ cùng nhiệm vụ đổi trạng thái đồng bộ. Lựa chọn ngày/ẩn hoàn thành tồn tại sau reload.
- Smoke toàn trang `node tests/browser-hoc-tap.mjs` đạt, 583 yêu cầu fixture: tạo/sửa/tách/chuyển nhiệm vụ, checklist, thùng rác có lỗi và thử lại, đặt/dời/hủy/xác nhận phiên, bắt đầu và kết thúc Focus, lịch lớp, giờ rảnh, preview/apply/retry, lịch tuần/tháng và tải lỗi. Đã cập nhật các kỳ vọng cũ: chọn menu theo nhãn; chi tiết mặc định 400 thay 310; thùng rác thay xóa vĩnh viễn; mở lại editor bước nhỏ sau khi lưu; chọn học kỳ/môn trước khi thêm lớp; đóng chi tiết phiên trước khi chuyển trang; chờ hộp lớp render; bước Hoàn tất thay tự điều hướng; chuyển khu vực qua menu chính. Whitelist ghi fixture có thêm đúng các endpoint Focus start/pause/resume/finish vì luồng hiện tại dùng trang Focus sẵn có. Không sửa nghiệp vụ để chiều test.
- `npm run lint` đạt exit 0, còn cảnh báo React sẵn có. Build còn cảnh báo kích thước chunk trên 500 kB. `git diff --check` đạt ở hai repo; liên kết tài liệu đã được kiểm tra.
- Kiểm thử trình duyệt dùng API fixture và UI thật, không phải chạy bằng dữ liệu Development. Không ghi/xóa lịch hoặc nhiệm vụ của người dùng. Các kiểm chứng HTTP/PostgreSQL của lượt trước là bằng chứng riêng, không ghi là đã chạy lại trong lượt này.

## Khác ảnh và việc tiếp theo

- Xem [bảng đối chiếu TickTick](../DOI-CHIEU-LICH-TICKTICK.md) cho 9 nguồn, phần đã có/mới và P2. Không dùng công thức tiến độ theo số phiên, không tạo đồng hồ thứ hai, không bỏ trạng thái Bỏ lỡ, không ghép sidebar hoặc tự kéo đổi deadline.
- Lượt này hoàn thiện UI trên code hiện tại; chưa triển khai dịch vụ hoặc đổi Database. Link chat vẫn chưa đọc được nội dung, không suy diễn các đoạn phía dưới câu người dùng trích.
- Việc còn lại: người dùng nghiệm thu Development theo vai trò sinh viên, nhất là thay đổi đồng thời và dữ liệu thật. Các ca test fixture đã cập nhật và chạy đạt; không thay cho nghiệm thu dữ liệu Development.

## Đọc lại trước khi đưa code lên

- [x] AI đã đọc code/diff liên quan và ghi đúng kết quả kiểm thử.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR: chưa tạo.
