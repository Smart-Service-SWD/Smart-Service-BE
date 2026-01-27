# SMART SERVICE ORCHESTRATION PLATFORM
## Mô Tả Hệ Thống và Thiết Kế Giao Diện

---

## 📋 TỔNG QUAN HỆ THỐNG

**Smart Service Orchestration Platform** là một hệ thống quản lý và điều phối dịch vụ thông minh, được thiết kế để:
- Tiếp nhận và quản lý yêu cầu dịch vụ từ khách hàng
- Đánh giá độ phức tạp của dịch vụ bằng AI và quy tắc nghiệp vụ
- Tự động khớp yêu cầu với nhà cung cấp dịch vụ phù hợp
- Theo dõi toàn bộ vòng đời dịch vụ từ tạo yêu cầu đến hoàn thành

**Điểm khác biệt:** Không phải hệ thống đặt lịch đơn giản, mà là một **orchestration engine** thông minh có khả năng ra quyết định tự động dựa trên độ phức tạp, kỹ năng và chứng chỉ của nhà cung cấp.

---

## 🏗️ KIẾN TRÚC HỆ THỐNG

### Clean Architecture - 4 Layers

#### 1. **Domain Layer** (Lõi nghiệp vụ)
- **Entities:** ServiceRequest (Aggregate Root), User, ServiceAgent, ServiceCategory, Assignment, MatchingResult, ServiceFeedback, ActivityLog, AgentCapability, ServiceAttachment
- **Value Objects:** ServiceComplexity (1-5), ServiceStatus (Created → PendingReview → Assigned → InProgress → Completed), Money
- **Business Rules:** Tất cả logic nghiệp vụ được đóng gói trong domain entities

#### 2. **Application Layer** (Use Cases)
- **CQRS Pattern:** Tách biệt Command (Write) và Query (Read)
- **MediatR:** Xử lý commands/queries thông qua pipeline
- **Features:** ServiceRequests, Users, ServiceAgents, Assignments, MatchingResults, ServiceFeedbacks, ActivityLogs
- **AI Integration:** AnalyzeServiceRequestHandler - phân tích mô tả dịch vụ bằng AI

#### 3. **Infrastructure Layer** (Technical Concerns)
- **Persistence:** Entity Framework Core + PostgreSQL
- **AI Service:** Ollama AI Analyzer (sử dụng Qwen2.5-7B model)
- **Knowledge Base:** JSON rules cho complexity và pricing theo từng category
- **Categories:** Technical (Electric, IT), Real Estate (Brokerage, Legal, Valuation), Legal (Civil Law)

#### 4. **WebAPI Layer** (Presentation)
- **REST API:** Commands (POST, PATCH) cho write operations
- **GraphQL API:** Queries cho read operations
- **Swagger:** API documentation

---

## 🔄 LUỒNG VẬN HÀNH CHI TIẾT

### **LUỒNG 1: Tạo Yêu Cầu Dịch Vụ (Customer Journey)**

```
1. Customer đăng nhập vào hệ thống
   ↓
2. Chọn Service Category (ví dụ: Technical > Electric, Real Estate > Legal Consulting)
   ↓
3. Nhập mô tả dịch vụ (Description) - có thể kèm file đính kèm
   ↓
4. [OPTIONAL] Customer có thể tự đánh giá Complexity Level (1-5)
   - Nếu có: Status = PendingReview
   - Nếu không: Status = Created
   ↓
5. Hệ thống tạo ServiceRequest với:
   - CustomerId
   - CategoryId
   - Description
   - Complexity (nullable)
   - Status
   - CreatedAt
   ↓
6. [AUTO] Hệ thống có thể gọi AI Analysis để đánh giá sơ bộ
   ↓
7. Customer nhận thông báo: "Yêu cầu đã được tạo thành công"
```

### **LUỒNG 2: Phân Tích AI Tự Động (AI Analysis Flow)**

