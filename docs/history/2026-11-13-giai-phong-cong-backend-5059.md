# 2026-11-13 — Giải phóng cổng backend cho terminal người dùng

## Yêu cầu và mục đích

- Người dùng báo backend lỗi khi build/chạy và cung cấp log của `dotnet run --project StudyPlanner.Api` tại thư mục backend.
- Đã đọc mục lục và [bản ghi gợi ý môn](2026-11-13-goi-y-mon-mau-khi-them-mon.md). Lượt trước AI khởi động API bằng build Catalog dưới dạng tiến trình nền để nạp endpoint mới, nhưng để tiến trình đó chạy nên lần người dùng chạy từ terminal bị trùng cổng.

## Code và logic thay đổi

- Không sửa code nghiệp vụ, launchSettings, cổng frontend/backend, migration hoặc database.
- Trước: tiến trình nền dotnet PID 18064 do AI tạo vẫn phục vụ localhost:5059. Lệnh chạy thứ hai của người dùng báo `AddressInUseException`/SocketException 10048. Trong quá trình host thất bại và dispose, worker và EventLog tiếp tục xuất lỗi disposed như log người dùng gửi.
- Sau khi được cấp quyền: chỉ dừng PID 18064, xác minh executable là dotnet và thời điểm khởi động khớp thời điểm tạo log runtime của lượt trước. Không dừng các tiến trình dotnet khác, không mở phiên backend nền thay thế.
- Luồng: đọc lỗi gốc từ log người dùng → đối chiếu PID/log với phiên AI tạo → dừng đúng phiên nền → xác minh đã thoát và API không còn phục vụ trên 5059 → để người dùng chạy bằng terminal của mình.

## Kiểm chứng

- Build toàn `backend/StudyPlanner.sln` vào `backend/obj/backend-diagnosis/bin/` với `--no-restore` và `CopyRetryCount=0`: thành công, 0 cảnh báo, 0 lỗi. Không ghi đè output của API đang chạy.
- Trước khi dừng: Swagger 200, endpoint bảo vệ nhận 401 khi thiếu JWT; xác minh đây không phải bằng chứng build bị lỗi. Log người dùng chỉ rõ ứng dụng mới không bind được 5059.
- Sau khi dừng: PID 18064 không còn, không có tiến trình `StudyPlanner.Api`, request Swagger localhost:5059 không còn nhận được phản hồi. Không tự chạy lại backend để tiếp tục chiếm cổng.
- Không chạy unit/DB tests vì không đổi source. Chưa xác nhận lần `dotnet run` tiếp theo trong terminal của người dùng; bước đó do người dùng khởi động để giữ quyền kiểm soát vòng đời server.
- Các lỗi kết nối PostgreSQL thoáng qua đã thấy trong log nền được tách khỏi lỗi trùng cổng người dùng cung cấp; không tự sửa chính sách transaction/retry hoặc database dựa trên lỗi thứ cấp.

## Việc tiếp theo

- Tại `D:/study-planner-datn/backend`, người dùng chạy `dotnet run --project StudyPlanner.Api` như trước.
- Giữ việc chạy backend trong terminal người dùng, không mở thêm phiên nền trong tác vụ xử lý này. Chưa commit/push.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với log và kết quả kiểm tra tiến trình/build.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: chưa tạo.
