    # Module 01 — Tài khoản và hồ sơ

    Trạng thái: đã có code tài khoản và hồ sơ. Mục đăng nhập bên dưới được đối chiếu với code ngày 2026-11-15; các mục khác vẫn giữ đặc tả ban đầu, có những chi tiết cũ về route, trường hồ sơ và lưu phiên, không dùng để kết luận trạng thái triển khai. Nguồn: [quy ước chung](00-quy-uoc-va-quyet-dinh.md), [ERD](../erd.md), [API — Auth/Profile](../THIET-KE-API-SWAGGER.md).

    ## 1. Mục tiêu và phạm vi

    Khách tạo tài khoản; sinh viên đăng nhập và sửa hồ sơ của mình. Bản đầu gồm đăng ký, đăng nhập, xem/sửa hồ sơ, đổi mật khẩu và đăng xuất phía client. Chưa có xác minh email, quên mật khẩu, đổi email, refresh token hoặc đăng nhập ngoài.

    Màn hình: đăng ký, đăng nhập, hồ sơ/cài đặt. Không buộc chọn trường/lớp hoặc nhập mã sinh viên để bắt đầu sử dụng.

    ## 2. Dữ liệu và quyền

    User là dữ liệu chính; StudentClass tùy chọn cho hồ sơ. Notification bị ảnh hưởng khi thay đổi giờ nhắc mặc định. Chỉ tài khoản hiện tại sửa hồ sơ của mình; form không nhận Role/IsActive/PasswordHash làm trường cập nhật.

    Mặc định theo ERD: Role = Student, IsActive = true, TimeZone = Asia/Ho_Chi_Minh, ReminderHours = 24. Chuẩn hóa email thống nhất và đặt unique trên NormalizedEmail; StudentCode trắng thành NULL, unique theo giả định hiện tại Q01.

    ## 3. Luồng chính

    ### Đăng ký

    1. Khách nhập fullName, email, password; xác nhận mật khẩu ở giao diện.
    2. Server kiểm tra dữ liệu, chuẩn hóa email và băm mật khẩu bằng thư viện đã chọn.
    3. Tạo User với giá trị mặc định, xử lý cả lỗi unique khi hai request trùng đồng thời.
    4. Trả 201 và hồ sơ an toàn; không trả hash và không tự cấp quyền Admin. Người dùng chuyển sang đăng nhập.

    ### Đăng nhập và sử dụng phiên

    1. Frontend nhận email hoặc MSSV tại /login. Cả hai ô bắt buộc; tài khoản tối đa 320 ký tự, bỏ khoảng trắng đầu/cuối. Email kiểm tra định dạng và nhận dấu chấm, gạch dưới, gạch ngang, dấu cộng; MSSV nhận chữ, số và dấu _. Tài khoản không nhận dấu nháy hoặc khoảng trắng ở giữa. Chưa có username riêng nên không ép email/MSSV vào giới hạn username 5–30 ký tự. Mật khẩu đăng nhập chỉ bắt buộc và tối đa 128 ký tự; không bị trim hoặc tự lọc ký tự, cho phép dấu nháy, ký tự đặc biệt và khoảng trắng.
    2. AuthPage hiển thị lỗi dưới từng ô khi rời ô hoặc submit; khi submit focus ô lỗi đầu tiên và không gửi dữ liệu lỗi. AuthProvider và loginAccount cũng kiểm tra trước khi tiếp nhận phiên hoặc gửi POST /api/v1/auth/login. Backend kiểm tra DangNhapRequest tại HTTP và Application, trả 400/VALIDATION_ERROR khi vi phạm; cả hai trường cùng sai đều có lỗi. FE ánh xạ errors.TaiKhoan/MatKhau từ API vào đúng ô.
    3. Dữ liệu hợp lệ được đối chiếu email không phân biệt hoa/thường hoặc MSSV giữ số 0 đầu và hoa/thường; không chọn tài khoản khi có nhiều kết quả khớp. Kiểm tra mật khẩu băm và DangHoatDong; sai thông tin hoặc bị khóa trả 401/INVALID_CREDENTIALS với thông báo Sai tài khoản hoặc mật khẩu. FE chuẩn hóa lỗi 401 của riêng POST đăng nhập để backend cũ cũng có thông báo rõ; lỗi mạng, khóa có mã ACCOUNT_DISABLED và lỗi xác minh hồ sơ sau đăng nhập vẫn dùng thông báo riêng.
    4. API trả tokenTruyCap, hetHanLuc và nguoiDung. Frontend xác minh qua GET /api/v1/users/me rồi lưu JWT/hạn phiên trong sessionStorage; admin vào /admin/overview, sinh viên vào /today. Không lưu mật khẩu.
    5. Mật khẩu mới khi đăng ký/đổi mật khẩu bắt buộc 8–128 ký tự, không chỉ gồm khoảng trắng và xác nhận khớp ở FE. Không ép đủ nhóm ký tự; hỗ trợ chữ thường/hoa, số, dấu nháy, các ký tự đặc biệt và khoảng trắng nguyên văn. Tài khoản cũ dùng mật khẩu ngắn vẫn tới được bước xác thực. Các ô mật khẩu ở đăng nhập/đăng ký/đổi mật khẩu mặc định ẩn và có nút Hiện/Ẩn độc lập, không submit form. Không thay đổi hash hoặc dữ liệu tài khoản đã lưu.
    6. Request bảo vệ kiểm tra phiên và trạng thái tài khoản. SSE và API phát hiện tài khoản bị khóa/hết phiên; đăng xuất xóa credential phía client. Xem bản ghi 2026-11-12-thoat-phien-khi-khoa.md về giới hạn thu hồi token.

    ### Sửa hồ sơ và đổi mật khẩu

    PATCH hồ sơ: trường vắng giữ nguyên; studentCode/classId/major có thể NULL để bỏ; fullName/timeZone/reminderHours không được NULL. ClassId nếu có phải tồn tại, múi giờ phải được hệ thống hỗ trợ.

    Đổi giờ nhắc mặc định phải phối hợp [Thông báo](06-thong-bao-nhac-han.md) để cập nhật những task dùng mặc định trong cơ chế nhất quán. Đổi múi giờ cần giải thích ảnh hưởng lịch/ngày hiển thị; không tự dịch chuyển thời điểm deadline đã lưu.

    Đổi mật khẩu yêu cầu mật khẩu hiện tại, kiểm tra mật khẩu mới và thay hash. Token cũ có thể còn hiệu lực nếu chưa bổ sung cơ chế thu hồi Q03; cần nêu đúng hành vi cho người dùng.

    ## 4. API và kết quả

    | Method | Route | Kết quả |
    | --- | --- | --- |
    | POST | /api/auth/register | 201 hồ sơ |
    | POST | /api/auth/login | 200 phiên và hồ sơ |
    | GET | /api/users/me | 200 hồ sơ hiện tại |
    | PATCH | /api/users/me | 200 hồ sơ đã cập nhật |
    | POST | /api/users/me/change-password | 204 |

    Mật khẩu/hash/token không đưa vào log hoặc ví dụ dữ liệu thật. Giới hạn tần suất login/register theo cấu hình đã chốt, vượt ngưỡng trả 429. Email/mã sinh viên trùng theo hợp đồng trả 409; classId không hợp lệ không được lưu thành liên kết hỏng.

    ## 5. Tình huống nghiệm thu

    | Tình huống | Kết quả kỳ vọng |
    | --- | --- |
    | Đăng ký không mã SV/lớp | Thành công với trường tùy chọn NULL |
    | Email khác hoa/thường nhưng chuẩn hóa trùng | Không tạo tài khoản trùng, kể cả request đồng thời |
    | Gửi thêm Role = Admin | Không tạo tài khoản Admin |
    | Sai mật khẩu/thiếu token/token hết hạn | Bị từ chối, không lộ thông tin xác thực |
    | PATCH bỏ trống trường so với gửi NULL | Giữ hoặc bỏ đúng từng trường theo hợp đồng |
    | User A gọi me; User B gọi me | Mỗi người chỉ nhận/sửa hồ sơ mình |
    | Tài khoản bị khóa sau login | Request bảo vệ tiếp theo bị từ chối |
    | Đổi giờ nhắc, cập nhật lời nhắc lỗi | Không báo thành công khi dữ liệu liên quan chưa nhất quán |

    ## 6. Điểm cần chốt và điều kiện hoàn thành

    Chốt Q01–Q04, chính sách mật khẩu, hạn token và cơ chế lưu phiên trước khi code phần phụ thuộc. Hồ sơ có thể bắt đầu với ClassId NULL; không cần chờ toàn bộ CRUD quản trị.

    Hoàn thành khi luồng tài khoản chạy qua Swagger và UI, có kiểm thử xác thực/quyền, dữ liệu không lộ hash, lỗi được mô tả đúng. Bảo vệ hồ sơ chưa chứng minh mọi API học tập an toàn; kiểm tra quyền sở hữu tiếp tục đi cùng từng module.
