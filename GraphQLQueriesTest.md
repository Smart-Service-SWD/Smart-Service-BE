
# GraphQL Queries Test Guide

## 📍 URL: `http://localhost:5268/graphql`

---

## ✅ **CÁCH QUERY CHUẨN**

```graphql
query {
  querySmartService
  serviceRequests {
    id
    customerId
    categoryId
    description
    complexity
    status
    assignedProviderId
    createdAt
  }
}
```

**Lưu ý:** `querySmartService` là query từ **Query root class**, các query khác là từ **Query Extensions**

---

## 🧪 **1. ROOT QUERY - QuerySmartService (Health Check)**

```graphql
query {
  querySmartService
}
```

**Response:** `"SmartService GraphQL is running"`

---

## 🧪 **2. SERVICE REQUEST QUERIES - serviceRequests**

### Get All ServiceRequests
```graphql
query {
  querySmartService
  serviceRequests {
    id
    customerId
    categoryId
    description
    complexity
    status
    assignedProviderId
    createdAt
  }
}
```

### Get ServiceRequest By ID
```graphql
query {
  querySmartService
  serviceRequestById(id: "00000000-0000-0000-0000-000000000001") {
    id
    customerId
    categoryId
    description
    complexity
    status
    assignedProviderId
    createdAt
  }
}
```

### Get ServiceRequests By Customer
```graphql
query {
  querySmartService
  serviceRequestsByCustomerId(customerId: "00000000-0000-0000-0000-000000000001") {
    id
    customerId
    categoryId
    description
    complexity
    status
    assignedProviderId
    createdAt
  }
}
```

### Get ServiceRequests By Status
```graphql
query {
  querySmartService
  serviceRequestsByStatus(status: "PENDING") {
    id
    customerId
    categoryId
    description
    complexity
    status
    assignedProviderId
    createdAt
  }
}
```

---

## 🧪 **3. USER QUERIES - users**

### Get All Users
```graphql
query {
  querySmartService
  users {
    id
    fullName
    email
    phoneNumber
    role
    createdAt
  }
}
```

### Get User By ID
```graphql
query {
  querySmartService
  userById(id: "00000000-0000-0000-0000-000000000001") {
    id
    fullName
    email
    phoneNumber
    role
    createdAt
  }
}
```

### Get Users By Role
```graphql
query {
  querySmartService
  usersByRole(role: "CUSTOMER") {
    id
    fullName
    email
    phoneNumber
    role
    createdAt
  }
}
```

---

## 🧪 **4. SERVICE AGENT QUERIES - serviceAgents**

### Get All ServiceAgents
```graphql
query {
  querySmartService
  serviceAgents {
    id
    fullName
    email
    phoneNumber
    isActive
    createdAt
  }
}
```

### Get ServiceAgent By ID
```graphql
query {
  querySmartService
  serviceAgentById(id: "00000000-0000-0000-0000-000000000001") {
    id
    fullName
    email
    phoneNumber
    isActive
    createdAt
  }
}
```

### Get Active ServiceAgents
```graphql
query {
  querySmartService
  activeServiceAgents {
    id
    fullName
    email
    phoneNumber
    isActive
    createdAt
  }
}
```

---

## 🧪 **5. SERVICE CATEGORY QUERIES - serviceCategories**

### Get All ServiceCategories
```graphql
query {
  querySmartService
  serviceCategories {
    id
    name
    description
    createdAt
  }
}
```

### Get ServiceCategory By ID
```graphql
query {
  querySmartService
  serviceCategoryById(id: "00000000-0000-0000-0000-000000000001") {
    id
    name
    description
    createdAt
  }
}
```

---

## 🧪 **6. ASSIGNMENT QUERIES - assignments**

### Get All Assignments
```graphql
query {
  querySmartService
  assignments {
    id
    serviceRequestId
    agentId
    estimatedCost
    assignedAt
    completedAt
  }
}
```

### Get Assignment By ID
```graphql
query {
  querySmartService
  assignmentById(id: "00000000-0000-0000-0000-000000000001") {
    id
    serviceRequestId
    agentId
    estimatedCost
    assignedAt
    completedAt
  }
}
```

### Get Assignments By ServiceRequest
```graphql
query {
  querySmartService
  assignmentsByServiceRequestId(serviceRequestId: "00000000-0000-0000-0000-000000000001") {
    id
    serviceRequestId
    agentId
    estimatedCost
    assignedAt
    completedAt
  }
}
```

### Get Assignments By Agent
```graphql
query {
  querySmartService
  assignmentsByAgentId(agentId: "00000000-0000-0000-0000-000000000001") {
    id
    serviceRequestId
    agentId
    estimatedCost
    assignedAt
    completedAt
  }
}
```