```
1. Staff hoặc System trigger AI Analysis
   ↓
2. Gửi Description đến /api/service-analysis
   ↓
3. AI Analyzer (Ollama) xử lý:
   - Đọc Knowledge Base rules (ví dụ: electric_complexity.json)
   - Phân tích keywords trong description
   - So khớp với criteria trong rules
   ↓
4. AI trả về kết quả:
   {
     "Complexity": 1-5,
     "UserMessage": {
       "Summary": "Tóm tắt dịch vụ",
       "RiskExplanation": "Giải thích rủi ro",
       "SafetyAdvice": "Lời khuyên an toàn"
     },
     "DispatchRules": {
       "RequiredSkillLevel": 1-5,
       "MinExperienceYears": số năm,
       "RequiresCertification": true/false,
       "RequiresSeniorTechnician": true/false,
       "RiskWeight": 0.0-1.0
     }
   }
   ↓
5. Hệ thống cập nhật ServiceRequest.Complexity (nếu chưa có)
   ↓
6. Status chuyển sang PendingReview
```

### **LUỒNG 3: Đánh Giá Độ Phức Tạp (Staff Evaluation)**

```
1. Staff xem danh sách ServiceRequest có Status = Created
   ↓
2. Staff chọn một request để đánh giá
   ↓
3. Staff có thể:
   - Xem AI Analysis result (nếu có)
   - Xem Description và Attachments
   - Xem Category và rules liên quan
   ↓
4. Staff đánh giá Complexity Level (1-5):
   - Level 1: Rất đơn giản
   - Level 2: Đơn giản
   - Level 3: Trung bình
   - Level 4: Phức tạp
   - Level 5: Rất phức tạp
   ↓
5. Gọi PATCH /api/service-requests/{id}/evaluate-complexity
   ↓
6. ServiceRequest.Evaluate(complexity) được gọi
   - Validation: Status phải = Created
   - Cập nhật Complexity
   - Status → PendingReview
   ↓
7. Hệ thống tạo ActivityLog: "Complexity evaluated"
```

### **LUỒNG 4: Khớp và Gán Nhà Cung Cấp (Matching & Assignment)**

```
1. Hệ thống tìm ServiceAgents phù hợp:
   - Agent có AgentCapability với CategoryId khớp
   - Agent.MaxComplexity >= ServiceRequest.Complexity
   - Agent.IsActive = true
   ↓
2. Tạo MatchingResults cho mỗi agent phù hợp:
   - ServiceRequestId
   - ServiceAgentId
   - SupportedComplexity
   - MatchingScore (tính toán dựa trên capability, experience, rating)
   - IsRecommended (true/false)
   ↓
3. Staff xem danh sách MatchingResults:
   - Sắp xếp theo MatchingScore
   - Highlight các agent được recommend
   - Hiển thị thông tin: Name, Capabilities, Experience, Rating
   ↓
4. Staff chọn một Agent và nhập EstimatedCost
   ↓
5. Gọi PATCH /api/service-requests/{id}/assign-provider
   {
     "ProviderId": Guid,
     "EstimatedCost": {
       "Amount": decimal,
       "Currency": "VND" | "USD"
     }
   }
   ↓
6. ServiceRequest.AssignProvider(providerId, estimatedCost):
   - Validation: Status phải = PendingReview
   - Cập nhật AssignedProviderId
   - Cập nhật EstimatedCost
   - Status → Assigned
   ↓
7. Tạo Assignment record:
   - ServiceRequestId
   - AgentId
   - EstimatedCost
   - AssignedAt
   ↓
8. Tạo ActivityLog: "Provider assigned"
   ↓
9. Gửi thông báo cho Agent và Customer
```

### **LUỒNG 5: Thực Hiện Dịch Vụ (Service Execution)**

