# Quy ước chung và quyết định cần chốt

Trạng thái: dự thảo ngày 2026-10-13. Đọc cùng [mục lục module](README.md), [ERD](../erd.md), [API](../THIET-KE-API-SWAGGER.md) và [đặc tả bản đầu](../DAC-TA-NGHIEP-VU-BAN-DAU.md). Ngoại trừ phạm vi gợi ý đã được ghi nhận trong đặc tả bản đầu, các phương án còn lại không đồng nghĩa người dùng đã duyệt.

## 1. Từ ngữ dùng thống nhất

| Khái niệm | Ý nghĩa |
| --- | --- |
| Nhiệm vụ lá | Không có con; được nhập tiến độ và thời lượng ước lượng |
| Nhiệm vụ cha | Có con; tiến độ được tính từ con trực tiếp |
| Hạn nhập | DueDate được lưu trên chính nhiệm vụ, có thể NULL |
| Hạn hiệu lực | Hạn nhập nếu có, nếu không thì hạn của tổ tiên gần nhất có hạn |
| Quá hạn | Hạn hiệu lực < thời điểm xét và chưa Completed; không phải trạng thái thứ tư |
| Lịch học | ClassSession lặp hằng tuần trong học kỳ |
| Gợi ý | Xếp thứ tự việc; chưa tự ghi các buổi tự học |
| Thời lượng còn lại | Ước lượng từ EstimatedMinutes và Progress, không phải số giờ thực học |

## 2. Quyền và validation

- Khách chỉ dùng các chức năng công khai như đăng ký/đăng nhập. Student quản lý dữ liệu của mình. Admin quản lý danh mục/trạng thái tài khoản, không mặc định được đọc mọi nhiệm vụ cá nhân.
- Lấy danh tính từ phiên xác thực. Kiểm tra cả tài nguyên đích và ID liên quan như CourseId, ParentTaskId, SemesterId; không tin UserId client gửi.
- Theo hợp đồng API: 400 đầu vào không hợp lệ; 401 thiếu/sai/hết hạn xác thực; 403 thiếu role; 404 tài nguyên cá nhân không tồn tại/không thuộc người gọi; 409 xung đột nghiệp vụ/unique/sửa bản cũ khi cơ chế đó được triển khai.
- Giới hạn trường, enum, nullability theo ERD/API. Trường vắng trong PATCH khác gửi NULL; không tự cho client đặt trường do server quản lý.
- Mọi trường hợp thất bại phải để dữ liệu nhất quán; lỗi 500 không trả stack trace hoặc bí mật.

## 3. Thời gian, giao dịch và dữ liệu

- Thời điểm API có offset, lưu UTC; lịch học dùng ngày/giờ địa phương theo User.TimeZone. Khoảng truy vấn [from, to) gồm đầu và loại cuối.
- Các module dùng cùng quy tắc tính hạn hiệu lực và cùng thời điểm xét trong một lần tính kết quả, tránh mỗi phần tự hiểu hạn khác nhau.
- Thay đổi nhiệm vụ, tổ tiên, lịch sử và lời nhắc liên quan nằm trong cùng giao dịch. Application điều phối; Infrastructure thực thi bằng cùng phạm vi DbContext/giao dịch.
- Giao dịch không tự chống mọi mất cập nhật. Chọn cơ chế khóa/phiên bản và thứ tự khóa thống nhất trước khi triển khai cây nhiệm vụ/worker.
- Dashboard, Calendar và Planning là kết quả đọc/tính từ dữ liệu hiện có; không mặc định thêm bảng riêng hoặc cột EffectiveDueDate.

## 4. Các quyết định còn mở và phạm vi đã ghi nhận

| Mã | Vấn đề | Phương án hiện có / cần quyết định | Cần chốt trước |
| --- | --- | --- | --- |
| Q01 | Mã sinh viên | Unique khi có giá trị trong đơn vị triển khai; đa trường trùng mã cần đổi mô hình | Migration/User và hồ sơ |
| Q02 | Xác thực và mật khẩu | Đề xuất access token ngắn hạn, chưa refresh; cần chốt thư viện, chính sách mật khẩu, thời hạn và lưu phiên | Auth |
| Q03 | Thu hồi phiên | Đăng xuất client chưa thu hồi token; nếu cần vô hiệu hóa ngay khi đổi mật khẩu phải bổ sung dữ liệu/cơ chế | Auth/schema phụ thuộc |
| Q04 | Sửa đồng thời | Chưa chọn khóa/phiên bản, header và phản hồi; không dùng UpdatedAt như giải pháp hoàn chỉnh mặc định | API sửa và giao dịch liên quan |
| Q05 | Giới hạn cây | Chưa chốt độ sâu/số node/query budget và hợp đồng lỗi khi vượt giới hạn | API cây/chuyển cha |
| Q06 | Nhắc cha/con và tắt nhắc | ERD nhắc mọi nhiệm vụ đủ điều kiện, chưa có bật/tắt; thay đổi cần cập nhật schema/quy tắc | Schema lời nhắc và tạo task |
| Q07 | Biên và thử lại lời nhắc | Chốt cách xử lý đúng hạn khi giờ nhắc = 0, mốc đã quá hạn, khoảng retry và tranh chấp | Worker |
| Q08 | Lưu lịch sử | Chưa chốt thời hạn giữ và quyền quản trị với lịch sử mất liên kết người dùng | Vận hành/xóa tài khoản |
| Q09 | Chuyển cây khác môn | API đề xuất đồng bộ cây chuyển về môn của cha mới; cần chốt hoặc chọn trả xung đột | Chuyển cha/đổi môn |
| Q10 | Lịch và ngày thống kê | Cảnh báo trùng nhưng cho lưu; chốt xử lý giờ địa phương không tồn tại/trùng ở múi giờ có DST | Calendar/múi giờ hỗ trợ |
| Q11 | Gợi ý | Baseline theo hạn đã đề xuất; công thức nâng cao, bộ dữ liệu và tiêu chí đánh giá chưa chốt | Thuật toán Planning |
| Q12 | Ranh giới phần tối ưu | Đặc tả bản đầu đã ghi nhận chọn gợi ý có giải thích, chưa tự xếp giờ; ERD chưa có lịch rảnh/buổi tự học/thời gian thực học | Giữ phạm vi bản đầu; chỉ thiết kế thêm khi có yêu cầu mở rộng |

Các mã chỉ dùng để trao đổi và theo dõi, chưa phải issue đã tạo. Q01–Q11 cần hoàn thiện theo phần phụ thuộc; Q12 nhắc phạm vi đã ghi nhận, không yêu cầu người dùng chọn lại. Không tự đánh dấu những phần còn lại đã chốt. Nếu một quyết định làm thay đổi ERD/API, cập nhật tài liệu đó và thêm lịch sử trước hoặc cùng lần sửa code.

## 5. Cách dùng tiêu chí nghiệm thu

Các bảng trong module mô tả đầu vào/thao tác và kết quả kỳ vọng, chưa phải báo cáo test. Khi kiểm chứng, ghi môi trường, dữ liệu thử, kết quả thật và giới hạn vào docs/history. Với kiểm tra quyền, luôn có hai tài khoản độc lập; với thời gian, cố định thời điểm xét; với giao dịch, kiểm tra cả rollback khi một bước lỗi.
