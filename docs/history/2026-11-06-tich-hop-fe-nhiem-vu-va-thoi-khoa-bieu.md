# 2026-11-06 — Tích hợp FE nhiệm vụ và thời khóa biểu

## Yêu cầu và mục đích

- Người dùng yêu cầu chuyển phần nhiệm vụ, chia nhỏ và thời khóa biểu sang FE mới tại `C:/Users/ADMIN/Desktop/Study_Planner_FE`. Phạm vi thời khóa biểu bao gồm giờ rảnh, gợi ý và áp dụng lịch tự học.
- Dữ liệu học kỳ/môn đã tồn tại cần được chọn bằng tên. Bộ lọc, tìm kiếm và hướng dẫn dùng cách nói phù hợp với sinh viên.
- Đã đọc README/TEMPLATE lịch sử, bản ghi API học kỳ/môn ngày 2026-11-04, bản ghi core ngày 2026-11-05 trong bản lưu local, các controller/DTO/service BE và mã React hiện tại. Tài liệu module 03/05 còn ghi dự thảo trong khi BE đã triển khai, vì vậy đã cập nhật ghi chú trạng thái ở đầu hai module.
- FE dùng nhánh `feat/tich-hop-nhiem-vu-va-thoi-khoa-bieu`. Bản đầu vào đang có thay đổi riêng ở `package-lock.json`, đã lưu dấu kiểm tra nội dung để bảo toàn thay đổi đó.

## Code và logic thay đổi

### Phạm vi file

Repository FE có các file chính sau.

- `src/hoc-tap/HocTapPage.tsx` quản lý màn học tập, bộ lọc theo tên, phân trang API, lấy múi giờ tài khoản và hộp thoại.
- `src/hoc-tap/NhiemVuView.tsx` quản lý danh sách/bảng trạng thái, các bước nhỏ, biểu mẫu, tiến độ, đổi nhiệm vụ lớn, đổi phạm vi và lịch sử.
- `src/hoc-tap/LichView.tsx` quản lý lịch, mẫu buổi học theo môn, giờ rảnh và các phiên tự học.
- `src/hoc-tap/LapLichView.tsx` xem gợi ý, chọn phiên, áp dụng và khôi phục yêu cầu áp dụng đang chờ kết quả.
- `src/hoc-tap/api.ts`, `types.ts`, `time.ts`, `hoc-tap.css` cung cấp hợp đồng API, thời gian theo tài khoản và kiểu trình bày giới hạn trong module.
- `src/App.tsx` nối ba route `/tasks`, `/calendar`, `/schedule`. `src/layouts/AppLayout.tsx` nối nút Tạo nhiệm vụ với màn mới và chờ dữ liệu sẵn sàng.
- `tests/hoc-tap.test.mjs`, `tests/browser-hoc-tap.mjs` kiểm tra hợp đồng và thao tác trên trình duyệt. `tests/planner.test.mjs` cập nhật các kỳ vọng dành cho màn nhiệm vụ mới.

### Trước và sau

Trước tích hợp, FE mới dùng bộ trang nhiệm vụ/lịch đang khóa ghi API. Bản `frontend-preview` trong repository BE là giao diện riêng và yêu cầu nhập mã học kỳ/môn. Chạy BE chỉ khởi động API, giao diện React phụ thuộc repository FE đang chạy.

Sau tích hợp, ba route học tập dùng cùng phiên đăng nhập hiện có. Danh sách tên học kỳ/môn được lấy lại từ `PlannerContext` và `api/academic.ts` của team. Người dùng chọn tên, giá trị gửi tới API là ID BE tương ứng. Môn được giới hạn theo học kỳ đã chọn. Biểu mẫu lịch lớp yêu cầu chọn học kỳ và môn hợp lệ. Lưu buổi học mới chuyển bộ lọc sang môn vừa chọn để người dùng thấy kết quả.

Các file nguồn học kỳ/môn, xác thực, quản trị, hồ sơ, Dashboard, thống kê, thư viện API chung và các file CSS hiện có đã được đối chiếu dấu nội dung với bản đầu vào. Những file này trùng khớp. `package-lock.json` cũng trùng khớp với trạng thái có sẵn trước khi bắt đầu.

