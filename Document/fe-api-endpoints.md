## Danh sách API FE cần dùng theo luồng

Tài liệu này liệt kê **cụ thể các API REST & GraphQL** mà FE cần gọi để chạy đúng các luồng trong hệ thống SmartService.

- **Base URL** (dev mặc định): `http://localhost:5000`
- **Swagger UI**: `GET /swagger`
- **GraphQL Playground**: `POST /graphql` (hoặc truy cập UI: `GET /graphql`)

---

## 1. Authentication & User Profile

### 1.1. Đăng ký Customer

- **REST**
  - **Method**: `POST`
  - **Endpoint**: `/api/auth/register`
  - **Body (JSON)**: `RegisterRequest`
  - **Auth**: Không
  - **Use**: Đăng ký tài khoản Customer mới từ FE.

### 1.2. Đăng nhập

- **REST**
  - **Method**: `POST`
  - **Endpoint**: `/api/auth/login`
  - **Body (JSON)**: `LoginRequest`
  - **Auth**: Không
  - **Use**: Lấy `accessToken`, `refreshToken` cho mọi role (Customer/Staff/Agent/Admin).

### 1.3. Refresh token

- **REST**
  - **Method**: `POST`
  - **Endpoint**: `/api/auth/refresh-token`
  - **Body (JSON)**: `RefreshTokenRequest`
  - **Auth**: Không (dựa trên refreshToken)
  - **Use**: FE gọi khi access token hết hạn.

### 1.4. Logout

- **REST**
  - **Method**: `POST`
  - **Endpoint**: `/api/auth/logout`
  - **Body (JSON)**: `LogoutRequest`
  - **Auth**: (tuỳ logic, nhưng nên gửi cả Bearer & refreshToken)
  - **Use**: Đăng xuất, revoke refresh token.

### 1.5. Cập nhật hồ sơ cá nhân

- **REST**
  - **Method**: `PUT`
  - **Endpoint**: `/api/auth/profile`
  - **Body (JSON)**: `UpdateProfileRequest`
  - **Auth**: `Authorization: Bearer {accessToken}`
  - **Use**: Màn hình cập nhật thông tin user đang đăng nhập.

### 1.6. Đổi mật khẩu

- **REST**
  - **Method**: `POST`
  - **Endpoint**: `/api/auth/change-password`
  - **Body (JSON)**: `ChangePasswordRequest`
  - **Auth**: Bearer
  - **Use**: Màn hình đổi mật khẩu.

### 1.7. Lấy thông tin user hiện tại

- **GraphQL**
  - **HTTP**:
    - **Method**: `POST`
    - **Endpoint**: `/graphql`
    - **Headers**: `Authorization: Bearer {accessToken}`
  - **Query**:
    - `me`
  - **Use**: Lấy thông tin user hiện tại sau login để hiện trên UI.

### 1.8. Quản trị user (Admin/Staff)

- **REST**
  - Tạo mới user theo loại:
    - `POST /api/users/customers`
    - `POST /api/users/agents`
    - `POST /api/users/staff`
  - Cập nhật role:
    - `PATCH /api/auth/users/{id}/role`
  - Khoá / mở khoá account:
    - `PATCH /api/auth/users/{id}/lock`
  - **Auth**: Bearer, role phù hợp (Admin/Staff).

- **GraphQL**
  - `getUsers`, `getUsersByRole`, `getUserById`
  - **Auth**: Bearer, role Staff/Admin.

---

## 2. Service Categories & Service Definitions

### 2.1. Service Categories (REST – command)

- **REST**
  - **Tạo danh mục mới**:
    - `POST /api/service-categories`
    - **Auth**: Bearer (Staff/Admin)

### 2.2. Service Categories (GraphQL – query)

- **GraphQL**
  - Public:
    - `getServiceCategories`
    - `getServiceCategoryById`
  - **Auth**: Không cần (public) trừ khi override cấu hình.
  - **Dùng cho**:
    - Màn hình chọn danh mục khi tạo ServiceRequest.
    - Màn filter / browse dịch vụ.

### 2.3. Service Definitions (REST – command)

- **REST**
  - **Tạo dịch vụ**:
    - `POST /api/services`
  - **Cập nhật dịch vụ**:
    - `PUT /api/services/{id}`
  - **Xoá dịch vụ**:
    - `DELETE /api/services/{id}`
  - **Auth**: Bearer (Staff/Admin).

