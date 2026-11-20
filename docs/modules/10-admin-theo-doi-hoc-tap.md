# Module 10 — Admin theo dõi học tập sinh viên

Trạng thái ngày 18/11/2026: đã có API và giao diện trên nhánh `feat/admin-theo-doi-hoc-tap-sinh-vien`. Đây là phần theo dõi dữ liệu học tập; quyền quản lý tài khoản, danh mục mẫu và quy tắc vẫn thuộc các module quản trị hiện có.

Cập nhật 19/11/2026: code đã được tích hợp vào checkout gốc backend/frontend trên nhánh `feat/tich-hop-quan-tri-sinh-vien`, cùng chức năng thêm/sửa tài khoản. Xem [kiểm chứng bản tích hợp](../history/2026-11-19-tich-hop-admin-vao-ban-dang-chay.md). Các giới hạn và việc chưa chạy trên dữ liệu thật của lần 18/11 là lịch sử kiểm thử nhánh độc lập.

## Phạm vi và quyền riêng tư

- Backend yêu cầu policy Admin tại toàn bộ `/api/v1/admin/learning`. Sinh viên nhận 403; chưa đăng nhập nhận 401. Chỉ có GET, không mở thao tác sửa/xóa nhiệm vụ hoặc giả danh sinh viên.
- Nhiệm vụ được xem phải thuộc tài khoản Student, có môn hoặc học kỳ và không mang loại `Personal`. Nhiệm vụ Personal vẫn được giữ riêng dù được gắn học kỳ. Thùng rác bị loại theo query filter hiện có.
- DTO riêng chỉ trả mã/tên sinh viên, mã sinh viên, tên nhiệm vụ, môn/học kỳ, mã cha, trạng thái, ưu tiên, tiến độ và hạn hiệu lực. Không trả mô tả, checklist, tệp đính kèm, đường dẫn lưu tệp, ghi chú, giờ rảnh hoặc thông tin xác thực.
- Hồ sơ học tập chỉ có thông tin nhận diện tối thiểu, trạng thái tài khoản, múi giờ và danh sách học kỳ/môn. Không thay đổi bộ kiểm tra chủ sở hữu của API sinh viên. Tài khoản Student bị khóa vẫn được theo dõi; tài khoản Admin không được chọn làm sinh viên.

## Giao diện

Trang `/admin/learning` có thẻ thống kê, bảng nhiệm vụ và bộ lọc tên nhiệm vụ/tên sinh viên/mã sinh viên, trạng thái, quá hạn và bước nhỏ. Bấm tên sinh viên hoặc nút **Xem học tập** trong chi tiết tài khoản để mở `?student=...`; lúc đó có thêm học kỳ, môn, kế hoạch và thời gian học.

Các bộ lọc nhiệm vụ và phân trang được giữ trong URL. Thống kê mặc định chỉ đếm nhiệm vụ chính để không cộng trùng cây. Khi bật **Gồm bước nhỏ**, giao diện nói rõ số liệu có tính các bước con. Thẻ đếm theo bộ lọc hiện hành, không đếm từ trang đầu của bảng.

Kế hoạch/thời gian học có khoảng ngày riêng, ban đầu là 7 ngày lịch gồm hôm nay theo múi giờ sinh viên. Chọn đến hết ngày cuối được chuyển sang mốc đầu ngày kế tiếp trước khi gọi API. Các bộ lọc môn/học kỳ của bảng nhiệm vụ không lọc khối kế hoạch/thời gian; khối này tổng hợp học tập của sinh viên trong khoảng ngày riêng đã chọn.

## API và cách tính