### Luồng nhiệm vụ

1. Màn hình đọc danh sách theo bộ lọc và trang từ API. Danh sách, bảng ba trạng thái và liên kết chế độ lịch sử dụng các tham số route hiện có.
2. Thêm nhiệm vụ bằng tên, nội dung, loại việc, ưu tiên, hạn và ước lượng. Chọn học kỳ/môn hoặc việc cá nhân bằng danh sách tên.
3. Mở chi tiết, thêm bước nhỏ hoặc chia tiếp một bước. BE kiểm tra phạm vi, hạn, tối đa 10 cấp và 1.000 nhiệm vụ trong một nhóm.
4. Tiến độ từng việc chưa chia nhỏ do người dùng cập nhật. BE tính tiến độ nhiệm vụ lớn từ các bước nhỏ rồi FE tải lại kết quả.
5. Chỉnh sửa, đổi nhiệm vụ lớn, tách riêng và đổi học kỳ/môn dùng API xem ảnh hưởng trước khi ghi. Xóa hiển thị số nhiệm vụ bị ảnh hưởng từ API.
6. Các nút mở rộng dùng “Xem các bước nhỏ”, “Thu gọn các bước nhỏ”. Thông báo lỗi về cấu trúc nhiệm vụ được diễn đạt bằng nhiệm vụ lớn/bước nhỏ.

### Luồng lịch và tự học

1. Người dùng tạo mẫu buổi học theo tên môn và khai báo khoảng rảnh theo thứ, giờ, ngày áp dụng.
2. Lịch hiển thị theo ngày, tuần, tháng hoặc danh sách. Có thể bật/tắt lịch lớp, sự kiện, hạn hoàn thành, giờ rảnh và tự học.
3. Người dùng nhập ước lượng cho việc chưa chia nhỏ, chọn khoảng lập lịch 1–31 ngày và các nhiệm vụ theo tên.
4. API xem trước trả phiên đề xuất, số phút cần xếp/đã dành chỗ/còn thiếu và lý do. Người dùng chọn phiên rồi bấm áp dụng.
5. FE lưu khóa và nội dung yêu cầu áp dụng đang chờ trong sessionStorage của tài khoản trước khi gửi. Mất kết nối giữ nguyên khóa/nội dung để thử lại. BE xử lý khóa chống tạo trùng và kiểm tra lại nguồn dữ liệu.
6. Có thể đặt/dời phiên thủ công. Các thao tác bắt đầu, kết thúc, bỏ lỡ, hủy được mở theo thời gian và trạng thái phiên. Sau khi học, người dùng cập nhật tiến độ nhiệm vụ theo kết quả thực tế.

### Bảo toàn nghiệp vụ khi ghi

- Mọi lệnh ghi thông thường gửi phiên bản đã đọc qua `If-Match`. Xem ảnh hưởng trả phiên bản khác thì biểu mẫu yêu cầu tải lại. Thao tác hoàn thành cũng dùng phiên bản của danh sách đang hiển thị.
- Các phiên tương lai cần hủy được hiển thị số lượng để người dùng xác nhận. Phiên đang học cần kết thúc trước khi sửa lịch liên quan. Trùng lịch lớp có xác nhận riêng theo API.
- Trạng thái và tiến độ trả từ BE là nguồn hiển thị sau khi lưu. Bộ nhiệm vụ local cũ được tách khỏi các route đã tích hợp.
- Múi giờ được xác minh từ hồ sơ BE trước khi mở thao tác đặt lịch. Trường thời gian đổi sang UTC khi gửi. Khi chỉ sửa nội dung và giữ nguyên phút đang hiển thị, FE giữ nguyên thời điểm gốc gồm độ chính xác giây/phần giây.
- Khung lịch dùng khoảng nửa mở để một mục kết thúc đúng 00 giờ thuộc ngày trước. Điều hướng tháng đi theo tháng lịch.

## Kiểm chứng

