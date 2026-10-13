# Quy trình làm việc với dự án

- Trước mỗi tác vụ code, đọc `docs/history/README.md`, các bản ghi liên quan và code hiện tại. Không chỉ dựa vào lịch sử hội thoại hoặc tài liệu.
- Đối chiếu tài liệu với code; nếu khác nhau, nêu rõ điểm khác và cập nhật tài liệu theo trạng thái đã xác minh.
- Sau mỗi thay đổi có ý nghĩa, thêm một bản ghi trong `docs/history/` theo `TEMPLATE.md`. Ghi yêu cầu, lý do, file thay đổi, logic trước/sau, luồng xử lý, kiểm chứng và việc còn lại.
- Giải thích đủ để người đọc hiểu vì sao sửa và lần theo code được. Không chỉ liệt kê tên file hoặc nói chung chung là đã hoàn thành.
- Đọc lại diff và code liên quan trước khi bàn giao; chạy kiểm tra phù hợp và ghi đúng kết quả. Nếu chưa chạy, ghi rõ chưa chạy và lý do.
- Không tự ghi rằng người dùng đã đọc, hiểu hoặc duyệt code. Phần người dùng đọc và đối chiếu code phải do người dùng xác nhận.
- Không tự commit, push hoặc triển khai chỉ vì đã sinh code; chỉ thực hiện khi người dùng yêu cầu hoặc đã cho phép trong phiên.
- Không ghi mật khẩu, token, connection string hoặc thông tin bí mật vào tài liệu.
