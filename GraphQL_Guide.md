# HƯỚNG DẪN TRUY VẤN GRAPHQL

## 📋 TỔNG QUAN

SmartService Platform sử dụng **GraphQL** cho tất cả read operations (queries). GraphQL queries yêu cầu JWT authentication và role-based authorization.

### **Kiến trúc:**
- **GraphQL Endpoint:** `POST /graphql`
- **Authentication:** JWT Bearer Token (lấy từ REST API `/api/auth/login` hoặc `/api/auth/register`)
- **Authorization:** Role-based (Customer, Staff, Agent, Admin)
- **Public Queries:** Một số queries không cần authentication (ServiceCategory, GetActiveServiceAgents)

---

## 🔐 AUTHENTICATION

### **Bước 1: Lấy Access Token**

Sử dụng REST API để login và lấy access token:

```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "customer@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "raw_refresh_token_base64_string",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "email": "customer@example.com",
  "fullName": "Nguyễn Văn A",
  "role": "CUSTOMER"
}
```

### **Bước 2: Sử dụng Access Token trong GraphQL**

Thêm `Authorization` header vào mọi GraphQL request:

```
Authorization: Bearer {accessToken}
```

---

## 🚀 GRAPHQL QUERIES

### **Endpoint**

```text
POST /graphql
```

Tất cả query bên dưới (trừ public) đều yêu cầu header:

```text
Authorization: Bearer {accessToken}
```

---

### **1. PUBLIC QUERIES (Không cần authentication)**

#### **1.1. Get Service Categories**

- **Tên query**: `getServiceCategories`
- **Mô tả**: Lấy danh sách tất cả danh mục dịch vụ (Pháp lý, Bất động sản, Kỹ thuật, …).
- **Role**: Public (không cần đăng nhập).

```graphql
query {
  getServiceCategories {
    id
    name
    description
  }
}
```

#### **1.2. Get Service Category By ID**

- **Tên query**: `getServiceCategoryById(id: UUID!)`
- **Mô tả**: Lấy chi tiết một danh mục theo `id`.
- **Role**: Public (không cần đăng nhập).

```graphql
query {
  getServiceCategoryById(id: "123e4567-e89b-12d3-a456-426614174000") {
    id
    name
    description
  }
}
```

#### **1.3. Get Active Service Agents**

- **Tên query**: `getActiveServiceAgents`
- **Mô tả**: Lấy danh sách các đại lý đang hoạt động.
- **Role**: Public (không cần đăng nhập).

```graphql
query {
  getActiveServiceAgents {
    id
    fullName
    isActive
  }
}
```

---

### **2. CUSTOMER QUERIES (Dành cho khách hàng đã đăng nhập)**

#### **2.1. Me – Lấy thông tin tài khoản hiện tại**

- **Tên query**: `me`
- **Mô tả**: Lấy thông tin user hiện tại từ access token (không cần truyền `id`).
- **Role**: Đã đăng nhập (bất kỳ role).

```graphql
query {
  me {
    id
    email
    fullName
    phoneNumber
    role
  }
}
```

#### **2.2. Get User By ID**

- **Tên query**: `getUserById(id: UUID!)`
- **Mô tả**: Lấy chi tiết một user theo `id`.
- **Role**: Đã đăng nhập (bất kỳ role).  
  *Khuyến nghị UI: customer thường chỉ dùng `me`; `getUserById` chủ yếu cho admin/staff.*

```graphql
query {
  getUserById(id: "123e4567-e89b-12d3-a456-426614174000") {
    id
    email
    fullName
    phoneNumber
    role
  }
}
```

#### **2.3. Get Service Request By ID (theo quyền sở hữu)**

- **Tên query**: `getServiceRequestById(id: UUID!)`
- **Mô tả**: Lấy chi tiết một service request.
- **Role**:
  - Customer: chỉ xem được **request của chính mình**.
  - Staff/Admin: xem được tất cả.
  - Agent: tuỳ business (hiện code cho phép nếu có access token, nhưng logic giới hạn chủ yếu theo Customer/Staff/Admin).

