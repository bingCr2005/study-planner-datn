# Module 05 — Lịch học và tổng quan

Trạng thái xác minh ngày 2026-11-06. BE và FE React đã có lịch lớp theo môn, giờ rảnh, lịch ngày/tuần/tháng/danh sách, gợi ý và áp dụng lịch tự học, quản lý phiên tự học. Calendar trả năm loại mục gồm lịch lớp, sự kiện, hạn nhiệm vụ, giờ rảnh và phiên tự học. Trục giờ, chia cột lịch trùng và các luồng tương ứng trong `frontend-preview` được đối chiếu trong [bản ghi chuyển đổi đầy đủ](../history/2026-11-06-chuyen-day-du-frontend-preview-va-sua-bo-loc.md).

Cập nhật ngày 2026-11-16 về [dải Hạn nộp và hạn kế thừa](../history/2026-11-16-lam-ro-han-trong-lich-tuan.md).

Các phần bên dưới giữ nội dung đặc tả ban đầu để đối chiếu. Dashboard là phần việc riêng của team, giao diện và nguồn dữ liệu của trang đó cần được kiểm chứng trong lượt triển khai tương ứng.

## 1. Mục tiêu và phạm vi

Sinh viên xem hôm nay học gì, các hạn sắp đến và tiến độ công việc. Màn hình gồm quản lý buổi học của môn, lịch ngày/tuần/tháng và Dashboard.

ClassSession là mẫu lặp theo tuần trong học kỳ, chưa phải buổi tự học được xếp từ TaskItem. Calendar/Dashboard là kết quả đọc/tính, không cần bảng mới mặc định. Chưa có nghỉ lễ, học bù, tuần luân phiên, ngoại lệ một ngày hoặc buổi học qua nửa đêm.

## 2. Quy tắc và dữ liệu

- ClassSession thuộc Course; quyền lấy qua Course → Semester → User. DayOfWeek 1–7, thứ Hai đến Chủ nhật; EndTime > StartTime; Room tùy chọn.
- Mẫu lặp có hiệu lực từ StartDate đến EndDate của Semester. Sinh các lần xuất hiện khi truy vấn, không lưu sẵn từng ngày học.
- Dùng User.TimeZone để diễn giải giờ học và ngày hiển thị; tuần bắt đầu thứ Hai. Khoảng request [from, to) và tối đa 93 ngày theo API.
- Phương án hiện tại cảnh báo trùng lịch nhưng cho lưu. Đổi biên học kỳ thay phạm vi lịch, không tự đổi deadline nhiệm vụ.
- Calendar có ClassSessionOccurrence (start/end) và TaskDeadline (dueAt). Không tạo một buổi học dài EstimatedMinutes từ deadline.
- Task không có hạn hiệu lực không tạo mốc hạn. Màn Thời khóa biểu hiển thị hạn theo nhiệm vụ được đặt hạn trực tiếp. Nếu con kế thừa hạn của cha, lịch chỉ hiện mốc của cha. Con được đặt hạn riêng có mốc riêng. Quy tắc hạn hiệu lực để theo dõi từng nhiệm vụ được mô tả trong [Nhiệm vụ](03-nhiem-vu-va-tien-do.md).

## 3. Luồng sử dụng

Trên màn Thời khóa biểu, **Ẩn menu** và **Hiện menu** cho phép mở rộng lịch hoặc mở lại cột điều hướng. Chế độ xem, ngày và bộ lọc vẫn giữ khi đổi độ rộng. Trên desktop, thanh cuộn bên phải dùng chung cho phần điều khiển và lưới lịch. Cuộn xuống để khu vực lịch chiếm gần hết khung. Hàng ngày và hạn của lịch giờ tiếp tục ghim ở đầu vùng xem.