### 2.4. Service Definitions (GraphQL – query)

- **GraphQL**
  - Các query (theo comment trong `ServicesController` & schema):
    - `getServiceDefinitions`
    - `getServiceDefinitionById`
    - `getServiceDefinitionsByCategory`
  - **Auth**: tuỳ config (thường là Customer/Staff/Agent/Admin đều dùng được).
  - **Dùng cho**:
    - Màn hình list dịch vụ trong danh mục.
    - Màn chi tiết dịch vụ.

---

## 3. Service Requests & Attachments & Feedback

### 3.1. Phân tích mô tả bằng AI (tuỳ chọn)

- **REST**
  - **Method**: `POST`
  - **Endpoint**: `/api/service-analysis`
  - **Body**: `{ "description": "..." }`
  - **Auth**: Bearer (Customer/Staff)
  - **Dùng cho**: Gợi ý complexity, context, dispatch policy trước khi submit request.

### 3.2. Tạo Service Request

- **REST**
  - **Method**: `POST`
  - **Endpoint**: `/api/service-requests`
  - **Body**: `CreateServiceRequestCommand`
  - **Auth**: Bearer (Customer, hoặc Staff tạo hộ)
  - **Dùng cho**: Màn hình tạo yêu cầu dịch vụ.

### 3.3. Tệp đính kèm Service Request

- **REST**
  - **Method**: `POST`
  - **Endpoint**: `/api/service-attachments`
  - **Body**: `CreateServiceAttachmentCommand`
  - **Auth**: Bearer
  - **Dùng cho**: Màn tải lên / lưu metadata file liên quan đến ServiceRequest.

### 3.4. Customer xem & theo dõi Service Requests của mình

- **GraphQL**
  - `getMyServiceRequests`
  - `getServiceRequestsByCustomerId`
  - `getServiceRequestById`
  - **Auth**: Bearer (Customer).
  - **Dùng cho**:
    - Dashboard “yêu cầu của tôi”.
    - Màn chi tiết Service Request.

### 3.5. Feedback sau khi hoàn thành

- **REST**
  - **Tạo feedback**:
    - `POST /api/service-feedbacks`
    - **Body**: `CreateServiceFeedbackCommand`
    - **Auth**: Bearer (Customer).

- **GraphQL**
  - `getServiceFeedbackById`
  - `getMyServiceFeedbacks`
  - `getFeedbackByServiceRequestId`
  - `getFeedbackByUserId`
  - `getAverageRatingByServiceRequestId`
  - **Auth**: Bearer.

---

## 4. Orchestration, Matching, Assignments & Activity Logs

### 4.1. Staff/Admin xem & lọc Service Requests (read side)

- **GraphQL**
  - `getServiceRequests`
  - `getServiceRequestsByStatus`
  - `getServiceRequestById`
  - `getServiceRequestsByCustomerId`
  - **Auth**: Bearer (Staff/Admin, một số cho Agent).
  - **Dùng cho**: Màn hình danh sách yêu cầu cần review, quản trị.

### 4.2. Đánh giá complexity & gán provider (command side)

- **REST**
  - Đánh giá độ phức tạp:
    - `PATCH /api/service-requests/{serviceRequestId}/evaluate-complexity`
  - Gán provider + estimated cost:
    - `PATCH /api/service-requests/{serviceRequestId}/assign-provider`
  - **Auth**: Bearer (Staff/Admin).
  - **Dùng cho**: Màn điều phối nội bộ.

### 4.3. Assignments (phân công Agent)

- **REST**
  - Tạo Assignment:
    - `POST /api/assignments`
    - **Body**: `CreateAssignmentCommand`
    - **Auth**: Bearer (Staff/Admin).

- **GraphQL**
  - Xem assignments theo Agent:
    - `getAssignmentsByAgentId`
  - Xem chi tiết assignment:
    - `getAssignmentById`
    - `getAssignmentsByServiceRequestId`
  - **Auth**: Bearer (Agent/Staff/Admin).

### 4.4. Matching Results (kết quả khớp)

- **REST**
  - Tạo kết quả khớp:
    - `POST /api/matching-results`
    - **Body**: `CreateMatchingResultCommand`
    - **Auth**: Bearer (Staff/Admin).