```graphql
query {
  getServiceRequestById(id: "123e4567-e89b-12d3-a456-426614174000") {
    id
    customerId
    categoryId
    description
    complexity {
      level
    }
    status
    assignedProviderId
    estimatedCost {
      amount
      currency
    }
    createdAt
  }
}
```

#### **2.4. Get Service Requests By Customer ID**

- **Tên query**: `getServiceRequestsByCustomerId(customerId: UUID!)`
- **Mô tả**: Lấy danh sách service request của một customer.
- **Role**:
  - Customer: **chỉ được dùng với chính `customerId` của mình**.
  - Staff/Admin: xem được theo bất kỳ `customerId`.

```graphql
query {
  getServiceRequestsByCustomerId(customerId: "123e4567-e89b-12d3-a456-426614174000") {
    id
    description
    status
    createdAt
  }
}
```

#### **2.5. Get My Service Requests**

- **Tên query**: `getMyServiceRequests(status: ServiceStatus)`
- **Mô tả**: Lấy danh sách service request của **chính user hiện tại**, có thể lọc theo `status`.
- **Role**: Đã đăng nhập.

```graphql
query {
  getMyServiceRequests(status: PENDING_REVIEW) {
    id
    description
    status
    createdAt
  }
}
```

#### **2.6. Feedback của chính mình**

##### **2.6.1. Get Service Feedback By ID**

- **Tên query**: `getServiceFeedbackById(id: UUID!)`
- **Mô tả**: Lấy chi tiết một feedback.
- **Role**: Đã đăng nhập.

```graphql
query {
  getServiceFeedbackById(id: "123e4567-e89b-12d3-a456-426614174000") {
    id
    serviceRequestId
    rating
    comment
    createdAt
  }
}
```

##### **2.6.2. Get My Service Feedbacks**

- **Tên query**: `getMyServiceFeedbacks`
- **Mô tả**: Lấy danh sách tất cả feedback mà **user hiện tại đã gửi**, sort mới nhất.
- **Role**: Đã đăng nhập.

```graphql
query {
  getMyServiceFeedbacks {
    id
    serviceRequestId
    rating
    comment
    createdAt
  }
}
```

##### **2.6.3. Get Feedback By Service Request ID**

- **Tên query**: `getFeedbackByServiceRequestId(serviceRequestId: UUID!)`
- **Mô tả**: Lấy tất cả feedback của một service request.
- **Role**: Đã đăng nhập.

```graphql
query {
  getFeedbackByServiceRequestId(serviceRequestId: "123e4567-e89b-12d3-a456-426614174000") {
    id
    rating
    comment
    createdAt
  }
}
```

##### **2.6.4. Get Feedback By User ID**

- **Tên query**: `getFeedbackByUserId(userId: UUID!)`
- **Mô tả**: Lấy tất cả feedback do một user cụ thể tạo.
- **Role**: Đã đăng nhập (thường dùng cho Staff/Admin).

```graphql
query {
  getFeedbackByUserId(userId: "123e4567-e89b-12d3-a456-426614174000") {
    id
    serviceRequestId
    rating
    comment
    createdAt
  }
}
```

##### **2.6.5. Get Average Rating By Service Request ID**

- **Tên query**: `getAverageRatingByServiceRequestId(serviceRequestId: UUID!)`
- **Mô tả**: Tính điểm rating trung bình cho một request, trả về `0` nếu chưa có feedback.
- **Role**: Đã đăng nhập.

```graphql
query {
  getAverageRatingByServiceRequestId(serviceRequestId: "123e4567-e89b-12d3-a456-426614174000")
}
```

---

### **3. AGENT QUERIES (Agent / Staff / Admin)**

#### **3.1. Get Assignments By Agent ID**