| GET dưới `/api/v1/admin/learning` | Kết quả |
| --- | --- |
| `/summary` | Tổng nhiệm vụ, hoàn thành, đang làm, chưa bắt đầu, quá hạn, số sinh viên có nhiệm vụ và mốc kiểm tra |
| `/tasks` | Phân trang nhiệm vụ; mặc định 20, tối đa 100 bản ghi/trang |
| `/students/{id}` | Hồ sơ tối thiểu và các học kỳ/môn của sinh viên |
| `/students/{id}/sessions?tu=...&den=...&trang=1` | Phiên tự học giao với khoảng ngày; 20 dòng/trang, có cả trạng thái hủy/bỏ lỡ |
| `/students/{id}/focus-summary?tu=...&den=...` | Số phiên và thời gian ghi nhận, tách bấm giờ/nhập tay |

`summary` và `tasks` dùng `MaSinhVien`, `MaHocKy`, `MaMonHoc`, `TuKhoa`, `TrangThai`, `ChiQuaHan`, `GomBuocNho`, `Trang`, `KichThuocTrang`. Sinh viên/học kỳ/môn không thuộc phạm vi đã chọn trả 404; đầu vào sai trả 400. Khoảng kế hoạch/thời gian phải dương và không quá 93 ngày tuyệt đối.

- Mỗi API đọc dùng một transaction RepeatableRead và AsNoTracking. Tổng số và trang dữ liệu của cùng API lấy từ cùng ảnh chụp; không tăng phiên bản học tập và không gọi SaveChanges.
- Hạn kế thừa dùng lại biểu thức `QuyTacCay.BieuThucHan`. Hai truy vấn lấy trường hiển thị và hạn dùng đúng cùng thứ tự/phân trang trong cùng transaction; không tải từng cây theo từng dòng.
- Quá hạn: chưa Completed và hạn hiệu lực nhỏ hơn thời điểm máy chủ. Nhiệm vụ hoàn thành không bị tính là đang quá hạn.
- Thực học chỉ cộng bản ghi tập trung Completed có mốc kết thúc trong `[tu, den)`, gắn nhiệm vụ học tập còn tồn tại ngoài thùng rác. Phiên tự do, nhiệm vụ cá nhân và ghi chú không được đưa vào tổng hợp admin. Đây là tổng toàn bộ bản ghi kết thúc trong khoảng, không chia nhỏ bản ghi qua từng ngày.
- Thời gian dự kiến của lịch không phải thực học. Nhập tay được tổng hợp riêng. Hoàn thành phiên không tự đồng nghĩa nhiệm vụ xong.
- Hai API thống kê và bảng gọi riêng nên có thể phản ánh hai thời điểm đọc hơi khác nhau nếu sinh viên sửa đúng lúc; không cam kết ảnh chụp chung xuyên nhiều HTTP request.

## Code và kiểm chứng

- [Controller](../../backend/StudyPlanner.Api/Controllers/Administration/TheoDoiHocTapController.cs), [service và DTO](../../backend/StudyPlanner.Infrastructure/Administration/TheoDoiHocTapService.cs), [kiểm thử HTTP/PostgreSQL](../../backend/tests/StudyPlanner.LearningAdminChecks/Program.cs).
- Frontend `src/admin/AdminLearningPage.tsx`, `learning.css`; menu và đường dẫn trong AdminLayout/AdminRoutes; liên kết từ StudentDetails. Có trạng thái chờ, lỗi/thử lại và rỗng thực tế; không sinh số liệu giả khi API lỗi.
- Chạy `scripts/test-admin-learning.ps1`: cụm PostgreSQL tạm chỉ nghe localhost, database tên ngẫu nhiên, tự dọn database và dừng cụm sau kiểm thử. Không đọc cấu hình DB ứng dụng.
- Kết quả chi tiết và giới hạn tại [bản ghi 18/11](../history/2026-11-18-admin-theo-doi-hoc-tap-sinh-vien.md).

Không thêm bảng hoặc migration cho phần admin theo dõi. Nhánh này dùng model đã có trên HEAD, không phụ thuộc chức năng sửa thời gian đang làm ở nhánh khác. Không tự restart API hoặc thay đổi DB ứng dụng trong tác vụ này.