- **GraphQL**
  - `getMatchingResults`
  - `getMatchingResultsByServiceRequestId`
  - `getRecommendedMatches`
  - **Auth**: Bearer (Staff/Admin).

### 4.5. Activity Logs (Audit)

- **REST**
  - Tạo Activity Log:
    - `POST /api/activity-logs`
    - **Body**: `CreateActivityLogCommand`
    - **Auth**: Bearer (Staff/Admin hoặc hệ thống nội bộ).

- **GraphQL**
  - Xem toàn bộ logs:
    - `getActivityLogs`
  - Xem theo id / theo ServiceRequest:
    - `getActivityLogById`
    - `getActivityLogsByServiceRequestId`
  - **Auth**: Bearer (Staff/Admin).

---

## 5. Agent flows

### 5.1. Agent đăng nhập & lấy thông tin

- **REST**
  - Như Authentication chung:
    - `POST /api/auth/login`
    - `POST /api/auth/refresh-token`

- **GraphQL**
  - `me`

### 5.2. Agent xem các công việc được gán

- **GraphQL**
  - `getAssignmentsByAgentId`
  - `getServiceRequestById`
  - (tuỳ UI) `getActivityLogsByServiceRequestId`, `getFeedbackByServiceRequestId`
  - **Auth**: Bearer (Agent).

---

## 6. Tóm tắt endpoint bắt buộc theo vai trò

### 6.1. Customer

- **REST**
  - `/api/auth/register`
  - `/api/auth/login`
  - `/api/auth/refresh-token`
  - `/api/auth/logout`
  - `/api/auth/profile`
  - `/api/auth/change-password`
  - `/api/service-analysis` (tuỳ chọn)
  - `/api/service-requests`
  - `/api/service-attachments`
  - `/api/service-feedbacks`

- **GraphQL**
  - `getServiceCategories`, `getServiceCategoryById`
  - `me`
  - `getMyServiceRequests`, `getServiceRequestsByCustomerId`, `getServiceRequestById`
  - `getMyServiceFeedbacks`, `getFeedbackByServiceRequestId`, `getAverageRatingByServiceRequestId`

### 6.2. Agent

- **REST**
  - `/api/auth/login`, `/api/auth/refresh-token`

- **GraphQL**
  - `me`
  - `getAssignmentsByAgentId`
  - `getServiceRequestById`
  - (tuỳ UI) `getActivityLogsByServiceRequestId`, `getFeedbackByServiceRequestId`

### 6.3. Staff/Admin

- **REST**
  - Toàn bộ auth endpoints
  - `/api/users/customers`, `/api/users/agents`, `/api/users/staff`
  - `/api/auth/users/{id}/role`
  - `/api/auth/users/{id}/lock`
  - `/api/service-categories`
  - `/api/services` (POST/PUT/DELETE)
  - `/api/service-requests/{id}/evaluate-complexity`
  - `/api/service-requests/{id}/assign-provider`
  - `/api/assignments`
  - `/api/matching-results`
  - `/api/activity-logs`

- **GraphQL**
  - `getUsers`, `getUsersByRole`, `getUserById`
  - `getServiceRequests`, `getServiceRequestsByStatus`, `getServiceRequestById`, `getServiceRequestsByCustomerId`
  - `getAssignments`, `getAssignmentsByServiceRequestId`, `getAssignmentsByAgentId`
  - `getMatchingResults`, `getMatchingResultsByServiceRequestId`, `getRecommendedMatches`
  - `getActivityLogs`, `getActivityLogById`, `getActivityLogsByServiceRequestId`

---

## 7. Ghi chú triển khai FE

- **Auth middleware FE**:
  - Luôn gắn `Authorization: Bearer {accessToken}` cho:
    - Tất cả REST command (trừ register/login/refresh-token).
    - Tất cả GraphQL query private.
- **GraphQL client**:
  - Một endpoint duy nhất: `/graphql`.
  - FE nên xây dựng lớp client dùng chung cho mọi query/mutation.
- **Phân quyền UI**:
  - Dựa trên `role` trả về từ login / `me`:
    - `CUSTOMER`, `AGENT`, `STAFF`, `ADMIN`.
  - Ẩn/hiện menu và nút hành động tương ứng với danh sách API ở trên.

