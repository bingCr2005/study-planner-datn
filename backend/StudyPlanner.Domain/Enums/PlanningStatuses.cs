namespace StudyPlanner.Domain.Enums;

// Phiên đã qua giờ không tự đồng nghĩa sinh viên đã học xong.
public enum StudySessionStatus
{
    Planned, // Đã xếp vào lịch, chưa bắt đầu.
    InProgress, // Đang học; thời gian thực tế lấy từ FocusInterval.
    Completed, // Đã kết thúc phiên, không tự đặt nhiệm vụ thành 100%.
    Missed, // Bỏ lỡ theo thao tác/chính sách của dịch vụ sau này.
    Cancelled // Đã hủy, không chiếm khung giờ trong lịch gợi ý.
}
public enum ReminderStatus { Pending, Delivered, Cancelled, Failed }
