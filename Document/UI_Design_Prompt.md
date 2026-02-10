# PROMPT CHO THIẾT KẾ GIAO DIỆN - SMART SERVICE PLATFORM

## 🎯 MÔ TẢ HỆ THỐNG (Dùng cho AI Design Tool)

### **Hệ thống là gì?**
Smart Service Orchestration Platform - một hệ thống quản lý và điều phối dịch vụ thông minh, tự động khớp yêu cầu khách hàng với nhà cung cấp dịch vụ phù hợp dựa trên độ phức tạp, kỹ năng và AI phân tích.

### **3 Loại người dùng chính:**

#### **1. CUSTOMER (Khách hàng)**
- **Mục đích:** Tạo yêu cầu dịch vụ, theo dõi tiến độ, đánh giá sau khi hoàn thành
- **Hành động chính:**
  - Chọn danh mục dịch vụ (Technical/Real Estate/Legal)
  - Nhập mô tả dịch vụ
  - Upload file đính kèm (ảnh, tài liệu)
  - Xem kết quả phân tích AI (nếu có)
  - Theo dõi status: Created → Assigned → InProgress → Completed
  - Xem thông tin agent được gán
  - Đánh giá và phản hồi sau khi hoàn thành

#### **2. STAFF (Nhân viên quản lý)**
- **Mục đích:** Đánh giá độ phức tạp, gán agent, quản lý workflow
- **Hành động chính:**
  - Xem danh sách yêu cầu cần đánh giá (Status = Created)
  - Xem kết quả phân tích AI (Summary, Risk, Safety Advice, Dispatch Rules)
  - Đánh giá độ phức tạp thủ công (Level 1-5) hoặc chấp nhận AI
  - Xem danh sách agents phù hợp (Matching Results với score)
  - Gán agent cho yêu cầu và nhập chi phí ước tính
  - Quản lý agents và capabilities

#### **3. AGENT (Nhà cung cấp dịch vụ)**
- **Mục đích:** Nhận assignment, thực hiện dịch vụ, cập nhật tiến độ
- **Hành động chính:**
  - Xem danh sách yêu cầu được gán (Status = Assigned)
  - Bắt đầu dịch vụ (Status → InProgress)
  - Cập nhật ghi chú tiến độ
  - Upload ảnh/document tiến độ
  - Hoàn thành dịch vụ (Status → Completed)
  - Xem feedback từ khách hàng

---

## 🔄 WORKFLOW CHÍNH

```
CUSTOMER                    STAFF                      AGENT
   │                          │                          │
   ├─ Tạo yêu cầu ────────────┼──────────────────────────┤
   │  (Description, Category) │                          │
   │                          │                          │
   │                          ├─ AI Phân tích ───────────┤
   │                          │  (Complexity, Risk,      │
   │                          │   Safety Advice)         │
   │                          │                          │
   │                          ├─ Đánh giá Complexity ────┤
   │                          │  (Level 1-5)            │
   │                          │                          │
   │                          ├─ Tìm Agents phù hợp ────┤
   │                          │  (Matching Results)     │
   │                          │                          │
   │                          ├─ Gán Agent ──────────────┤
   │                          │  (ProviderId, Cost)     │
   │                          │                          │
   │                          │                          ├─ Nhận Assignment
   │                          │                          │
   │                          │                          ├─ Bắt đầu (Start)
   │                          │                          │
   │                          │                          ├─ Thực hiện
   │                          │                          │  (Progress, Files)
   │                          │                          │
   │                          │                          ├─ Hoàn thành
   │                          │                          │
   ├─ Nhận thông báo ──────────┼──────────────────────────┤
   │                          │                          │
   ├─ Đánh giá & Feedback ────┼──────────────────────────┤
```

---

## 🎨 YÊU CẦU GIAO DIỆN

### **1. CUSTOMER DASHBOARD**

**Màn hình chính:**
- **Header:** Logo, Notifications (badge số), Profile dropdown
- **Sidebar:** Menu navigation (Tạo yêu cầu, Yêu cầu của tôi, Lịch sử, Phản hồi)
- **Main Content:**
  - **Form tạo yêu cầu:**
    - Step 1: Dropdown chọn Category (Technical > Electric/IT, Real Estate > Brokerage/Legal/Valuation, Legal > Civil Law)
    - Step 2: Textarea mô tả dịch vụ (có button "Phân tích bằng AI")
    - Step 3: Upload files (drag & drop hoặc click)
    - Step 4: Slider đánh giá complexity (1-5) - optional
    - Button "Tạo yêu cầu"
  
  - **Danh sách yêu cầu:**
    - Filter tabs: Tất cả | Đang xử lý | Hoàn thành
    - Table/Cards hiển thị: ID, Category, Status badge (màu sắc), Created date, Actions (View)
    - Click vào → Chi tiết yêu cầu

