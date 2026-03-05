## Luồng chính FE ↔ BE trong SmartService

Tài liệu này mô tả **các luồng nghiệp vụ chính** mà FE cần triển khai, kèm theo **những API REST + GraphQL liên quan** (chỉ liệt kê tên, chi tiết field xem thêm Swagger & `GraphQL_Guide.md`).

Ký hiệu:
- **REST**: endpoint từ Swagger (WebAPI)
- **GQL**: query từ GraphQL (`POST /graphql`)

---

## 1. Luồng Authentication & Hồ sơ người dùng

- **Mục tiêu**: Đăng ký / đăng nhập, quản lý token, cập nhật hồ sơ, đổi mật khẩu.

### 1.1. Đăng ký & đăng nhập (Customer)

1. **Đăng ký tài khoản Customer (REST)**
   - `POST /api/auth/register`
   - Nhận về: `accessToken`, `refreshToken`, thông tin user.
2. **Đăng nhập (REST)**
   - `POST /api/auth/login`
   - Nhận về: `accessToken`, `refreshToken`, thông tin user.
3. **Lưu token ở FE** để dùng cho:
   - Header `Authorization: Bearer {accessToken}` cho REST bảo vệ.
   - Header tương tự cho **GraphQL** (`POST /graphql`).

### 1.2. Làm mới & huỷ token

1. **Làm mới access token (REST)**
   - `POST /api/auth/refresh-token`
   - Input: `refreshToken`
   - Output: access token mới + refresh token mới.
2. **Đăng xuất (REST)**
   - `POST /api/auth/logout`
   - Input: `refreshToken` cần revoke.

### 1.3. Quản lý hồ sơ & bảo mật

1. **Cập nhật hồ sơ cá nhân (REST)**
   - `PUT /api/auth/profile`
2. **Đổi mật khẩu (REST)**
   - `POST /api/auth/change-password`
3. **Lấy thông tin user hiện tại (GQL)**
   - `query me { ... }`

### 1.4. Quản trị tài khoản (Admin/Staff)

1. **Tạo mới Customer/Agent/Staff (REST)**
   - `POST /api/users/customers`
   - `POST /api/users/agents`
   - `POST /api/users/staff`
2. **Cập nhật role user (REST – Admin/Staff)**
   - `PATCH /api/auth/users/{id}/role`
3. **Khoá / mở khoá user (REST – Admin)**
   - `PATCH /api/auth/users/{id}/lock`
4. **Quản lý danh sách user (GQL – Admin/Staff)**
   - `getUsers`, `getUsersByRole`, `getUserById`

---

## 2. Luồng Customer tạo & theo dõi yêu cầu dịch vụ

- **Mục tiêu**: Customer chọn danh mục dịch vụ, mô tả nhu cầu, hệ thống tạo ServiceRequest, sau đó có thể xem & feedback.

### 2.1. Chuẩn bị dữ liệu chọn lựa

1. **Danh sách danh mục dịch vụ (GQL – public)**
   - `getServiceCategories`
2. **Chi tiết danh mục (GQL – public)**
   - `getServiceCategoryById`
3. **Danh sách Agent đang active (GQL – public, nếu UI cần hiển thị)**
   - `getActiveServiceAgents`

### 2.2. Tạo yêu cầu dịch vụ (Service Request)

1. **(Tuỳ chọn) Gợi ý AI cho mô tả & độ phức tạp (REST)**
   - `POST /api/service-analysis`
   - Input: `description`
   - Output: gợi ý context/complexity/dispatch policy.
2. **Tạo Service Request (REST)**
   - `POST /api/service-requests`
   - Body: `CreateServiceRequestCommand` (customerId, categoryId, description, optional ComplexityLevel…).
3. **(Tuỳ chọn) Upload / lưu thông tin file đính kèm (REST)**
   - `POST /api/service-attachments`

### 2.3. Customer xem & theo dõi yêu cầu của mình

1. **Xem toàn bộ request của chính mình (GQL)**
   - `getMyServiceRequests`
2. **Xem chi tiết 1 request (GQL)**
   - `getServiceRequestById`
3. **Xem feedback của chính mình (GQL)**
   - `getMyServiceFeedbacks`
   - `getFeedbackByServiceRequestId`

### 2.4. Customer đánh giá & feedback sau khi hoàn thành

1. **Gửi feedback (REST)**
   - `POST /api/service-feedbacks`
2. **Xem lại feedback & điểm trung bình (GQL)**
   - `getServiceFeedbackById`
   - `getAverageRatingByServiceRequestId`

---

## 3. Luồng Staff/Admin quản lý danh mục & dịch vụ

- **Mục tiêu**: Quản trị cấu hình danh mục dịch vụ, danh sách service định nghĩa, phục vụ cho UI và engine matching.

### 3.1. Quản lý Service Categories

1. **Tạo danh mục dịch vụ mới (REST)**
   - `POST /api/service-categories`
2. **Xem danh sách / chi tiết danh mục (GQL – public)**
   - `getServiceCategories`, `getServiceCategoryById`