```
1. Agent đăng nhập và xem danh sách Assignment
   ↓
2. Agent chọn một ServiceRequest có Status = Assigned
   ↓
3. Agent bắt đầu làm việc:
   - Xem Description, Attachments, Complexity
   - Xem EstimatedCost
   - Có thể upload thêm attachments (progress photos, documents)
   ↓
4. Agent bấm "Start Service"
   - Gọi API để update Status → InProgress
   - ServiceRequest.Start() được gọi
   - Validation: Status phải = Assigned
   ↓
5. Agent thực hiện công việc:
   - Có thể cập nhật progress notes
   - Upload completion photos/documents
   ↓
6. Agent hoàn thành và bấm "Complete Service"
   - Gọi API để update Status → Completed
   - ServiceRequest.Complete() được gọi
   - Validation: Status phải = InProgress
   ↓
7. Tạo ActivityLog: "Service completed"
   ↓
8. Hệ thống gửi thông báo cho Customer
```

### **LUỒNG 6: Đánh Giá và Phản Hồi (Feedback Flow)**

```
1. Customer nhận thông báo: "Dịch vụ đã hoàn thành"
   ↓
2. Customer xem ServiceRequest details:
   - Description
   - Agent information
   - Timeline (Created → Assigned → InProgress → Completed)
   - Attachments (before/after photos)
   ↓
3. Customer đánh giá:
   - Rating: 1-5 sao
   - Comment (optional)
   ↓
4. Tạo ServiceFeedback:
   - ServiceRequestId
   - CreatedByUserId (Customer)
   - Rating
   - Comment
   - CreatedAt
   ↓
5. Feedback được lưu và hiển thị cho Agent và Staff
```

---

## 🎨 Ý TƯỞNG THIẾT KẾ GIAO DIỆN

### **NGUYÊN TẮC THIẾT KẾ**

1. **Role-Based Dashboard:** Mỗi role (Customer, Staff, Agent, Admin) có dashboard riêng
2. **Status-Driven UI:** Giao diện thay đổi theo ServiceStatus
3. **Real-time Updates:** Hiển thị thay đổi status và notifications
4. **AI-Powered Insights:** Hiển thị kết quả phân tích AI một cách trực quan
5. **Workflow Visualization:** Timeline/kanban board cho service lifecycle

### **CẤU TRÚC GIAO DIỆN ĐỀ XUẤT**

#### **1. CUSTOMER DASHBOARD**

**Layout:**
```
┌─────────────────────────────────────────────────┐
│  Header: Logo | Notifications | Profile         │
├─────────────────────────────────────────────────┤
│  Sidebar:                                      │
│  - Tạo yêu cầu mới                              │
│  - Yêu cầu của tôi                              │
│  - Lịch sử                                      │
│  - Phản hồi                                     │
├─────────────────────────────────────────────────┤
│  Main Content:                                  │
│  ┌──────────────────────────────────────────┐  │
│  │  TẠO YÊU CẦU DỊCH VỤ                     │  │
│  │  ┌────────────────────────────────────┐  │  │
│  │  │ 1. Chọn danh mục                   │  │  │
│  │  │    [Dropdown: Technical/Real Estate│  │  │
│  │  │     /Legal]                         │  │  │
│  │  │    → Sub-category: [Electric/IT...] │  │  │
│  │  ├────────────────────────────────────┤  │  │
│  │  │ 2. Mô tả dịch vụ                   │  │  │
│  │  │    [Textarea: Nhập mô tả chi tiết] │  │  │
│  │  │    [Button: Phân tích bằng AI]     │  │  │
│  │  ├────────────────────────────────────┤  │  │
│  │  │ 3. Đính kèm (tùy chọn)             │  │  │
│  │  │    [Upload: Images/Documents]      │  │  │
│  │  ├────────────────────────────────────┤  │  │
│  │  │ 4. Đánh giá độ phức tạp (tùy chọn) │  │  │
│  │  │    [Slider: 1 ──────●────── 5]    │  │  │
│  │  │    Level: 3                        │  │  │
│  │  └────────────────────────────────────┘  │  │
│  │  [Button: Tạo yêu cầu]                   │  │
│  └──────────────────────────────────────────┘  │
│                                                  │
│  ┌──────────────────────────────────────────┐  │
│  │  YÊU CẦU CỦA TÔI                         │  │
│  │  [Filter: Tất cả | Đang xử lý | Hoàn thành]│
│  │  ┌──────┬──────────┬──────────┬────────┐  │
│  │  │ ID   │ Category │ Status   │ Actions│  │
│  │  ├──────┼──────────┼──────────┼────────┤  │
│  │  │ #001 │ Electric │ Assigned │ [View] │  │
│  │  │ #002 │ Legal    │ Completed│[Review]│  │
│  │  └──────┴──────────┴──────────┴────────┘  │
│  └──────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
```