**Màn hình chi tiết yêu cầu:**
- **Status badge** (màu sắc): Created (gray), Assigned (blue), InProgress (orange), Completed (green)
- **Thông tin cơ bản:** Category, Description, Created date
- **AI Analysis card** (nếu có):
  - Summary (tóm tắt)
  - Risk Explanation (rủi ro) - màu vàng/đỏ
  - Safety Advice (lời khuyên) - màu xanh
- **Complexity indicator:** Progress bar 1-5 với màu sắc
- **Agent được gán:** Avatar, Tên, Rating (sao), Phone
- **Estimated Cost:** Số tiền lớn, rõ ràng
- **Timeline:** Horizontal timeline với các milestone (Created → Assigned → InProgress → Completed)
- **Attachments:** Image gallery grid, click để preview
- **Feedback form** (nếu Completed): Rating 1-5 sao, Comment textarea

### **2. STAFF DASHBOARD**

**Màn hình chính:**
- **Header:** Logo, Notifications, Profile
- **Sidebar:** Dashboard, Yêu cầu cần đánh giá, Yêu cầu đã gán, Quản lý Agents, Báo cáo
- **Main Content:**
  - **Dashboard overview:** 4 cards số liệu (Created, Pending Review, Assigned, Total)
  - **Kanban Board:** 3 columns (Created, Pending Review, Assigned) với cards có thể drag & drop
  - Mỗi card hiển thị: ID, Category, Customer name, Created date, Complexity (nếu có)

**Màn hình chi tiết yêu cầu (Staff):**
- **Tabs:** Thông tin | AI Analysis | Matching Results | Timeline
- **Tab AI Analysis:**
  - Card hiển thị kết quả AI:
    - Complexity Level đề xuất (1-5 với màu sắc)
    - Lý do chọn level (selectedLevelReason)
    - Dispatch Rules: Required Skill Level, Min Experience Years, Requires Certification, Risk Weight
  - Buttons: "Chấp nhận AI" | "Đánh giá thủ công"
  - Nếu chọn thủ công: Slider 1-5, Button "Lưu đánh giá"
  
- **Tab Matching Results:**
  - Danh sách agents phù hợp, sắp xếp theo Match Score
  - Mỗi agent card:
    - Badge "RECOMMENDED" (nếu IsRecommended = true)
    - Tên, Avatar
    - Match Score (progress bar %)
    - Capabilities (Category + Max Complexity)
    - Experience (số năm)
    - Rating (sao)
    - Button "Gán agent này"
  - Form gán agent:
    - Dropdown chọn agent (hoặc click từ list)
    - Input Estimated Cost (Amount + Currency dropdown)
    - Button "Xác nhận gán"

### **3. AGENT DASHBOARD**

**Màn hình chính:**
- **Header:** Logo, Notifications, Profile
- **Sidebar:** Dashboard, Yêu cầu được gán, Yêu cầu đang thực hiện, Lịch sử, Thống kê
- **Main Content:**
  - **Danh sách yêu cầu được gán (Assigned):**
    - Cards hiển thị: ID, Category, Complexity, Customer name, Estimated Cost
    - Button "Bắt đầu" → Status → InProgress
  
  - **Danh cầu đang thực hiện (InProgress):**
    - Cards với tabs: Chi tiết | Ghi chú | Files
    - Progress bar (manual hoặc tự động)
    - Textarea ghi chú tiến độ
    - Upload files (ảnh tiến độ, documents)
    - Button "Hoàn thành dịch vụ" → Status → Completed

---

## 🎨 DESIGN SYSTEM

### **Color Palette:**
- **Primary Blue:** #2563EB (Trust, Professional)
- **Success Green:** #10B981 (Completed, Success)
- **Warning Orange:** #F59E0B (In Progress, Pending)
- **Danger Red:** #EF4444 (High Risk, Cancelled)
- **Info Cyan:** #06B6D4 (AI Insights, Information)
- **Background:** #F9FAFB (Light Gray)
- **Text:** #1F2937 (Dark Gray)

