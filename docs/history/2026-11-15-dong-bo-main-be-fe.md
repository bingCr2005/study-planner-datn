# 2026-11-15 — Đồng bộ main vào nhánh BE/FE và giải quyết xung đột PR

## Yêu cầu và mục đích

- Người dùng yêu cầu push BE/FE, lấy main mới nhất; sau đó gửi ảnh PR BE #15 và FE #5 đang báo conflict. Giữ nhánh `feat/thong-bao-va-cau-hinh-nhac`, gộp main vào nhánh này rồi push; không merge PR vào main.
- Trước gộp đã push BE `0108a4a` và FE `a9b8645`. Main đã fetch: BE `70646d3`, FE `dbdec7e`. Đã đọc AGENTS, mục lục và các bản ghi gộp main, phiên đăng nhập, quy tắc ưu tiên, danh mục JSON, bỏ nút tải lại và các chức năng hiện tại trước khi giải quyết.

## Code và logic thay đổi

- Main bổ sung quản trị danh mục học kỳ/môn mẫu trong JSON, quy tắc ưu tiên và xếp hạng Hôm nay, tổng quan/thống kê, kênh SSE xử lý khóa tài khoản. Giữ toàn bộ các phần này cùng đồng hồ, tiến độ cây, giờ rảnh, cấu hình học và thông báo của nhánh hiện tại.
- Xung đột BE: `backend/tests/StudyPlanner.SchemaChecks/Program.cs`, `docs/history/README.md`, `docs/modules/07-goi-y-cong-viec.md`, `docs/modules/README.md`. Giữ kiểm tra model 22 bảng/34 FK, tên khoảng thời gian học và mapping checklist; hợp mục lục lịch sử hai phía. Mục đồng hồ là phần mở rộng, giữ đường dẫn file đã có và bỏ số module trên tiêu đề để không trùng module 09 danh mục mới từ main.
- Module 07 giữ mô tả preview/apply hiện hành; bổ sung phân biệt cấu hình ưu tiên trên trang Hôm nay với thuật toán phân bổ phiên tự học. Không ghi rằng cấu hình thứ tự tiêu chí của quản trị đã được tích hợp vào `QuyTacLapLich` khi code chưa làm vậy.
- FE `tests/browser-auth.mjs` xung đột ở luồng admin: giữ đăng nhập chung → Tổng quan → Tài khoản sinh viên theo main, comment tiếng Việt. Các file thông báo và thống kê được Git tự gộp; kiểm tra lại bằng build/unit/browser thay vì chọn bỏ một phía.
- `tests/browser-hoc-tap.mjs`: fixture dùng luồng SSE HTTP từ bộ kiểm tra phiên, thêm phản hồi quy tắc mặc định cho Hôm nay. Tách cache Vite theo thư mục thử để không ghi đè cache khi chạy song song với client contract BE. Chờ nút hết disabled trước khi bấm.
- `tests/browser-auth.mjs`: tách cache, tìm và bấm phần tử trong cùng lượt JS rồi chờ React dựng lại. `tests/browser-catalog.mjs` đợi đúng tab đã được chọn trước khi mở form; tránh bấm form môn trong lúc vẫn đang chuyển sang học kỳ.
- FE `src/admin/AdminPriorityRulesPage.tsx`: main đã bỏ nút tải lại thường trực, nhưng lỗi áp dụng do phiên bản cũ chỉ hiện thông báo, chưa có cách tải cấu hình mới tại chỗ. Bổ sung nút **Tải dữ liệu mới** chỉ khi lỗi thao tác, giữ nguyên form đang soạn. `tests/browser-priority-rules.mjs` dùng nút trong lỗi để tải quy tắc đang áp dụng rồi thử lại; không khôi phục nút header đã được yêu cầu bỏ.
- BE `backend/tests/StudyPlanner.CatalogChecks/TransferChecks.cs`: kiểm tra cũ giả lập migration danh mục cũ trên DB đã chạy migration mới của nhánh, nên công cụ chuyển JSON từ chối đúng theo bảo vệ. Dựng DB thử legacy riêng tại mốc trước danh mục, thêm hai tài khoản giả và dữ liệu mẫu; giữ nguyên công cụ sản phẩm. Bổ sung ca có migration mới hơn phải bị chặn trước khi đổi JSON/bảng, rồi kiểm tra xuất/giữ phiên bản/sequence/xung đột JSON/chuyển và dọn DB riêng.
- Không sửa migration hoặc chạy cập nhật trên database Development người dùng trong lượt gộp này. Ảnh/log/browser cache vẫn ngoài commit.

## Kiểm chứng

- BE build solution Release đạt 0 cảnh báo/0 lỗi; SchemaChecks đạt 22 bảng/34 FK và SQL/mapping. Không còn file unmerged.
- CoreChecks PostgreSQL cục bộ riêng đạt: 30 kiểm tra migration, 561 HTTP nghiệp vụ, 39 giờ rảnh, 38 cấu hình/lập lại lịch, 47 nhắc/cài đặt và 121 đồng hồ cùng các kiểm tra thuần. Client nhiệm vụ FE thật → HTTP API → PostgreSQL đạt.
- Các bộ từ main đạt: SessionChecks (kênh khóa tài khoản/heartbeat/quyền), PriorityChecks (cấu hình/áp dụng/đồng thời/phân trang), CatalogChecks (JSON/quyền/CRUD/chuyển legacy) và OverviewChecks (tổng quan/quyền/số liệu/bộ lọc).
- Các project kiểm thử mới chưa có assets nên lượt `--no-restore` đầu không chạy; đã restore và chạy lại đạt. Ca chuyển danh mục đầu bị guard từ chối vì fixture sai mốc lịch sử, đã sửa fixture như trên; không nới guard sản phẩm.
- FE `npm test` đạt 167/167; TypeScript/Vite production build đạt, còn cảnh báo chunk JS lớn hơn 500 kB đã có. Trình duyệt đạt các luồng thông báo/cài đặt, giờ rảnh/lập lại lịch, đồng hồ, phiên tự học, tiến độ cây, nhắc hạn cũ, SSE khóa tài khoản, quy tắc ưu tiên và danh mục.
- Browser dùng API/stream fixture, BE dùng DB thử tạo/xóa riêng; không ghi thành kiểm thử trình duyệt trên tài khoản thật. Các lỗi browser ban đầu do fixture thiếu API mới, cache dùng chung, selector nút đã bỏ và thao tác trước lượt dựng giao diện đã được đối chiếu/sửa rồi chạy lại.
- Đã đọc diff các phần giải quyết và đối chiếu nội dung hai phía. Kết quả push/commit merge được báo trực tiếp sau khi hoàn tất, không tự ghi người dùng đã duyệt.

## Việc tiếp theo

- GitHub kiểm tra lại khả năng merge khi nhánh nguồn được push. Chưa thực hiện merge PR vào main; người dùng có thể rà thay đổi trên PR sau khi conflict được giải quyết.

## Đọc lại trước khi đưa code lên

- [x] AI đã đối chiếu bản ghi với code/diff và ghi đúng kết quả kiểm tra.
- [ ] Người dùng đã đọc bản ghi và code/diff liên quan. Chỉ người dùng xác nhận mục này.

Commit/PR liên quan: PR BE #15 và FE #5; commit merge tạo sau kiểm chứng theo yêu cầu người dùng.