1. Mở môn, nhập thứ/giờ/phòng → xác minh chủ sở hữu và dữ liệu → lưu mẫu, kèm warning nếu trùng theo phương án hiện tại.
2. Mở lịch → server xác định khoảng/múi giờ → tải mẫu thuộc người dùng → sinh lần xuất hiện trong học kỳ và khoảng request → gộp deadline → trả DTO có loại/nguồn sự kiện.
3. Bấm tên trên thẻ để mở chi tiết. Thẻ hạn và sự kiện của nhiệm vụ đơn lẻ có ô tích để hoàn thành hoặc mở lại ngay trong lịch. Nhiệm vụ có bước nhỏ hoặc checklist cần cập nhật trong chi tiết. Việc đã hoàn thành và phiên tự học đã xong có màu nhạt hơn.
4. Trong lịch Ngày, Tuần hoặc 3 ngày, dải Hạn nộp được ghim ở đầu lưới giờ. Dải thường hiện tối đa hai thẻ mỗi ngày và **Xem thêm N hạn**. Bấm **Thu gọn hạn** để chỉ giữ nút đếm hạn bên dưới ngày, rồi bấm số hạn nếu cần mở danh sách. Chế độ Ngày đặt hai thẻ cạnh nhau khi đủ chiều rộng.
5. Mở Dashboard → lấy ngày địa phương và phạm vi học kỳ tùy chọn → tính số liệu từ nhiệm vụ lá → trả scope/generatedAt cùng lịch ngày và tối đa 10 việc cần chú ý.

## 4. Định nghĩa số liệu Dashboard

| Trường | Cách hiểu theo thiết kế API |
| --- | --- |
| totalLeaves | Tất cả nhiệm vụ lá trong phạm vi đã chọn |
| completedLeaves | Lá Completed trong cùng phạm vi |
| overdueLeaves | Lá chưa Completed, hạn hiệu lực < generatedAt |
| dueTodayLeaves | Lá có hạn hiệu lực rơi vào ngày được chọn theo TimeZone |
| undatedLeaves | Lá không có hạn hiệu lực trong phạm vi |

Total/completed/undated không bị hiểu là chỉ riêng ngày đang xem. Tỷ lệ completed/total phản ánh số nhiệm vụ, không phải tỷ lệ thời gian học. Trường hợp total = 0 cần chốt cách trả/hiển thị (đề xuất hiển thị chưa có dữ liệu), không chia cho 0. Các nhóm có thể giao nhau, không cộng chúng thành một tổng mới.

## 5. API và ngoại lệ

- GET/POST /api/courses/{id}/class-sessions.
- PUT/DELETE /api/class-sessions/{id}.
- `GET /api/v1/calendar?tu=...&den=...&maHocKy=...&gomHanKeThua=true` trả hạn theo nhiệm vụ đặt hạn trực tiếp. Bên gọi bỏ tham số `gomHanKeThua` tiếp tục nhận hạn của nhiệm vụ lá theo mặc định, hoặc cả hạn của nhiệm vụ cha nếu bật `hienHanCha`.
- GET /api/dashboard?date=...&semesterId=... .

Route/DTO chính thức theo tài liệu API. 400 cho giờ/khoảng truy vấn sai hoặc quá dài; 404 cho tài nguyên cá nhân không thuộc người gọi. Không trả event của người khác hoặc toàn bộ entity có navigation. Sửa/xóa buổi học chỉ tác động mẫu tương ứng, không biến thành xóa nhiệm vụ.

## 6. Tình huống nghiệm thu

| Tình huống | Kết quả kỳ vọng |
| --- | --- |
| Ngày đầu/cuối học kỳ, request [from,to) | Event đúng biên; không có buổi học ngoài học kỳ |
| Cuối tháng/năm, thứ Hai/Chủ nhật | Sinh đúng thứ/ngày, không thiếu hoặc lặp sai |
| EndTime <= StartTime; request > 93 ngày | 400, không lưu/truy vấn ngoài giới hạn |
| Hai buổi trùng | Cảnh báo, vẫn lưu theo phương án đề xuất |
| Con kế thừa hạn và cha có hạn trực tiếp | Màn Thời khóa biểu hiện một mốc mang tên cha |
| Cha và con đều đặt hạn trực tiếp | Màn Thời khóa biểu hiện mốc của cả hai |
| Một ngày có hơn hai hạn | Mở danh sách đầy đủ ngay trên dải Hạn nộp, giữ nguyên lịch tuần |
| Cha và hai lá | Counts chỉ tính lá, số event không đại diện khối lượng |
| Đổi múi giờ, lọc học kỳ, hai tài khoản | Ngày/phạm vi đúng, không lộ dữ liệu tài khoản khác |
| Không có nhiệm vụ | Counts bằng 0, không chia cho 0 hoặc hiển thị hoàn thành giả |

## 7. Điểm cần chốt và điều kiện hoàn thành

Chốt Q10 cho các múi giờ hỗ trợ, quy tắc warning trùng và response khi tổng bằng 0. Hoàn thành khi dữ liệu lịch/tổng quan khớp bộ mẫu có kết quả kỳ vọng và UI giải thích rõ ngày, phạm vi, loại sự kiện.