### **Status Colors:**
- **Created:** Gray (#6B7280)
- **PendingReview:** Yellow (#F59E0B)
- **Assigned:** Blue (#2563EB)
- **InProgress:** Orange (#F59E0B)
- **Completed:** Green (#10B981)
- **Cancelled:** Red (#EF4444)

### **Complexity Level Colors:**
- **Level 1:** Green (#10B981) - Rất đơn giản
- **Level 2:** Light Green (#34D399) - Đơn giản
- **Level 3:** Yellow (#F59E0B) - Trung bình
- **Level 4:** Orange (#FB923C) - Phức tạp
- **Level 5:** Red (#EF4444) - Rất phức tạp

### **Typography:**
- **Headings:** Bold, 24-32px
- **Body:** Regular, 14-16px
- **Labels:** Medium, 12-14px
- **Font:** Inter hoặc Roboto (modern, clean)

### **Components:**
- **Cards:** Rounded corners (8-12px), Shadow (subtle)
- **Buttons:** Primary (filled), Secondary (outlined), Danger (red)
- **Badges:** Pill-shaped, small padding
- **Inputs:** Rounded, border on focus
- **Progress bars:** Animated, color-coded

---

## 📱 RESPONSIVE BREAKPOINTS

- **Desktop:** > 1024px (Full sidebar, multi-column layout)
- **Tablet:** 768px - 1024px (Collapsible sidebar, 2-column)
- **Mobile:** < 768px (Bottom navigation, single column, simplified cards)

---

## ✨ INTERACTIVE FEATURES

1. **Real-time Updates:** Status changes trigger notifications (toast)
2. **Drag & Drop:** Kanban board cho Staff
3. **Image Preview:** Modal khi click vào attachment
4. **AI Analysis Animation:** Loading state khi phân tích AI
5. **Timeline Interaction:** Click vào milestone để xem chi tiết
6. **Match Score Visualization:** Animated progress bar
7. **File Upload:** Drag & drop với preview

---

## 🎯 PRIORITY FEATURES (Must Have)

1. ✅ Role-based dashboards
2. ✅ Service request creation form
3. ✅ AI Analysis display (Summary, Risk, Safety)
4. ✅ Matching results với recommendations
5. ✅ Status workflow visualization (Timeline/Kanban)
6. ✅ Complexity level visualization
7. ✅ File upload & preview
8. ✅ Real-time status badges
9. ✅ Feedback/rating system

---

## 📝 PROMPT CHO AI DESIGN TOOL

**Copy và paste prompt này vào Google Gemini, ChatGPT, hoặc design tool:**

```
Tôi cần thiết kế giao diện frontend cho hệ thống Smart Service Orchestration Platform với các yêu cầu sau:

**Hệ thống:** Quản lý và điều phối dịch vụ thông minh, tự động khớp yêu cầu với nhà cung cấp dựa trên AI và độ phức tạp.

**3 User Roles:**
1. Customer: Tạo yêu cầu, theo dõi, đánh giá
2. Staff: Đánh giá complexity, gán agent, quản lý
3. Agent: Nhận assignment, thực hiện dịch vụ

**Workflow:** Customer tạo → AI phân tích → Staff đánh giá → Khớp agent → Agent thực hiện → Hoàn thành → Feedback

**Tính năng UI quan trọng:**
- AI Analysis card (Summary, Risk, Safety Advice)
- Matching results với score và recommendations
- Timeline/Kanban visualization
- Complexity level (1-5) với màu sắc
- Status badges (Created/Assigned/InProgress/Completed)
- File upload & preview
- Real-time notifications

**Design Style:** Modern, clean, professional. Color scheme: Blue primary, Green success, Orange warning, Red danger.

**Responsive:** Desktop (full sidebar), Tablet (collapsible), Mobile (bottom nav).

Hãy tạo wireframes và mockups chi tiết cho:
1. Customer Dashboard (Tạo yêu cầu, Danh sách, Chi tiết)
2. Staff Dashboard (Kanban board, AI Analysis, Matching Results)
3. Agent Dashboard (Assigned list, InProgress details)

Bao gồm: Layout structure, Component placement, Color usage, Interactive states, Responsive breakpoints.
```

---

**Tài liệu này được tối ưu để sử dụng trực tiếp với AI design tools như Google Gemini, ChatGPT, hoặc các công cụ thiết kế UI khác.**