- **Tên query**: `getAssignmentsByAgentId(agentId: UUID!)`
- **Mô tả**: Lấy danh sách assignment của một agent.
- **Role**: Agent, Staff hoặc Admin.

```graphql
query {
  getAssignmentsByAgentId(agentId: "123e4567-e89b-12d3-a456-426614174000") {
    id
    serviceRequestId
    estimatedCost {
      amount
      currency
    }
    assignedAt
  }
}
```

---

### **4. STAFF / ADMIN QUERIES (Quản trị hệ thống)**

#### **4.1. User management**

##### **4.1.1. Get All Users**

- **Tên query**: `getUsers`
- **Mô tả**: Lấy danh sách tất cả user trong hệ thống.
- **Role**: Staff hoặc Admin.

```graphql
query {
  getUsers {
    id
    email
    fullName
    phoneNumber
    role
  }
}
```

##### **4.1.2. Get Users By Role**

- **Tên query**: `getUsersByRole(role: UserRole!)`
- **Mô tả**: Lọc user theo vai trò (CUSTOMER, STAFF, AGENT, ADMIN).
- **Role**: Staff hoặc Admin.

```graphql
query {
  getUsersByRole(role: CUSTOMER) {
    id
    email
    fullName
  }
}
```

#### **4.2. Service Request management**

##### **4.2.1. Get All Service Requests**

- **Tên query**: `getServiceRequests`
- **Mô tả**: Lấy tất cả service request, sort mới nhất.
- **Role**: Staff, Admin hoặc Agent (theo code hiện tại).

```graphql
query {
  getServiceRequests {
    id
    customerId
    description
    status
    complexity {
      level
    }
    createdAt
  }
}
```

##### **4.2.2. Get Service Requests By Status**

- **Tên query**: `getServiceRequestsByStatus(status: ServiceStatus!)`
- **Mô tả**: Lọc service request theo trạng thái (PENDING, PENDING_REVIEW, IN_PROGRESS, COMPLETED, CANCELLED, …).
- **Role**: Staff hoặc Admin.

```graphql
query {
  getServiceRequestsByStatus(status: PENDING_REVIEW) {
    id
    customerId
    description
    status
    createdAt
  }
}
```

#### **4.3. Assignment & Matching**

##### **4.3.1. Get Assignment By ID**

- **Tên query**: `getAssignmentById(id: UUID!)`
- **Role**: Đã đăng nhập (thường Staff/Admin).

```graphql
query {
  getAssignmentById(id: "123e4567-e89b-12d3-a456-426614174000") {
    id
    serviceRequestId
    agentId
    estimatedCost {
      amount
      currency
    }
    assignedAt
  }
}
```

##### **4.3.2. Get Assignments By Service Request ID**

- **Tên query**: `getAssignmentsByServiceRequestId(serviceRequestId: UUID!)`
- **Role**: Đã đăng nhập (thường Staff/Admin).

```graphql
query {
  getAssignmentsByServiceRequestId(serviceRequestId: "123e4567-e89b-12d3-a456-426614174000") {
    id
    agentId
    estimatedCost {
      amount
      currency
    }
    assignedAt
  }
}
```

##### **4.3.3. Get All Assignments**

- **Tên query**: `getAssignments`
- **Role**: Staff hoặc Admin.

```graphql
query {
  getAssignments {
    id
    serviceRequestId
    agentId
    estimatedCost {
      amount
      currency
    }
    assignedAt
  }
}
```

##### **4.3.4. Get Matching Results By Service Request ID**

- **Tên query**: `getMatchingResultsByServiceRequestId(serviceRequestId: UUID!)`
- **Role**: Đã đăng nhập (thường Staff/Admin).

```graphql
query {
  getMatchingResultsByServiceRequestId(serviceRequestId: "123e4567-e89b-12d3-a456-426614174000") {
    id
    serviceAgentId
    supportedComplexity {
      level
    }
    matchingScore
    isRecommended
  }
}
```

##### **4.3.5. Get Recommended Matches**