---

## 🧪 **7. AGENT CAPABILITY QUERIES - agentCapabilities**

### Get All AgentCapabilities
```graphql
query {
  querySmartService
  agentCapabilities {
    id
    agentId
    categoryId
    maxComplexity
    createdAt
  }
}
```

### Get AgentCapability By ID
```graphql
query {
  querySmartService
  agentCapabilityById(id: "00000000-0000-0000-0000-000000000001") {
    id
    agentId
    categoryId
    maxComplexity
    createdAt
  }
}
```

### Get Capabilities By Agent
```graphql
query {
  querySmartService
  capabilitiesByAgentId(agentId: "00000000-0000-0000-0000-000000000001") {
    id
    agentId
    categoryId
    maxComplexity
    createdAt
  }
}
```

### Get Capabilities By Category
```graphql
query {
  querySmartService
  capabilitiesByCategoryId(categoryId: "00000000-0000-0000-0000-000000000001") {
    id
    agentId
    categoryId
    maxComplexity
    createdAt
  }
}
```

---

## 🧪 **8. MATCHING RESULT QUERIES - matchingResults**

### Get All MatchingResults
```graphql
query {
  querySmartService
  matchingResults {
    id
    serviceRequestId
    serviceAgentId
    supportedComplexity
    matchingScore
    isRecommended
    createdAt
  }
}
```

### Get MatchingResult By ID
```graphql
query {
  querySmartService
  matchingResultById(id: "00000000-0000-0000-0000-000000000001") {
    id
    serviceRequestId
    serviceAgentId
    supportedComplexity
    matchingScore
    isRecommended
    createdAt
  }
}
```

### Get Matching Results By ServiceRequest
```graphql
query {
  querySmartService
  matchingResultsByServiceRequestId(serviceRequestId: "00000000-0000-0000-0000-000000000001") {
    id
    serviceRequestId
    serviceAgentId
    supportedComplexity
    matchingScore
    isRecommended
    createdAt
  }
}
```

### Get Recommended Matches
```graphql
query {
  querySmartService
  recommendedMatches(serviceRequestId: "00000000-0000-0000-0000-000000000001") {
    id
    serviceRequestId
    serviceAgentId
    supportedComplexity
    matchingScore
    isRecommended
    createdAt
  }
}
```

---

## 🧪 **9. SERVICE ATTACHMENT QUERIES - serviceAttachments**

### Get All ServiceAttachments
```graphql
query {
  querySmartService
  serviceAttachments {
    id
    serviceRequestId
    fileName
    fileUrl
    type
    uploadedAt
    createdAt
  }
}
```

### Get ServiceAttachment By ID
```graphql
query {
  querySmartService
  serviceAttachmentById(id: "00000000-0000-0000-0000-000000000001") {
    id
    serviceRequestId
    fileName
    fileUrl
    type
    uploadedAt
    createdAt
  }
}
```

### Get Attachments By ServiceRequest
```graphql
query {
  querySmartService
  attachmentsByServiceRequestId(serviceRequestId: "00000000-0000-0000-0000-000000000001") {
    id
    serviceRequestId
    fileName
    fileUrl
    type
    uploadedAt
    createdAt
  }
}
```

---

## 🧪 **10. ACTIVITY LOG QUERIES - activityLogs**

### Get All ActivityLogs
```graphql
query {
  querySmartService
  activityLogs {
    id
    serviceRequestId
    action
    createdAt
    updatedAt
  }
}
```

### Get ActivityLog By ID
```graphql
query {
  querySmartService
  activityLogById(id: "00000000-0000-0000-0000-000000000001") {
    id
    serviceRequestId
    action
    createdAt
    updatedAt
  }
}
```

### Get ActivityLogs By ServiceRequest
```graphql
query {
  querySmartService
  activityLogsByServiceRequestId(serviceRequestId: "00000000-0000-0000-0000-000000000001") {
    id
    serviceRequestId
    action
    createdAt
    updatedAt
  }
}
```

---

## 🧪 **11. SERVICE FEEDBACK QUERIES - serviceFeedbacks**

### Get All ServiceFeedbacks
```graphql
query {
  querySmartService
  serviceFeedbacks {
    id
    serviceRequestId
    createdByUserId
    rating
    comment
    createdAt
    updatedAt
  }
}
```

### Get ServiceFeedback By ID
```graphql
query {
  querySmartService
  serviceFeedbackById(id: "00000000-0000-0000-0000-000000000001") {
    id
    serviceRequestId
    createdByUserId
    rating
    comment
    createdAt
    updatedAt
  }
}
```