**Chi tiết Service Request View (Customer):**
```
┌─────────────────────────────────────────────────┐
│  ← Back to List                                  │
├─────────────────────────────────────────────────┤
│  YÊU CẦU DỊCH VỤ #001                           │
│  ┌──────────────────────────────────────────┐  │
│  │ Status Badge: [🟡 Assigned]             │  │
│  │ Category: Technical > Electric           │  │
│  │ Created: 23/01/2026 10:30 AM            │  │
│  ├──────────────────────────────────────────┤  │
│  │ MÔ TẢ:                                   │  │
│  │ "Cần sửa chữa hệ thống điện 3 pha..."    │  │
│  ├──────────────────────────────────────────┤  │
│  │ ĐỘ PHỨC TẠP:                             │  │
│  │ [████████░░] Level 4 (Phức tạp)         │  │
│  ├──────────────────────────────────────────┤  │
│  │ PHÂN TÍCH AI (nếu có):                   │  │
│  │ ┌────────────────────────────────────┐  │  │
│  │ │ 📋 Tóm tắt:                        │  │  │
│  │ │ "Yêu cầu sửa chữa hệ thống điện..."│  │  │
│  │ ├────────────────────────────────────┤  │  │
│  │ │ ⚠️ Rủi ro:                         │  │  │
│  │ │ "Có nguy cơ điện giật cao..."      │  │  │
│  │ ├────────────────────────────────────┤  │  │
│  │ │ ✅ Lời khuyên:                     │  │  │
│  │ │ "Cần thợ có chứng chỉ điện lực..."│  │  │
│  │ └────────────────────────────────────┘  │  │
│  ├──────────────────────────────────────────┤  │
│  │ NHÀ CUNG CẤP ĐƯỢC GÁN:                   │  │
│  │ ┌────────────────────────────────────┐  │  │
│  │ │ 👤 Nguyễn Văn A                     │  │  │
│  │ │ ⭐ 4.8/5 (120 đánh giá)             │  │  │
│  │ │ 📞 0901234567                       │  │  │
│  │ └────────────────────────────────────┘  │  │
│  ├──────────────────────────────────────────┤  │
│  │ CHI PHÍ ƯỚC TÍNH:                       │  │
│  │ 💰 2,500,000 VND                        │  │
│  ├──────────────────────────────────────────┤  │
│  │ TIMELINE:                                │  │
│  │ ● Created ────● Assigned ────○ InProgress│  │
│  │   23/01 10:30   23/01 14:20              │  │
│  ├──────────────────────────────────────────┤  │
│  │ ĐÍNH KÈM:                                │  │
│  │ [Image 1] [Image 2] [Document.pdf]      │  │
│  └──────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
```

#### **2. STAFF DASHBOARD**