- **Tên query**: `getRecommendedMatches(serviceRequestId: UUID!)`
- **Role**: Staff hoặc Admin.

```graphql
query {
  getRecommendedMatches(serviceRequestId: "123e4567-e89b-12d3-a456-426614174000") {
    id
    serviceAgentId
    matchingScore
    isRecommended
  }
}
```

##### **4.3.6. Get All Matching Results**

- **Tên query**: `getMatchingResults`
- **Role**: Staff hoặc Admin.

```graphql
query {
  getMatchingResults {
    id
    serviceRequestId
    serviceAgentId
    matchingScore
    isRecommended
  }
}
```

#### **4.4. Activity Logs**

##### **4.4.1. Get All Activity Logs**

- **Tên query**: `getActivityLogs`
- **Mô tả**: Lấy tất cả nhật ký hoạt động (audit).
- **Role**: Staff hoặc Admin.

```graphql
query {
  getActivityLogs {
    id
    serviceRequestId
    action
    createdAt
  }
}
```

##### **4.4.2. Get Activity Log By ID**

- **Tên query**: `getActivityLogById(id: UUID!)`
- **Role**: Đã đăng nhập.

```graphql
query {
  getActivityLogById(id: "123e4567-e89b-12d3-a456-426614174000") {
    id
    serviceRequestId
    action
    createdAt
  }
}
```

##### **4.4.3. Get Activity Logs By Service Request ID**

- **Tên query**: `getActivityLogsByServiceRequestId(serviceRequestId: UUID!)`
- **Role**: Đã đăng nhập.

```graphql
query {
  getActivityLogsByServiceRequestId(serviceRequestId: "123e4567-e89b-12d3-a456-426614174000") {
    id
    action
    createdAt
  }
}
```

---

### **4. AGENT QUERIES (Yêu cầu: Agent, Staff hoặc Admin)**

#### **Get Assignments By Agent ID**
```graphql
query {
  getAssignmentsByAgentId(agentId: "123e4567-e89b-12d3-a456-426614174000") {
    id
    serviceRequestId
    estimatedCost {
      amount
      currency
    }
    assignedAt
  }
}
```

**Required Role:** `Agent`, `Staff` hoặc `Admin`

---

## 📱 VÍ DỤ SỬ DỤNG

### **cURL Example:**

```bash
# 1. Login để lấy access token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer@example.com",
    "password": "SecurePassword123!"
  }'

# 2. Sử dụng access token để query GraphQL
curl -X POST http://localhost:5000/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -d '{
    "query": "query { getServiceRequestById(id: \"123e4567-e89b-12d3-a456-426614174000\") { id description status } }"
  }'
```

### **JavaScript/TypeScript Example:**

```typescript
// 1. Login
const loginResponse = await fetch('http://localhost:5000/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'customer@example.com',
    password: 'SecurePassword123!'
  })
});

const { accessToken } = await loginResponse.json();

// 2. GraphQL Query
const graphqlResponse = await fetch('http://localhost:5000/graphql', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${accessToken}`
  },
  body: JSON.stringify({
    query: `
      query {
        getServiceRequestById(id: "123e4567-e89b-12d3-a456-426614174000") {
          id
          description
          status
          complexity {
            level
          }
        }
      }
    `
  })
});

const result = await graphqlResponse.json();
console.log(result.data);
```

---

## 🧪 TESTING VỚI GRAPHQL PLAYGROUND

1. **Mở GraphQL Playground:**
   ```
   http://localhost:5000/graphql
   ```

2. **Lấy Access Token:**
   - Mở Swagger UI: `http://localhost:5000/swagger`
   - Test `POST /api/auth/login`
   - Copy `accessToken` từ response

3. **Thêm Authorization Header:**
   - Trong GraphQL Playground, click vào tab "HTTP HEADERS"
   - Thêm:
     ```json
     {
       "Authorization": "Bearer {your_access_token_here}"
     }
     ```