### Get Feedback By ServiceRequest
```graphql
query {
  querySmartService
  feedbackByServiceRequestId(serviceRequestId: "00000000-0000-0000-0000-000000000001") {
    id
    serviceRequestId
    createdByUserId
    rating
    comment
    createdAt
    updatedAt
  }
}
```

### Get Feedback By User
```graphql
query {
  querySmartService
  feedbackByUserId(userId: "00000000-0000-0000-0000-000000000001") {
    id
    serviceRequestId
    createdByUserId
    rating
    comment
    createdAt
    updatedAt
  }
}
```

### Get Average Rating
```graphql
query {
  querySmartService
  averageRatingByServiceRequestId(serviceRequestId: "00000000-0000-0000-0000-000000000001")
}
```

---

## 🔥 **TEST ALL AT ONCE (Combined Query)**

```graphql
query {
  querySmartService
  serviceRequests {
    id
  }
  users {
    id
  }
  serviceAgents {
    id
  }
  serviceCategories {
    id
  }
  assignments {
    id
  }
  agentCapabilities {
    id
  }
  matchingResults {
    id
  }
  serviceAttachments {
    id
  }
  activityLogs {
    id
  }
  serviceFeedbacks {
    id
  }
}
```

---

## 📝 **DANH SÁCH TẤT CẢ QUERY NAMES**

| # | Query Name | From | Method |
|---|---|---|---|
| 1 | `querySmartService` | Query Root | Health Check |
| 2 | `serviceRequests` | ServiceRequestQuery | Get All |
| 3 | `serviceRequestById` | ServiceRequestQuery | Get By ID |
| 4 | `serviceRequestsByCustomerId` | ServiceRequestQuery | Filter |
| 5 | `serviceRequestsByStatus` | ServiceRequestQuery | Filter |
| 6 | `users` | UserQuery | Get All |
| 7 | `userById` | UserQuery | Get By ID |
| 8 | `usersByRole` | UserQuery | Filter |
| 9 | `serviceAgents` | ServiceAgentQuery | Get All |
| 10 | `serviceAgentById` | ServiceAgentQuery | Get By ID |
| 11 | `activeServiceAgents` | ServiceAgentQuery | Filter |
| 12 | `serviceCategories` | ServiceCategoryQuery | Get All |
| 13 | `serviceCategoryById` | ServiceCategoryQuery | Get By ID |
| 14 | `assignments` | AssignmentQuery | Get All |
| 15 | `assignmentById` | AssignmentQuery | Get By ID |
| 16 | `assignmentsByServiceRequestId` | AssignmentQuery | Filter |
| 17 | `assignmentsByAgentId` | AssignmentQuery | Filter |
| 18 | `agentCapabilities` | AgentCapabilityQuery | Get All |
| 19 | `agentCapabilityById` | AgentCapabilityQuery | Get By ID |
| 20 | `capabilitiesByAgentId` | AgentCapabilityQuery | Filter |
| 21 | `capabilitiesByCategoryId` | AgentCapabilityQuery | Filter |
| 22 | `matchingResults` | MatchingResultQuery | Get All |
| 23 | `matchingResultById` | MatchingResultQuery | Get By ID |
| 24 | `matchingResultsByServiceRequestId` | MatchingResultQuery | Filter |
| 25 | `recommendedMatches` | MatchingResultQuery | Filter |
| 26 | `serviceAttachments` | ServiceAttachmentQuery | Get All |
| 27 | `serviceAttachmentById` | ServiceAttachmentQuery | Get By ID |
| 28 | `attachmentsByServiceRequestId` | ServiceAttachmentQuery | Filter |
| 29 | `activityLogs` | ActivityLogQuery | Get All |
| 30 | `activityLogById` | ActivityLogQuery | Get By ID |
| 31 | `activityLogsByServiceRequestId` | ActivityLogQuery | Filter |
| 32 | `serviceFeedbacks` | ServiceFeedbackQuery | Get All |
| 33 | `serviceFeedbackById` | ServiceFeedbackQuery | Get By ID |
| 34 | `feedbackByServiceRequestId` | ServiceFeedbackQuery | Filter |
| 35 | `feedbackByUserId` | ServiceFeedbackQuery | Filter |
| 36 | `averageRatingByServiceRequestId` | ServiceFeedbackQuery | Calculate |

---

## ⚠️ **NOTES**

- Tất cả query names được viết **camelCase** (lowercase chữ cái đầu) và **không có `get` prefix**
- `querySmartService` là query từ **Query root class** (HealthCheck)
- Các query khác là từ **Query Extensions** (tên file query)
- Thay `"00000000-0000-0000-0000-000000000001"` bằng GUID thực tế
- Nếu trả về `[]` hoặc `null` → cần seed data vào database
- GraphQL UI có **IntelliSense** - gõ xong gọi `Ctrl+Space` để gợi ý