**Layout:**
```
┌─────────────────────────────────────────────────┐
│  Header: Logo | Notifications | Profile         │
├─────────────────────────────────────────────────┤
│  Sidebar:                                      │
│  - Dashboard                                   │
│  - Yêu cầu cần đánh giá                        │
│  - Yêu cầu đã gán                              │
│  - Quản lý Agents                              │
│  - Báo cáo                                     │
├─────────────────────────────────────────────────┤
│  Main Content:                                  │
│  ┌──────────────────────────────────────────┐  │
│  │ DASHBOARD TỔNG QUAN                      │  │
│  │ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐     │  │
│  │ │ 15   │ │ 8    │ │ 12   │ │ 45   │     │  │
│  │ │ Created│ │ Pending│ │ Assigned│ │ Total│     │  │
│  │ └──────┘ └──────┘ └──────┘ └──────┘     │  │
│  └──────────────────────────────────────────┘  │
│                                                  │
│  ┌──────────────────────────────────────────┐  │
│  │ YÊU CẦU CẦN ĐÁNH GIÁ (Status = Created)  │  │
│  │ [Kanban Board]                            │  │
│  │ ┌────────┐ ┌────────┐ ┌────────┐        │  │
│  │ │ Created│ │ Pending│ │ Assigned│        │  │
│  │ │        │ │ Review │ │        │        │  │
│  │ │ [Card] │ │ [Card] │ │ [Card] │        │  │
│  │ │ [Card] │ │ [Card] │ │        │        │  │
│  │ └────────┘ └────────┘ └────────┘        │  │
│  └──────────────────────────────────────────┘  │
│                                                  │
│  ┌──────────────────────────────────────────┐  │
│  │ CHI TIẾT YÊU CẦU (khi click vào card)     │  │
│  │ ┌────────────────────────────────────┐  │  │
│  │ │ [Tabs: Thông tin | AI Analysis |    │  │  │
│  │ │  Matching Results | Timeline]      │  │  │
│  │ │                                     │  │  │
│  │ │ Tab: AI Analysis                   │  │  │
│  │ │ ┌────────────────────────────────┐ │  │  │
│  │ │ │ AI Đề xuất: Level 4            │ │  │  │
│  │ │ │ Lý do: "Có từ khóa '3 pha'..." │ │  │  │
│  │ │ │ Required Skill: 4               │ │  │  │
│  │ │ │ Min Experience: 5 years         │ │  │  │
│  │ │ │ Requires Certification: Yes     │ │  │  │
│  │ │ │ Risk Weight: 0.8                 │ │  │  │
│  │ │ └────────────────────────────────┘ │  │  │
│  │ │                                     │  │  │
│  │ │ [Button: Chấp nhận AI] [Button:    │  │  │
│  │ │  Đánh giá thủ công]                 │  │  │
│  │ │                                     │  │  │
│  │ │ Đánh giá thủ công:                  │  │  │
│  │ │ [Slider: 1 ──────●────── 5]       │  │  │
│  │ │ [Button: Lưu đánh giá]             │  │  │
│  │ └────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────┘  │
│                                                  │
│  ┌──────────────────────────────────────────┐  │
│  │ MATCHING RESULTS (Tab)                    │  │
│  │ ┌────────────────────────────────────┐  │  │
│  │ │ Tìm thấy 5 agents phù hợp:          │  │  │
│  │ │ ┌────────────────────────────────┐  │  │  │
│  │ │ │ ⭐ RECOMMENDED                 │  │  │  │
│  │ │ │ 👤 Nguyễn Văn A                │  │  │  │
│  │ │ │ Match Score: 95%               │  │  │  │
│  │ │ │ Capabilities: Electric (Level 5)│  │  │  │
│  │ │ │ Experience: 8 years             │  │  │  │
│  │ │ │ Rating: 4.8/5                   │  │  │  │
│  │ │ │ [Button: Gán agent này]        │  │  │  │
│  │ │ └────────────────────────────────┘  │  │  │
│  │ │ ┌────────────────────────────────┐  │  │  │
│  │ │ │ 👤 Trần Văn B                   │  │  │  │
│  │ │ │ Match Score: 82%                │  │  │  │
│  │ │ │ ...                             │  │  │  │
│  │ │ └────────────────────────────────┘  │  │  │
│  │ └────────────────────────────────────┘  │  │
│  │                                          │  │
│  │ Gán Agent:                               │  │
│  │ [Dropdown: Chọn agent]                  │  │
│  │ Chi phí ước tính:                        │  │
│  │ [Input: Amount] [Dropdown: VND/USD]     │  │
│  │ [Button: Xác nhận gán]                   │  │
│  └──────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
```

