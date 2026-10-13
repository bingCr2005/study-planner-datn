# Module 07 — Gợi ý công việc

Trạng thái: đặc tả dự thảo, chưa có thuật toán triển khai hoặc kết quả đánh giá. Nguồn: [quy ước chung](00-quy-uoc-va-quyet-dinh.md), [API — Planning](../THIET-KE-API-SWAGGER.md), [kế hoạch chi tiết — thuật toán](../KE-HOACH-TRIEN-KHAI-CHI-TIET.md). Giai đoạn 9.

## 1. Mục tiêu và phạm vi

Sinh viên biết nên bắt đầu nhiệm vụ nào tiếp theo và hiểu lý do. Màn hình: danh sách đề xuất có thứ tự, thời lượng ước lượng và cảnh báo thiếu dữ liệu; mở nhiệm vụ để thao tác.

Bản đầu là truy vấn xếp hạng, không sửa Priority/Status/DueDate và không tạo buổi học. Tự phân bổ thời gian cần thêm lịch rảnh, ràng buộc và mô hình buổi tự học (Q12), không suy ra từ ClassSession hiện tại.

[Đặc tả bản đầu](../DAC-TA-NGHIEP-VU-BAN-DAU.md) ghi nhận người dùng đã chọn phạm vi gợi ý có giải thích, chưa tự xếp giờ. Phạm vi đó được giữ; công thức baseline/nâng cao vẫn là đề xuất cần kiểm chứng.

## 2. Đầu vào và quy tắc

- Chỉ chọn nhiệm vụ lá chưa Completed thuộc người dùng, theo semesterId nếu có. Không cộng nhiệm vụ cha như một công việc độc lập.
- Đầu vào: hạn hiệu lực, ưu tiên, EstimatedMinutes, Progress và thời điểm xét cố định cho một lần tính.
- RemainingMinutes = ceil(EstimatedMinutes × (100 − Progress) / 100) nếu có ước lượng. Thiếu EstimatedMinutes → NULL, không coi là việc cần 0 phút.
- Progress là tự báo cáo; thời lượng còn lại là ước lượng. Hạn NULL không làm nhiệm vụ bị loại; giải thích dữ liệu thiếu.
- Deadline kế thừa tính giống bộ lọc, Calendar và Notifications.

## 3. Luồng và bản cơ sở

1. Người dùng yêu cầu gợi ý → server kiểm tra xác thực/phạm vi → tải nhiệm vụ lá.
2. Chuẩn hóa đầu vào và hạn; gọi bộ xếp hạng thuần không tự truy vấn database hoặc đọc thời gian hệ thống bên trong.
3. Baseline đề xuất: nhóm quá hạn trước, sau đó có hạn từ gần đến xa, cuối cùng không hạn; trong nhóm quá hạn cũng theo hạn tăng dần. Cùng hạn/không hạn thì High → Medium → Low, cuối cùng Id tăng dần để ổn định.
4. Lấy top limit và trả generatedAt, strategy, items, warnings. Mỗi item có rank, taskId, title, effectiveDueDate, priority, remainingMinutes, reasons, missingInputs.
5. UI giải thích lý do bằng ngôn ngữ người dùng; sửa dữ liệu rồi tính lại, không âm thầm biến kết quả thành kế hoạch đã cam kết.

### Ví dụ minh họa baseline

Thời điểm xét cố định: 2026-10-13 09:00 +07:00. Đây là dữ liệu minh họa chưa chạy trên API.

| Việc | Hạn hiệu lực | Ưu tiên | Ước lượng/tiến độ | Thứ tự kỳ vọng |
| --- | --- | --- | --- | --- |
| A | 2026-10-12 18:00 +07:00 | Low | 100 phút / 40% | 1: quá hạn, còn 60 phút ước lượng |
| B | 2026-10-13 18:00 +07:00 | High | 120 phút / 25% | 2: cùng hạn C, ưu tiên cao hơn |
| C | 2026-10-13 18:00 +07:00 | Medium | Không có thời lượng / 0% | 3: remainingMinutes NULL |
| D | Không hạn | High | 60 phút / 0% | 4: nhóm không hạn |

Baseline chưa dùng thời lượng để quyết định thứ tự; có thời lượng trong response để giải thích và làm đầu vào phương án nâng cao. Không thêm trọng số tùy ý rồi gọi kết quả là tối ưu.

## 4. API và ngoại lệ

GET /api/planning/recommendations; semesterId tùy chọn, limit mặc định 10 tối đa 50. limit là top N, không phải tổng số việc; không có việc thì items rỗng hợp lệ.

400 cho limit/query sai; 404 nếu chọn tài nguyên cá nhân không thuộc mình theo quy ước chung; 401 khi không có phiên hợp lệ. Không đưa nhiệm vụ của tài khoản khác vào lý do hoặc warnings. API là truy vấn, không tạo bảng Recommendations mặc định.

## 5. Đánh giá và tình huống nghiệm thu

| Tình huống | Kết quả kỳ vọng |
| --- | --- |
| Cùng đầu vào và thời điểm xét | Cùng thứ tự và lý do |
| Cùng hạn/ưu tiên | Id quyết định ổn định |
| Thiếu hạn hoặc thời lượng | Không crash, không coi thiếu thời lượng là 0, có giải thích |
| Nhiệm vụ cha/Completed | Không đưa vào danh sách việc độc lập |
| Đổi hạn/ưu tiên/tiến độ | Đầu vào và kết quả mới khớp quy tắc đã chọn |
| Lọc học kỳ/tài khoản | Đúng phạm vi, không lộ dữ liệu người khác |
| GET gợi ý | Không thay dữ liệu nhiệm vụ hoặc tự ghi buổi học |

Chuẩn bị bộ ví dụ kỳ vọng và so sánh phương án nâng cao với baseline trên cùng đầu vào. Nếu muốn khẳng định giảm trễ hạn, cần tiêu chí đo, giả định thời gian rảnh và dữ liệu/mô phỏng cụ thể. Tỷ lệ thời gian đến hạn so với thời lượng còn lại chỉ là tín hiệu, không chứng minh giờ học rảnh hoặc lịch khả thi.

## 6. Điểm cần chốt và điều kiện hoàn thành

Chốt Q11, công thức nâng cao, cách giải thích và tiêu chí đánh giá; Q12 giữ ranh giới phạm vi đã ghi nhận, chỉ mở lại nếu có yêu cầu mở rộng. Hoàn thành bản cơ sở khi có unit test bộ xếp hạng, kiểm tra quyền truy vấn, ví dụ kiểm chứng và UI nêu rõ giới hạn. Không cần chọn xong thuật toán nâng cao để bắt đầu Auth/CRUD.