- `npm run build` trên repository FE thành công với TypeScript và Vite.
- `npm test` đạt 97/97 bài, gồm kiểm thử sẵn có và 14 bài mới cho client học tập/thời gian.
- `node tests/browser-hoc-tap.mjs` đạt. Trình duyệt chạy các route React thật với phản hồi HTTP giả lập, gồm chọn tên, tạo nhiệm vụ, bảng trạng thái, thêm bước nhỏ, giờ rảnh, lịch lớp, xem gợi ý, áp dụng, mất phản hồi rồi thử lại cùng khóa/nội dung, trạng thái phiên và lịch tháng. Lượt cuối ghi nhận 47 request giả lập. Có kiểm tra phạm vi route ghi để phát hiện ghi nhầm học kỳ/môn hoặc tài khoản.
- Ảnh lịch tháng đã được mở và kiểm tra bố cục desktop. Mã kiểm thử và ảnh trung gian được giữ trong thư mục obj bị Git bỏ qua.
- Host kiểm thử BE dùng database PostgreSQL cục bộ tạo mới rồi xóa. Bộ CoreChecks đạt 244 điều kiện HTTP/PostgreSQL, 15 điều kiện nhiệm vụ và kiểm tra thời gian thuần. Client prototype cũ đạt 11 điều kiện HTTP.
- Client React mới được nạp trực tiếp qua Vite rồi gọi host BE thật trong database kiểm thử trên. Đã xác minh danh sách tên/ID học kỳ và môn của team, tạo nhiệm vụ, thêm bước, xem ảnh hưởng/phiên bản, tổng hợp tiến độ, lịch sử và xóa trả 204. Bản sao host kiểm thử bổ sung đăng ký dịch vụ Academic, toàn bộ mã chuẩn bị nằm trong obj.
- Bài browser có sẵn `test:auth:browser` dừng ở phép đo chiều rộng thanh bên `54 !== 215`. Chạy lại bằng hai file App/Layout gốc cũng cho cùng lỗi. Nhánh kiểm thử môn học của team dừng ở `browser-subjects.mjs` dòng 140 do đọc select trước khi phần tử xuất hiện, bản gốc cũng tái hiện cùng lỗi. Hai bộ browser này chưa được tính là đạt.
- Đã đọc lại diff App/Layout và các module mới, kiểm tra các file được bảo vệ bằng dấu nội dung. `git diff --check` đạt cho diff FE.

## Việc tiếp theo và giới hạn

- Người dùng chạy `npm run dev` trong `C:/Users/ADMIN/Desktop/Study_Planner_FE`, đăng nhập bằng tài khoản BE và mở Nhiệm vụ hoặc Thời khóa biểu để test dữ liệu của mình.
- Danh sách chọn nhiệm vụ lớn/phiên hiển thị tối đa 100 kết quả theo tên. Nhập tên để thu hẹp danh sách. Bộ lọc nhiệm vụ chính dùng phân trang từ API.
- Luồng gợi ý/áp dụng trên giao diện đã kiểm bằng HTTP giả lập. Logic lập lịch thực tế được kiểm riêng trong bộ BE, phần nối FE thật với BE đã kiểm các thao tác nhiệm vụ nêu trên. Chưa chạy toàn bộ chuỗi gợi ý/áp dụng qua trình duyệt với database thật.
- Phần worker nhắc hạn, bộ đếm tập trung, đính kèm, thùng rác và đồng bộ Dashboard thuộc các đợt triển khai riêng.
- Tài liệu local trước khi đồng bộ BE vẫn có bản sao trong stash mang tên `local: tai-lieu-truoc-dong-bo-be-2026-11-06`. Bản ghi này bổ sung trạng thái mới theo mã đã kiểm tra, giữ bản lưu đó để phục hồi theo nhu cầu.
- Source FE đang là thay đổi local trên nhánh riêng. Người dùng quyết định nghiệm thu và thời điểm commit/push. Tài liệu thuộc repository BE là tài liệu local theo yêu cầu của người dùng.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu code/diff, ghi phạm vi và kết quả kiểm chứng thực tế.
- [ ] Người dùng đã đọc bản ghi và đối chiếu code. Chỉ người dùng xác nhận mục này.

Commit/PR chờ người dùng thực hiện hoặc yêu cầu.
