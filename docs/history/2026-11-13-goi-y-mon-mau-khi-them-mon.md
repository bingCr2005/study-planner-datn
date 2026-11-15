# 2026-11-13 — Gợi ý môn mẫu trong ô thêm môn học

## Yêu cầu và mục đích

- Người dùng muốn đưa môn mẫu vào form thêm môn: khi bấm ô nhập Tên môn học thì hiện gợi ý.
- Đã đọc README/TEMPLATE lịch sử, [triển khai danh mục](2026-11-13-quan-ly-du-lieu-nen-tang.md), [cập nhật database](2026-11-13-cap-nhat-database-danh-muc-mau.md), module 09, SubjectForm/SubjectEditor/CreateSubjectPage, API danh mục và luồng lưu môn cá nhân.
- Tiếp tục nhánh `feat/quan-ly-du-lieu-nen-tang` của hai repo vì đây là phần nối tiếp cùng chức năng. Giữ các thay đổi admin/Tiến độ có sẵn, không commit/push.

## Code và logic thay đổi

- Trước: chỉ admin đọc được danh mục mẫu; form thêm môn là các ô tự nhập, mã/tín chỉ không được điền từ mẫu.
- BE `Controllers/DanhMucMauController.cs`: thêm GET `/api/v1/catalog/courses`, yêu cầu đăng nhập, cho phép sinh viên đọc. Dùng service danh mục đã có, tìm tên/mã và phân trang 20 dòng qua `tuKhoa`, `trang`. DTO chỉ gồm mã mẫu, mã/tên kỳ mẫu, ký hiệu, tên môn, tín chỉ; không trả phiên bản quản trị. Controller chỉ có GET; quyền đọc/ghi tại `/admin/catalog` vẫn giữ nguyên Admin.
- Không thêm migration hoặc sửa schema/database trong lượt này. Dùng hai bảng mẫu đã được cập nhật ở lượt trước.
- FE `src/api/courseSuggestions.ts`: đọc endpoint riêng bằng Bearer JWT, kiểm tra hợp đồng DTO/phân trang, không gọi API quản trị thay sinh viên hoặc dựng danh sách giả.
- FE `src/components/CourseNameInput.tsx`, `courseSuggestions.css`: combobox mở khi focus/bấm ô tên, tìm tên/mã sau debounce 200 ms, hủy request khi đổi từ khóa/đóng hoặc rời form, loại phản hồi lỗi thời. Có chọn chuột, phím lên/xuống + Enter, Escape và đóng khi rời vùng gợi ý; có loading, empty, retry và Xem thêm. Lỗi nguồn gợi ý không chặn tự nhập/lưu môn. Demo/mock không gọi nguồn danh mục thật.
- FE `SubjectForm.tsx`: chỉ form tạo mới dùng gợi ý; mã/tín chỉ đổi sang controlled state để điền từ mẫu. Chọn mẫu điền tên, mã môn, tín chỉ; không đổi học kỳ cá nhân, giảng viên hoặc màu. Người dùng tiếp tục chỉnh các trường này. Form sửa môn giữ thông tin hiện tại và không mở gợi ý.
- Luồng: mở form và focus tên → GET danh mục mẫu dùng JWT → chọn môn → điền giá trị vào form → người dùng kiểm tra/chỉnh → bấm Tạo môn học → gọi `saveSubject`/API môn cá nhân hiện có. Chọn mẫu chưa ghi database. Không gửi ID mẫu, không tạo liên kết FK hay tự đồng bộ môn cá nhân với mẫu sau này.
- Không suy mã học kỳ mẫu từ mã học kỳ cá nhân hoặc so tên học kỳ để khóa gợi ý. Danh sách lấy từ mọi học kỳ mẫu; thông tin kỳ mẫu chỉ giúp phân biệt lựa chọn.
- `StudyPlanner.CatalogChecks/Program.cs` bổ sung quyền đọc sinh viên, anonymous 401, chỉ GET, tìm kiếm/tín chỉ và trang hai. FE bổ sung `tests/course-suggestions.test.mjs`, `browser-course-suggestions.mjs` và chế độ `--course-suggestions` trong harness auth; fixture tách biệt database ứng dụng.
- [Module 09](../modules/09-du-lieu-nen-tang.md) cập nhật mô tả trước đó rằng chưa có luồng chọn mẫu.

## Kiểm chứng

- Build CatalogChecks/backend theo configuration Catalog đạt, 0 cảnh báo/lỗi; build FE TypeScript/Vite đạt, có cảnh báo chunk >500 kB đã ghi từ lượt danh mục trước.
- Đã đọc lại diff controller, form, combobox và test; `git diff --check` hai repo đạt. Oxlint combobox, form, API client và hai file kiểm thử mới đạt, không cảnh báo ở lượt cuối.
- `scripts/test-catalog.ps1`: toàn bộ validate và HTTP/JWT/PostgreSQL đạt, gồm quyền chỉ đọc cho sinh viên và tìm mẫu ngoài trang đầu. Cụm/database kiểm thử tạo riêng, database được dọn và PostgreSQL do script mở đã dừng. Không dùng database ứng dụng để kiểm tra thao tác ghi.
- FE `npm.cmd test`: 151/151 đạt, gồm hợp đồng endpoint gợi ý, JWT, phân trang, hủy request, DTO sai và lỗi dịch vụ không trở thành gợi ý giả.
- Chrome `--course-suggestions`: kiểm tra focus/tìm tên-mã, đọc thêm trang, chọn chuột/bàn phím, điền tên/mã/tín chỉ, không đổi học kỳ cá nhân, chọn chưa lưu, chỉnh mã/tín chỉ rồi lưu bằng API cá nhân, tự nhập, lỗi/retry, Escape/rời ô, request cũ và mobile. Có ảnh `.backend-integration/screenshots/course-suggestions.png` của FE, đã xem trực tiếp; dữ liệu ảnh là fixture.
- Bài Chrome `--subjects` cũ đã đi qua thêm/sửa/xóa môn nhưng dừng ở kỳ vọng retry: đang chọn học kỳ II rỗng, test lại đòi ba môn học kỳ I. Khi thử chọn rõ kỳ để kiểm tra tiếp, phần workspace trong fixture lại thiếu dữ liệu múi giờ; đã hoàn nguyên thay đổi thử trong bài cũ, không sửa module nhiệm vụ/fixture ngoài phạm vi. Không ghi bài hồi quy cũ này là đạt. Bài mới bổ sung xác minh riêng sửa môn đã có và giữ mã/tín chỉ.
- Kiểm tra runtime trước cập nhật cho thấy backend localhost:5059 chưa có route gợi ý. Đã được cấp quyền khởi động lại đúng API của dự án sang build Catalog đã kiểm thử, môi trường Development, cổng 5059. Lần đầu script không xác định được socket nên dừng trước khi chạm tiến trình; sau đó xác minh đúng một executable API thuộc đường dẫn dự án rồi khởi động lại. Không dừng các tiến trình dotnet khác. Output runtime nằm trong thư mục ignored `backend/obj/course-suggestions-live/`.
- Sau khởi động lại: Swagger đã có `/api/v1/catalog/courses`; GET không JWT trả 401 như thiết kế. Không đăng nhập thay người dùng hoặc tạo môn vào database thật. Chưa xác minh trực tiếp trên phiên trình duyệt đăng nhập của người dùng.

## Việc tiếp theo

- Người dùng F5 frontend, mở Thêm môn học rồi bấm ô Tên môn học để dùng nguồn mẫu hiện có. Không cần migration mới.
- Chưa commit/push. Bản ghi này không thay thế xác nhận đọc code của người dùng.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