#### **3. AGENT DASHBOARD**

**Layout:**
```
┌─────────────────────────────────────────────────┐
│  Header: Logo | Notifications | Profile         │
├─────────────────────────────────────────────────┤
│  Sidebar:                                      │
│  - Dashboard                                   │
│  - Yêu cầu được gán                            │
│  - Yêu cầu đang thực hiện                      │
│  - Lịch sử                                     │
│  - Thống kê                                    │
├─────────────────────────────────────────────────┤
│  Main Content:                                  │
│  ┌──────────────────────────────────────────┐  │
│  │ YÊU CẦU ĐƯỢC GÁN (Status = Assigned)      │  │
│  │ ┌────────────────────────────────────┐  │  │
│  │ │ Service Request #001              │  │  │
│  │ │ Category: Electric                │  │  │
│  │ │ Complexity: Level 4               │  │  │
│  │ │ Customer: Nguyễn Văn C            │  │  │
│  │ │ Estimated Cost: 2,500,000 VND     │  │  │
│  │ │ [Button: Bắt đầu]                 │  │  │
│  │ └────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────┘  │
│                                                  │
│  ┌──────────────────────────────────────────┐  │
│  │ YÊU CẦU ĐANG THỰC HIỆN (Status = InProgress)│
│  │ ┌────────────────────────────────────┐  │  │
│  │ │ Service Request #002               │  │  │
│  │ │ Progress: [████████░░] 80%        │  │  │
│  │ │                                     │  │  │
│  │ │ [Tabs: Chi tiết | Ghi chú | Files] │  │  │
│  │ │                                     │  │  │
│  │ │ Ghi chú tiến độ:                   │  │  │
│  │ │ [Textarea: Đã hoàn thành phần...]  │  │  │
│  │ │                                     │  │  │
│  │ │ Upload files:                       │  │  │
│  │ │ [Upload: Progress photos]          │  │  │
│  │ │                                     │  │  │
│  │ │ [Button: Hoàn thành dịch vụ]       │  │  │
│  │ └────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
```

---

## 🎯 CÁC TÍNH NĂNG GIAO DIỆN QUAN TRỌNG

### **1. AI Analysis Display**
- **Card hiển thị kết quả AI:** Summary, Risk, Safety Advice
- **Visual indicators:** Màu sắc theo risk level (green/yellow/red)
- **Action buttons:** "Chấp nhận AI" hoặc "Đánh giá thủ công"
- **Confidence score:** Hiển thị độ tin cậy của AI (nếu có)

### **2. Status Workflow Visualization**
- **Timeline view:** Hiển thị các bước trong lifecycle
- **Kanban board:** Drag & drop giữa các status (cho Staff)
- **Status badges:** Màu sắc phân biệt (Created=gray, Assigned=blue, InProgress=orange, Completed=green)

### **3. Matching Results Display**
- **Recommended badge:** Highlight agents được AI/system recommend
- **Match score visualization:** Progress bar hoặc stars
- **Filter & Sort:** Theo score, experience, rating
- **Quick assign:** One-click assign với default estimated cost

### **4. Complexity Visualization**
- **Level indicator:** 1-5 với màu sắc (1=green, 5=red)
- **Progress bar:** Visual representation
- **Tooltip:** Giải thích từng level khi hover

### **5. Real-time Notifications**
- **Toast notifications:** Khi status thay đổi
- **Badge counter:** Số lượng yêu cầu mới cần xử lý
- **Email/SMS integration:** (Future feature)

