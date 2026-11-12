# 2026-11-11 — Cải tiến chế độ xem thời khóa biểu

## Yêu cầu và mục đích

- Đối chiếu Thời khóa biểu trong Study Planner với các ảnh TickTick. Chọn các chế độ giúp sinh viên xem lịch lớp, hạn và phiên tự học theo ngày và tuần.
- Đọc [bản ghi tích hợp lịch](2026-11-06-tich-hop-fe-nhiem-vu-va-thoi-khoa-bieu.md), [luồng tự học](2026-11-10-luong-tu-hoc-va-bam-gio.md), code FE và [dịch vụ thời khóa biểu](../../backend/StudyPlanner.Application/HocTap/ThoiKhoaBieu/ThoiKhoaBieuService.cs). API hiện trả năm nhóm mục theo khoảng thời gian, học kỳ, môn và có giới hạn 93 ngày mỗi yêu cầu. Chọn các chế độ từ một ngày đến một tháng để phục vụ luồng học hằng ngày và theo dõi hạn gần.

## Code và logic thay đổi

- Trong repo `Study_Planner_FE`, `src/hoc-tap/LichView.tsx` xác định ngày đầu, độ dài khoảng xem và truy vấn `GET /calendar`. Trước đây FE dùng dữ liệu trả về gần nhất khi đổi khoảng và dựng lại toàn bộ màn mỗi lần đồng bộ. Bản mới chỉ hiển thị dữ liệu khớp tài khoản, bộ lọc và khoảng xem. Đồng bộ cùng khoảng tải trong nền, giữ ngày, chế độ xem và vị trí cuộn. Nút thêm lịch dùng phiên bản của lần tải hiện hành.
- `src/hoc-tap/LichKhungNhin.tsx` và `src/hoc-tap/lich.css` bổ sung Ngày, 3 ngày và Tuần theo trục giờ, 2 tuần và Tháng theo ô ngày, cùng Danh sách 7 ngày theo nhóm. Lưới giờ cuộn tới buổi đầu tiên của ngày đang chọn nếu có. Hạn nằm ở đầu cột vì chỉ có một mốc thời gian. Lịch lớp, sự kiện và phiên học nằm theo khung giờ. Giờ rảnh nằm ở lớp nền. Các mục dùng màu theo loại và màu môn ở mép thẻ nếu API trả màu. Có thể rê chuột xem ngày giờ đầy đủ, hoặc bấm mở chi tiết tương ứng.
- `src/hoc-tap/lich.ts` tìm ranh giới ngày trong múi giờ tài khoản, kể cả ngày đổi giờ tại nửa đêm. Lịch cắt sự kiện qua ngày theo khoảng nửa mở. Khối ngắn có chiều cao tối thiểu để thao tác. Chia cột dựa vào phần khối hiển thị đè lên nhau, kể cả khi hai khoảng thời gian tiếp giáp.
- `src/hoc-tap/HocTapPage.tsx` và `src/layouts/AppLayout.tsx` dành thêm chỗ cho lưới. Mở lại menu trong màn bằng Công cụ học tập, dùng bộ lọc học kỳ và môn khi cần. Nút Thêm vào lịch mở các biểu mẫu sẵn có. Bấm phiên tự học tiếp tục vào chi tiết phiên và đồng hồ khi đến giờ. Thay đổi nghiệp vụ hiển thị nằm ở FE và dùng hợp đồng lịch hiện có.
- Cập nhật [kịch bản demo](../KICH-BAN-DEMO-NHIEM-VU-VA-THOI-KHOA-BIEU.md) theo các chế độ và cách mở menu hiện tại.

## Kiểm chứng

- Đã đọc lại code và diff liên quan. TypeScript `node node_modules/typescript/bin/tsc --noEmit -p tsconfig.app.json`, phân tích CSS bằng PostCSS và `git diff --check` đạt.
- Kiểm tra logic lịch bằng Node với giờ Việt Nam, ngày chuyển giờ tại nửa đêm, hai mục ngắn kề nhau, khung giờ rảnh, hạn dạng mốc, sự kiện qua nửa đêm và khối sự kiện vào ngày đổi giờ.
- Kiểm tra giao diện trên trình duyệt với tài khoản và dữ liệu thực để người dùng thực hiện theo trao đổi trước đó.

## Việc tiếp theo

- Kiểm tra luồng từ phiên tự học đã áp dụng sang thời khóa biểu, đổi chế độ xem và đồng bộ ở tuần đang xem bằng dữ liệu đồ án.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code và kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR điền khi đã có.
