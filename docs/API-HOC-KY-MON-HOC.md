# API học kỳ và môn học

Các route dưới đây đã được triển khai trong `StudyPlanner.Api/Controllers/SemestersController.cs`, `CoursesController.cs`; logic nằm ở `StudyPlanner.Application/Academic/AcademicService.cs` và DTO ở `Contracts.cs`. Base URL `/api/v1`, tất cả yêu cầu Bearer JWT. ID/chủ sở hữu lấy từ phiên; tài nguyên không tồn tại hoặc thuộc người khác đều trả 404. Không cần migration mới.

| Method | Route | Kết quả |
| --- | --- | --- |
| GET/POST | `/semesters` | 200 phân trang / 201 và Location |
| GET/PUT/DELETE | `/semesters/{id}` | 200 / 200 / 204 không body |
| GET | `/semesters/{id}/deletion-preview` | 200 ảnh hưởng xóa |
| GET/POST | `/semesters/{id}/courses` | 200 phân trang / 201 và Location |
| GET/PUT/DELETE | `/courses/{id}` | 200 / 200 / 204 không body |
| GET | `/courses/{id}/deletion-preview` | 200 ảnh hưởng xóa |

Query danh sách: `Trang` mặc định 1, `KichThuocTrang` mặc định 20 (1–100), `TuKhoa` tùy chọn (tối đa 200). Phản hồi `{ danhSach, trang, kichThuocTrang, tongSo }`, sắp xếp ổn định theo `ma` tăng dần. Từ khóa học kỳ tìm tên; môn tìm tên hoặc ký hiệu. Offset vượt giới hạn int trả 400.

POST/PUT học kỳ: `{ ten, ngayBatDau, ngayKetThuc }`. Tên bắt buộc tối đa 200; ngày bắt buộc dạng `yyyy-MM-dd`; kết thúc >= bắt đầu. Cho phép tên và ngày trùng. Response thêm `ma`, `ngayTao`, `ngayCapNhat` UTC.

POST/PUT môn: `{ ten, kyHieu, soTinChi, giangVien, mauSac }`. Tên bắt buộc tối đa 200; ký hiệu tùy chọn tối đa 50; giảng viên tùy chọn tối đa 200; tín chỉ nếu có phải là int > 0; màu nếu có đúng `#RRGGBB`, trả về viết hoa. Chuỗi tùy chọn được trim, trắng thành null. Response thêm `ma`, `maHocKy`, `ngayTao`, `ngayCapNhat`. POST lấy học kỳ từ route; PUT giữ học kỳ đã tạo. Gửi `maHocKy`, chủ sở hữu hay field ngoài DTO trong body trả 400.

Preview: `{ soMonHoc, soBuoiHoc, soNhiemVuBoLienKet }`. `soBuoiHoc` đếm mẫu lịch, không phải số lần xuất hiện. Preview môn trả `soMonHoc = 1`. Preview học kỳ đếm cả việc chung kỳ không có môn. Preview không khóa dữ liệu và không phải token cấp phép cho DELETE: máy chủ kiểm tra quyền lại trong giao dịch; dữ liệu có thể đổi giữa hai lần gọi.

Xóa môn giữ nhiệm vụ và bỏ `maMonHoc`, giữ `maHocKy`; xóa học kỳ bỏ cả hai liên kết, gồm việc chung kỳ. Cây/cha con, chủ sở hữu, hạn và tiến độ không đổi. Mỗi nhiệm vụ thay đổi có `TaskHistory.Updated` với trước/sau các liên kết; lịch và môn/học kỳ bị xóa trong cùng giao dịch Serializable. Lỗi ghi lịch sử hoặc xóa rollback toàn bộ. Xung đột serialization/FK theo cơ chế hiện có trả 409. Không tự tính lại tiến độ hoặc đổi hạn khi thu hẹp ngày học kỳ.

Nhiệm vụ/lịch học chưa có API ghi trong phạm vi này; test tạo fixture trực tiếp trong DB kiểm thử để kiểm chứng xóa xuyên module. Không suy ra worker thông báo hoặc API nhiệm vụ đã hoàn thành.

Kiểm thử trên PostgreSQL cục bộ tạm:

```powershell
dotnet run --project backend/tests/StudyPlanner.AcademicChecks
```

Đặt `STUDYPLANNER_TEST_POSTGRES` trong môi trường trỏ localhost và database quản trị `postgres`. Bộ kiểm thử tạo database tên ngẫu nhiên, chạy migration rồi dọn đúng database đó trong `finally`; không đọc cấu hình DB ứng dụng. Có thể đặt `STUDYPLANNER_FE_ROOT` trỏ repo FE để chạy thêm `tests/academic-live.mjs` qua Vite proxy và JWT thật. `scripts/test-administration.ps1` cũng chạy nhóm AcademicChecks cùng Auth/Administration khi có Docker.