### **6. File Management**
- **Image gallery:** Hiển thị attachments dạng grid
- **Preview modal:** Xem ảnh/document full screen
- **Upload progress:** Progress bar khi upload

---

## 📱 RESPONSIVE DESIGN

- **Desktop:** Full dashboard với sidebar
- **Tablet:** Collapsible sidebar, responsive cards
- **Mobile:** Bottom navigation, simplified views

---

## 🎨 COLOR SCHEME ĐỀ XUẤT

- **Primary:** Blue (#2563EB) - Trust, Professional
- **Success:** Green (#10B981) - Completed, Success
- **Warning:** Orange (#F59E0B) - In Progress, Pending
- **Danger:** Red (#EF4444) - High Risk, Cancelled
- **Info:** Cyan (#06B6D4) - Information, AI Insights
- **Background:** Light Gray (#F9FAFB)
- **Text:** Dark Gray (#1F2937)

---

## 🔌 API ENDPOINTS CẦN CHO FRONTEND

### **REST API (Commands)**
- `POST /api/service-requests` - Tạo yêu cầu
- `PATCH /api/service-requests/{id}/evaluate-complexity` - Đánh giá complexity
- `PATCH /api/service-requests/{id}/assign-provider` - Gán provider
- `POST /api/service-analysis` - Phân tích AI
- `POST /api/service-feedbacks` - Tạo feedback

### **GraphQL (Queries)**
- `query serviceRequests` - Lấy danh sách yêu cầu
- `query serviceRequest(id)` - Lấy chi tiết yêu cầu
- `query matchingResults(serviceRequestId)` - Lấy kết quả matching
- `query serviceAgents` - Lấy danh sách agents
- `query activityLogs(serviceRequestId)` - Lấy lịch sử hoạt động

---

## 📝 PROMPT CHO GOOGLE GEMINI / AI DESIGN TOOL

**Prompt mẫu:**

```
Tôi cần thiết kế giao diện frontend cho một hệ thống quản lý dịch vụ thông minh với các yêu cầu sau:

1. **Hệ thống:** Smart Service Orchestration Platform - quản lý vòng đời dịch vụ từ tạo yêu cầu đến hoàn thành

2. **User Roles:** 
   - Customer: Tạo yêu cầu, xem tiến độ, đánh giá
   - Staff: Đánh giá complexity, gán agent, quản lý
   - Agent: Nhận assignment, thực hiện dịch vụ, cập nhật progress

3. **Workflow chính:**
   - Customer tạo yêu cầu → AI phân tích → Staff đánh giá → Khớp agent → Agent thực hiện → Hoàn thành → Feedback

4. **Tính năng đặc biệt:**
   - Hiển thị kết quả phân tích AI (Summary, Risk, Safety Advice)
   - Matching results với score và recommendations
   - Timeline visualization cho service lifecycle
   - Kanban board cho Staff quản lý requests
   - Complexity level visualization (1-5)

5. **Yêu cầu UI/UX:**
   - Modern, clean design
   - Role-based dashboards
   - Real-time status updates
   - Responsive (Desktop/Tablet/Mobile)
   - Color-coded status badges
   - Intuitive navigation

Hãy tạo wireframes và mockups chi tiết cho từng role và các màn hình chính.
```

---

## 📚 TÀI LIỆU THAM KHẢO

- **Backend API:** Swagger UI tại `/swagger`
- **GraphQL Playground:** `/graphql`
- **Domain Model:** Xem các file trong `SmartService.Domain/Entities`
- **API Controllers:** Xem `SmartService.WebAPI/Controllers`

---

**Tài liệu này có thể được sử dụng để:**
1. Prompt AI design tools (Google Gemini, ChatGPT, Midjourney, etc.)
2. Hướng dẫn frontend developers
3. Tài liệu tham khảo cho UX/UI designers
4. Documentation cho stakeholders
