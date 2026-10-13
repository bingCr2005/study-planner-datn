    # Module 01 — Tài khoản và hồ sơ

    Trạng thái: đặc tả dự thảo, chưa có code nghiệp vụ. Nguồn: [quy ước chung](00-quy-uoc-va-quyet-dinh.md), [ERD](../erd.md), [API — Auth/Profile](../THIET-KE-API-SWAGGER.md). Giai đoạn 3 của kế hoạch.

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

    1. Chuẩn hóa email; kiểm tra mật khẩu và IsActive.
    2. Trả token, expiresAt và hồ sơ theo đề xuất API; sai thông tin trả lỗi chung không tiết lộ email có tồn tại.
    3. Request bảo vệ kiểm tra phiên và trạng thái tài khoản. Token hết hạn thì giao diện đưa người dùng về đăng nhập.
    4. Đăng xuất bản đầu xóa token ở client; không khẳng định token đã bị thu hồi trên server. Lưu token trong bộ nhớ theo đề xuất hiện tại nên tải lại có thể cần đăng nhập lại.

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
