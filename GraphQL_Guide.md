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

### **Endpoint:**
```
POST /graphql
```

### **1. PUBLIC QUERIES (Không cần authentication)**

#### **Get Service Categories**
```graphql
query {
  getServiceCategories {
    id
    name
    description
  }
}
```

#### **Get Active Service Agents**
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

### **2. AUTHENTICATED QUERIES (Yêu cầu: Authenticated)**

#### **Get User By ID**
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

**Header:**
```
Authorization: Bearer {accessToken}
```

#### **Get Service Request By ID**
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

**Lưu ý:** 
- Customer chỉ xem được requests của mình
- Staff/Admin xem được tất cả

#### **Get Service Requests By Customer ID**
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

**Lưu ý:**
- Customer chỉ xem được requests của mình
- Staff/Admin xem được tất cả

#### **Get Assignment By ID**
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

#### **Get Assignments By Service Request ID**
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

#### **Get Matching Results By Service Request ID**
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

#### **Get Activity Logs By Service Request ID**
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

### **3. STAFF/ADMIN QUERIES (Yêu cầu: Staff hoặc Admin role)**

#### **Get All Users**
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

**Required Role:** `Staff` hoặc `Admin`

#### **Get Users By Role**
```graphql
query {
  getUsersByRole(role: CUSTOMER) {
    id
    email
    fullName
  }
}
```

**Required Role:** `Staff` hoặc `Admin`

#### **Get All Service Requests**
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

**Required Role:** `Staff` hoặc `Admin`

#### **Get Service Requests By Status**
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

**Required Role:** `Staff` hoặc `Admin`

#### **Get All Assignments**
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

**Required Role:** `Staff` hoặc `Admin`

#### **Get Recommended Matches**
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

**Required Role:** `Staff` hoặc `Admin`

#### **Get All Matching Results**
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

**Required Role:** `Staff` hoặc `Admin`

#### **Get All Activity Logs**
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

**Required Role:** `Staff` hoặc `Admin`

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