4. **Test Query:**
   ```graphql
   query {
     getServiceRequestById(id: "your-service-request-id") {
       id
       description
       status
     }
   }
   ```

---

## 🔒 ROLE-BASED AUTHORIZATION

### **Role Permissions:**

| Query | Customer | Agent | Staff | Admin |
|-------|----------|-------|-------|-------|
| `getServiceCategories` | ✅ | ✅ | ✅ | ✅ |
| `getActiveServiceAgents` | ✅ | ✅ | ✅ | ✅ |
| `getUserById` | ✅ | ✅ | ✅ | ✅ |
| `getServiceRequestById` | ✅* | ✅ | ✅ | ✅ |
| `getServiceRequestsByCustomerId` | ✅* | ✅ | ✅ | ✅ |
| `getAssignmentsByAgentId` | ❌ | ✅ | ✅ | ✅ |
| `getUsers` | ❌ | ❌ | ✅ | ✅ |
| `getServiceRequests` | ❌ | ❌ | ✅ | ✅ |
| `getAssignments` | ❌ | ❌ | ✅ | ✅ |
| `getMatchingResults` | ❌ | ❌ | ✅ | ✅ |
| `getActivityLogs` | ❌ | ❌ | ✅ | ✅ |

*Customer chỉ xem được data của chính mình

---

## 🐛 ERROR HANDLING

### **Unauthorized Error (401):**
```json
{
  "errors": [
    {
      "message": "The current user is not authorized to access this resource.",
      "extensions": {
        "code": "AUTH_NOT_AUTHENTICATED"
      }
    }
  ]
}
```

**Giải pháp:** Thêm `Authorization: Bearer {accessToken}` header

### **Forbidden Error (403):**
```json
{
  "errors": [
    {
      "message": "The current user is not authorized to access this resource.",
      "extensions": {
        "code": "AUTH_NOT_AUTHORIZED"
      }
    }
  ]
}
```

**Giải pháp:** User không có đủ quyền (role). Cần role `Staff` hoặc `Admin` cho query này.

### **Token Expired:**
```json
{
  "errors": [
    {
      "message": "Token validation failed"
    }
  ]
}
```

**Giải pháp:** 
1. Sử dụng refresh token để lấy access token mới:
   ```bash
   POST /api/auth/refresh-token
   { "refreshToken": "your_refresh_token" }
   ```
2. Hoặc login lại

---

## 📚 QUERY EXAMPLES

### **Customer View - Xem requests của mình:**

```graphql
query GetMyServiceRequests($customerId: UUID!) {
  getServiceRequestsByCustomerId(customerId: $customerId) {
    id
    description
    status
    complexity {
      level
    }
    assignedProviderId
    estimatedCost {
      amount
      currency
    }
    createdAt
  }
}
```

**Variables:**
```json
{
  "customerId": "123e4567-e89b-12d3-a456-426614174000"
}
```

### **Staff View - Xem tất cả requests cần review:**

```graphql
query GetPendingReviewRequests {
  getServiceRequestsByStatus(status: PENDING_REVIEW) {
    id
    customerId
    description
    complexity {
      level
    }
    createdAt
  }
}
```

### **Agent View - Xem assignments của mình:**

```graphql
query GetMyAssignments($agentId: UUID!) {
  getAssignmentsByAgentId(agentId: $agentId) {
    id
    serviceRequestId
    estimatedCost {
      amount
      currency
    }
    assignedAt
  }
}
```

**Variables:**
```json
{
  "agentId": "123e4567-e89b-12d3-a456-426614174000"
}
```

---

## ✅ CHECKLIST

- [x] Login qua REST API để lấy access token
- [x] Thêm `Authorization: Bearer {accessToken}` header
- [x] Test public queries (không cần auth)
- [x] Test authenticated queries
- [x] Test role-based queries (Staff/Admin)
- [x] Xử lý token expiration (refresh token)

---

**Tài liệu này cung cấp hướng dẫn đầy đủ để truy vấn GraphQL trong SmartService Platform.**
