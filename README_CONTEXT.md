# NGỮ CẢNH HỆ THỐNG: XÁC THỰC GIÁ VÀ ĐIỀU PHỐI DỊCH VỤ

## 1. Mục tiêu (Objective)
- Ngăn chặn Staff nhập "Chi phí ước tính" thấp hơn "Giá niêm yết" của dịch vụ.
- Hiển thị thông báo lỗi thân thiện (tiếng Việt), chỉ rõ mức giá tối thiểu cần nhập.
- Xử lý thói quen nhập liệu dấu chấm phân cách hàng nghìn của người Việt (vd: `200.000` -> `200000`).

## 2. Các thay đổi đã thực hiện (Changes Made)

### Backend (C#)
- **Domain Exception**: Thêm `PriceTooLowException` trong `ServiceRequestException.cs` với thông báo tiếng Việt.
- **Handlers**: 
    - `EvaluateServiceComplexityHandler.cs`: Kiểm tra giá so với `BasePrice` của `ServiceDefinition`.
    - `AssignProviderHandler.cs`: Kiểm tra giá tương tự trước khi phân công thợ.
- **Middleware**: Cập nhật `ExceptionHandlingMiddleware.cs` để trả về trực tiếp thông báo lỗi cụ thể thay vì chuỗi cố định "Validation failed.".
- **Program.cs**: Cấu hình `InvalidModelStateResponseFactory` để bắt các lỗi Model Binding (như sai kiểu dữ liệu) và trả về `ErrorResponse` chuẩn.

### Frontend (React Native)
- **DispatchCenterScreen.tsx**: 
    - Bổ sung logic `.replace(/\./g, "")` để loại bỏ dấu chấm trước khi parse số tiền.
    - Thêm `console.error` để bắt log lỗi chi tiết trong console.

## 3. Nhật ký sự cố (Issue Log)

### Vấn đề 1: File Locked (Đã xử lý)
- **Hiện tượng**: `dotnet build` thất bại với lỗi "being used by another process".
- **Nguyên nhân**: Các tiến trình `dotnet` cũ chạy ngầm chiếm giữ DLL.
- **Giải quyết**: Đã dùng `taskkill /F /IM dotnet.exe` để dọn sạch. Hiện tại đã Build thành công 100%.

### Vấn đề 2: Lỗi "The request field is required." (ĐANG XỬ LÝ)
- **Hiện tượng**: Màn hình App Staff hiện thông báo màu đỏ: "The request field is required." khi nhấn đánh giá/phân công.
- **Chẩn đoán**: 
    - Đây là lỗi mặc định của ASP.NET Core khi `[FromBody] request` nhận giá trị `null`.
    - Nguyên nhân có thể do cấu trúc JSON gửi lên từ FE không khớp với `EvaluateComplexityRequest` record sau khi hoàn tác/revert nhiều lần.
    - Hoặc do locale của hệ thống khiến việc binding decimal gặp trục trặc.

## 4. Hướng xử lý tiếp theo (Next Steps)
1. Kiểm tra lại cấu trúc JSON thực tế gửi từ `httpRequest` trong `httpClient.ts`.
2. Chỉnh sửa Action `EvaluateComplexity` để cho phép `request` là null (để debug) và trả về thông báo chi tiết hơn.
3. Đảm bảo Backend được khởi động đúng cách sau khi build thành công.

---
*Ghi chú: File này được tạo để lưu trữ ngữ cảnh khi chuyển đổi giữa các phiên làm việc.*