### 3.2. Quản lý Service Definitions (dịch vụ cụ thể)

1. **Tạo dịch vụ (REST)**
   - `POST /api/services`
2. **Cập nhật dịch vụ (REST)**
   - `PUT /api/services/{id}`
3. **Xoá dịch vụ (REST)**
   - `DELETE /api/services/{id}`
4. **Xem danh sách / chi tiết dịch vụ (GQL)**
   - Các query dạng: `getServiceDefinitions`, `getServiceDefinitionById`, `getServiceDefinitionsByCategory` (theo comment trong `ServicesController` & GraphQL schema).

---

## 4. Luồng điều phối & matching yêu cầu dịch vụ

- **Mục tiêu**: Staff/Admin/Agent phối hợp để đánh giá phức tạp, gán Agent, tạo assignment & matching result.

### 4.1. Staff/Admin xem các request cần xử lý

1. **Danh sách tất cả Service Requests (GQL – Staff/Admin/Agent)**  
   - `getServiceRequests`
2. **Lọc theo trạng thái (GQL – Staff/Admin)**
   - `getServiceRequestsByStatus`
3. **Xem chi tiết 1 request (GQL)**
   - `getServiceRequestById`

### 4.2. Đánh giá độ phức tạp & cập nhật trạng thái

1. **Đánh giá độ phức tạp (REST)**
   - `PATCH /api/service-requests/{serviceRequestId}/evaluate-complexity`
2. **(Tuỳ chọn) Giao diện hiển thị Activity Logs (GQL)**
   - `getActivityLogsByServiceRequestId`

### 4.3. Matching Agent & tạo Assignment

1. **(Tuỳ chọn) Giao diện gợi ý matching (GQL)**
   - `getMatchingResultsByServiceRequestId`
   - `getRecommendedMatches`
2. **Tạo kết quả khớp (REST) – nếu FE cho phép thao tác thủ công**
   - `POST /api/matching-results`
3. **Tạo Assignment (REST)**
   - `POST /api/assignments`
4. **Cập nhật Service Request gán provider (REST)**
   - `PATCH /api/service-requests/{serviceRequestId}/assign-provider`
5. **Agent xem các assignment của mình (GQL)**
   - `getAssignmentsByAgentId`

### 4.4. Nhật ký hoạt động (Audit)

1. **Tạo ActivityLog (REST) – có thể được backend gọi từ các use case, nhưng FE có thể dùng nếu có màn hình manual log**
   - `POST /api/activity-logs`
2. **Xem Activity Logs (GQL – Staff/Admin)**
   - `getActivityLogs`, `getActivityLogById`, `getActivityLogsByServiceRequestId`

---

## 5. Luồng Agent xử lý yêu cầu được gán

- **Mục tiêu**: Agent xem các yêu cầu đã gán, thực hiện công việc và cập nhật thông tin cần thiết.

1. **Agent đăng nhập (REST)**
   - `POST /api/auth/login`
2. **Xem assignment của mình (GQL)**
   - `getAssignmentsByAgentId`
3. **Xem chi tiết Service Request liên quan (GQL)**
   - `getServiceRequestById`
4. **Xem/mở file đính kèm, log, feedback (GQL)**
   - `getActivityLogsByServiceRequestId`
   - `getFeedbackByServiceRequestId`
5. **(Tuỳ roadmap) Các update trạng thái service request có thể sẽ bổ sung thêm REST/GQL sau.**

---

## 6. Tổng hợp nhanh các endpoint FE chắc chắn dùng

- **Auth cơ bản**:  
  - REST: `/api/auth/register`, `/api/auth/login`, `/api/auth/refresh-token`, `/api/auth/logout`, `/api/auth/profile`, `/api/auth/change-password`  
  - GQL: `me`

- **Customer request lifecycle**:  
  - REST: `/api/service-analysis`, `/api/service-requests`, `/api/service-attachments`, `/api/service-feedbacks`  
  - GQL: `getMyServiceRequests`, `getServiceRequestById`, `getMyServiceFeedbacks`, `getFeedbackByServiceRequestId`, `getAverageRatingByServiceRequestId`

- **Service configuration (Admin/Staff)**:  
  - REST: `/api/service-categories`, `/api/services` (POST/PUT/DELETE), `/api/users/*`  
  - GQL: `getServiceCategories`, `getServiceCategoryById`, các query services, `getUsers`, `getUsersByRole`

- **Orchestration & matching**:  
  - REST: `/api/service-requests/{id}/evaluate-complexity`, `/api/service-requests/{id}/assign-provider`, `/api/assignments`, `/api/matching-results`, `/api/activity-logs`  
  - GQL: `getServiceRequests`, `getServiceRequestsByStatus`, `getAssignmentsByAgentId`, `getMatchingResults*`, `getActivityLogs*`

Chi tiết tham số và response cho từng API xem thêm **Swagger UI (`/swagger`)** và **`GraphQL_Guide.md`**.

