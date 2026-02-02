# Tài Liệu Các Luồng Nghiệp Vụ Mới - Smart Service Platform

## Tổng Quan

Dự án Smart Service Platform đã được bổ sung **6 luồng nghiệp vụ mới** để hoàn thiện vòng đời quản lý yêu cầu dịch vụ.

---

## 📋 Danh Sách Các Luồng Nghiệp Vụ Hiện Có

### Luồng Đã Có Từ Trước ✅
1. **Đăng ký & Đăng nhập** - Quản lý người dùng với JWT token
2. **Tạo yêu cầu dịch vụ** - Khách hàng tạo yêu cầu dịch vụ mới
3. **Đánh giá độ phức tạp** - Nhân viên hoặc AI đánh giá mức độ phức tạp (1-5)
4. **Gán nhà cung cấp** - Gán agent cho yêu cầu dịch vụ
5. **Phân tích AI** - Sử dụng Ollama để phân tích độ phức tạp
6. **Quản lý Agent** - Tạo agent và quản lý năng lực
7. **Phản hồi dịch vụ** - Khách hàng đánh giá sau khi hoàn thành

### Luồng Mới Được Phát Triển ⭐

## 1. Bắt Đầu Thực Hiện Dịch Vụ (Start Service Request)

**Endpoint:** `PATCH /api/service-requests/{id}/start`

**Mô tả:** Agent bắt đầu thực hiện công việc sau khi được gán.

**Quy tắc nghiệp vụ:**
- Chỉ có thể bắt đầu khi trạng thái là **Assigned** (Đã gán)
- Chuyển trạng thái từ `Assigned` → `InProgress`

**Ví dụ sử dụng:**
```bash
PATCH /api/service-requests/abc-123/start
# Không cần body
```

---

## 2. Hoàn Thành Dịch Vụ (Complete Service Request)

**Endpoint:** `PATCH /api/service-requests/{id}/complete`

**Mô tả:** Agent đánh dấu công việc đã hoàn thành.

**Quy tắc nghiệp vụ:**
- Chỉ có thể hoàn thành khi trạng thái là **InProgress** (Đang thực hiện)
- Chuyển trạng thái từ `InProgress` → `Completed`
- Sau khi hoàn thành, khách hàng có thể đánh giá

**Ví dụ sử dụng:**
```bash
PATCH /api/service-requests/abc-123/complete
# Không cần body
```

---

## 3. Hủy Yêu Cầu Dịch Vụ (Cancel Service Request)

**Endpoint:** `PATCH /api/service-requests/{id}/cancel`

**Mô tả:** Hủy yêu cầu dịch vụ với lý do cụ thể.

**Quy tắc nghiệp vụ:**
- **KHÔNG thể** hủy yêu cầu đã **Completed** (Hoàn thành)
- **KHÔNG thể** hủy yêu cầu đã bị hủy trước đó
- Bắt buộc phải có lý do hủy (tối đa 500 ký tự)
- Lý do hủy được lưu vào database để kiểm tra sau này

**Ví dụ sử dụng:**
```bash
PATCH /api/service-requests/abc-123/cancel
{
  "cancellationReason": "Khách hàng đổi ý, không cần dịch vụ nữa"
}
```

**Trường hợp sử dụng:**
- Khách hàng thay đổi kế hoạch
- Không tìm được agent phù hợp
- Agent không còn khả dụng

---

## 4. Cập Nhật Yêu Cầu Dịch Vụ (Update Service Request)

**Endpoint:** `PATCH /api/service-requests/{id}/update`

**Mô tả:** Cập nhật mô tả của yêu cầu dịch vụ.

**Quy tắc nghiệp vụ:**
- **CHỈ** có thể cập nhật trước khi được gán (trước Assigned)
- **KHÔNG thể** cập nhật sau khi đã Assigned, InProgress hoặc Completed
- Mô tả bắt buộc và tối đa 2000 ký tự

**Ví dụ sử dụng:**
```bash
PATCH /api/service-requests/abc-123/update
{
  "description": "Sửa vòi nước bếp bị rò rỉ, nước chảy xuống dưới tủ bếp"
}
```

**Trường hợp sử dụng:**
- Khách hàng muốn bổ sung thông tin chi tiết
- Khách hàng nhận ra thiếu thông tin quan trọng

---

## 5. Phê Duyệt Yêu Cầu Dịch Vụ (Approve Service Request)

**Endpoint:** `PATCH /api/service-requests/{id}/approve`

**Mô tả:** Nhân viên phê duyệt yêu cầu sau khi đánh giá độ phức tạp.

**Quy tắc nghiệp vụ:**
- Chỉ có thể phê duyệt khi trạng thái là **PendingReview** (Chờ xem xét)
- Chuyển trạng thái từ `PendingReview` → `Approved`
- Sau khi phê duyệt, có thể tiến hành gán agent

**Ví dụ sử dụng:**
```bash
PATCH /api/service-requests/abc-123/approve
# Không cần body
```

**Quy trình:**
1. Yêu cầu được đánh giá độ phức tạp → PendingReview
2. Nhân viên kiểm tra và xác nhận đánh giá
3. Nhân viên phê duyệt → Approved
4. Sẵn sàng để tìm và gán agent

---

## 6. Tìm Kiếm Agent Phù Hợp (Smart Agent Matching) ⭐⭐⭐

**Endpoint:** `POST /api/service-requests/{id}/match-agents`

**Mô tả:** Thuật toán tự động tìm kiếm và xếp hạng các agent phù hợp.

**Thuật toán matching:**
1. **Kiểm tra danh mục dịch vụ**: Agent phải có năng lực trong danh mục này
2. **Kiểm tra độ phức tạp**: Agent phải có khả năng xử lý mức độ phức tạp này
3. **Tính điểm**: 
   - Khớp hoàn hảo (cùng level) = 100 điểm
   - Mỗi level vượt quá giảm 10 điểm
4. **Xếp hạng**: Sắp xếp theo điểm từ cao xuống thấp

**Kết quả trả về:**
```json
[
  {
    "agentId": "guid-1",
    "agentName": "Nguyễn Văn A",
    "score": 100.0,
    "isRecommended": true,
    "reason": "Perfect complexity match"
  },
  {
    "agentId": "guid-2", 
    "agentName": "Trần Thị B",
    "score": 90.0,
    "isRecommended": true,
    "reason": "Slight overqualified"
  }
]
```

**Bảng tính điểm:**

| Độ Phức Tạp Yêu Cầu | Độ Phức Tạp Tối Đa Agent | Điểm | Đề Xuất? | Lý Do |
|---------------------|-------------------------|------|----------|-------|
| Level 3 | Level 3 | 100 | ✓ Có | Khớp hoàn hảo |
| Level 3 | Level 4 | 90 | ✓ Có | Hơi cao hơn |
| Level 3 | Level 5 | 80 | ✓ Có | Cao hơn đáng kể |
| Level 3 | Level 2 | N/A | ✗ Không | Không đủ năng lực |

**Quy tắc nghiệp vụ:**
- Yêu cầu **BẮT BUỘC** phải có độ phức tạp trước khi match
- Chỉ xét agent **đang hoạt động** (IsActive = true)
- Agent phải có năng lực trong **đúng danh mục** dịch vụ
- Kết quả được **đề xuất** (recommended) khi điểm ≥ 80

**Ví dụ thực tế:**

Yêu cầu: "Sửa chữa hệ thống điện phức tạp" (Danh mục: Điện, Độ phức tạp: Level 4)

1. **Agent A**: Điện - Max Level 4 → **Điểm 100** (Khớp hoàn hảo) ✓
2. **Agent B**: Điện - Max Level 5 → **Điểm 90** (Hơi cao) ✓
3. **Agent C**: Điện - Max Level 3 → **Loại** (Không đủ khả năng) ✗
4. **Agent D**: Ống nước - Max Level 5 → **Loại** (Sai danh mục) ✗

Kết quả: Nhân viên sẽ chọn Agent A hoặc B để gán.

---

## 🔄 Vòng Đời Hoàn Chỉnh Của Yêu Cầu Dịch Vụ

```
Created (Mới tạo)
    ↓
Evaluate Complexity (Đánh giá)
    ↓
PendingReview (Chờ xem xét)
    ↓
⭐ Approve (Phê duyệt) ⭐
    ↓
⭐ Match Agents (Tìm agent phù hợp) ⭐
    ↓
Assign Provider (Gán agent)
    ↓
Assigned (Đã gán)
    ↓
⭐ Start (Bắt đầu thực hiện) ⭐
    ↓
InProgress (Đang thực hiện)
    ↓
⭐ Complete (Hoàn thành) ⭐
    ↓
Completed (Đã hoàn thành)
    ↓
Give Feedback (Đánh giá)
```

**Các điểm có thể hủy:**
- Từ bất kỳ trạng thái nào (trừ Completed) → ⭐ Cancel (Hủy) ⭐

**Cập nhật mô tả:**
- Chỉ có thể ⭐ Update ⭐ trước khi Assigned

---

## 🎯 Quy Trình Sử Dụng Đầy Đủ

### Kịch Bản 1: Quy trình thành công hoàn chỉnh

```bash
# 1. Khách hàng tạo yêu cầu
POST /api/service-requests
{
  "customerId": "...",
  "categoryId": "...",
  "description": "Sửa vòi nước bị rò"
}

# 2. Đánh giá độ phức tạp
PATCH /api/service-requests/{id}/evaluate-complexity
{
  "complexity": { "level": 2 }
}

# 3. ⭐ Phê duyệt (MỚI)
PATCH /api/service-requests/{id}/approve

# 4. ⭐ Tìm agent phù hợp (MỚI)
POST /api/service-requests/{id}/match-agents
# Trả về danh sách agent với điểm số

# 5. Gán agent
PATCH /api/service-requests/{id}/assign-provider
{
  "providerId": "...",
  "estimatedCost": { "amount": 150000, "currency": "VND" }
}

# 6. ⭐ Agent bắt đầu làm việc (MỚI)
PATCH /api/service-requests/{id}/start

# 7. ⭐ Agent hoàn thành công việc (MỚI)
PATCH /api/service-requests/{id}/complete

# 8. Khách hàng đánh giá
POST /api/service-feedbacks
{
  "serviceRequestId": "...",
  "rating": 5,
  "comment": "Dịch vụ tốt!"
}
```

### Kịch Bản 2: Khách hàng hủy yêu cầu

```bash
# Sau khi tạo, khách hàng đổi ý
PATCH /api/service-requests/{id}/cancel
{
  "cancellationReason": "Tôi tìm được thợ khác rẻ hơn"
}
```

### Kịch Bản 3: Cập nhật mô tả trước khi gán

```bash
# Khách hàng nhận ra thiếu thông tin
PATCH /api/service-requests/{id}/update
{
  "description": "Sửa vòi nước bếp bị rò rỉ, nước chảy xuống dưới tủ và làm ướt sàn nhà"
}
```

---

## 📊 Tóm Tắt Các Thay Đổi Kỹ Thuật

### Domain Layer (SmartService.Domain)
**File:** `ServiceRequest.cs`

**Các phương thức domain mới:**
```csharp
// 1. Bắt đầu thực hiện
public void Start()

// 2. Hoàn thành
public void Complete() 

// 3. Hủy với lý do
public void Cancel(string reason)

// 4. Phê duyệt
public void Approve()

// 5. Cập nhật mô tả
public void Update(string description)
```

**Thuộc tính mới:**
```csharp
public string? CancellationReason { get; private set; }
```

### Application Layer (SmartService.Application)

**6 Commands mới được thêm:**
1. `StartServiceRequestCommand` + Handler + Validator
2. `CompleteServiceRequestCommand` + Handler + Validator
3. `CancelServiceRequestCommand` + Handler + Validator
4. `UpdateServiceRequestCommand` + Handler + Validator
5. `ApproveServiceRequestCommand` + Handler + Validator
6. `MatchAgentsForServiceRequestCommand` + Handler + Validator

### Infrastructure Layer

**Migration mới:**
- `AddCancellationReasonToServiceRequest` - Thêm cột CancellationReason vào bảng ServiceRequests

### API Layer (SmartService.WebAPI)

**6 endpoints mới:**
1. `PATCH /api/service-requests/{id}/start`
2. `PATCH /api/service-requests/{id}/complete`
3. `PATCH /api/service-requests/{id}/cancel`
4. `PATCH /api/service-requests/{id}/update`
5. `PATCH /api/service-requests/{id}/approve`
6. `POST /api/service-requests/{id}/match-agents`

---

## 🛡️ Validation Rules (Quy Tắc Kiểm Tra)

| Command | Trường | Quy Tắc |
|---------|--------|---------|
| Start | ServiceRequestId | Bắt buộc, phải là GUID |
| Complete | ServiceRequestId | Bắt buộc, phải là GUID |
| Cancel | ServiceRequestId | Bắt buộc, phải là GUID |
| Cancel | CancellationReason | Bắt buộc, tối đa 500 ký tự |
| Update | ServiceRequestId | Bắt buộc, phải là GUID |
| Update | Description | Bắt buộc, tối đa 2000 ký tự |
| Approve | ServiceRequestId | Bắt buộc, phải là GUID |
| MatchAgents | ServiceRequestId | Bắt buộc, phải là GUID |

---

## ⚠️ Các Lỗi Thường Gặp

1. **"Service request not found"** - ID không tồn tại
2. **"Service request must be assigned"** - Chưa gán agent, không thể start
3. **"Service request must be in progress"** - Chưa bắt đầu, không thể complete
4. **"Cannot cancel a completed service request"** - Đã hoàn thành, không thể hủy
5. **"Cannot update service request after it has been assigned"** - Đã gán, không thể cập nhật
6. **"Service request must be pending review to approve"** - Sai trạng thái, không thể phê duyệt
7. **"Service request must have complexity evaluated before matching agents"** - Chưa đánh giá độ phức tạp

---

## 📈 Lợi Ích Của Các Luồng Mới

### 1. Quản Lý Vòng Đời Hoàn Chỉnh
- Theo dõi từng bước từ tạo đến hoàn thành
- Ghi lại thời điểm bắt đầu và kết thúc thực tế

### 2. Tính Linh Hoạt
- Khách hàng có thể cập nhật hoặc hủy yêu cầu
- Lưu lý do hủy để phân tích sau này

### 3. Kiểm Soát Chất Lượng
- Quy trình phê duyệt bởi nhân viên
- Đảm bảo đánh giá độ phức tạp chính xác

### 4. Matching Thông Minh
- Tự động tìm agent phù hợp nhất
- Tránh gán sai người, sai năng lực
- Tiết kiệm thời gian tìm kiếm thủ công

### 5. Audit Trail (Dấu Vết Kiểm Tra)
- Lưu lý do hủy để phân tích xu hướng
- Theo dõi lịch sử thay đổi trạng thái

### 6. Tuân Thủ Clean Architecture
- Domain rules được bảo vệ trong Domain layer
- CQRS pattern cho Commands
- FluentValidation cho tất cả inputs

---

## 🔮 Các Tính Năng Có Thể Phát Triển Tiếp

1. **Hệ Thống Thông Báo**: Gửi thông báo khi có thay đổi trạng thái
2. **Event Publishing**: Phát sự kiện domain cho các hệ thống khác
3. **Matching Nâng Cao**: 
   - Kết hợp vị trí địa lý
   - Xét lịch trình của agent
   - Xét đánh giá của agent
4. **Tự Động Gán**: Tự động gán agent có điểm cao nhất
5. **Lịch Hẹn**: Agent hẹn giờ bắt đầu thay vì start ngay
6. **Thông Báo Qua Email/SMS**: Thông báo cho khách hàng và agent
7. **Dashboard Theo Dõi**: Biểu đồ trạng thái các yêu cầu
8. **Báo Cáo Thống Kê**: 
   - Tỷ lệ hủy theo lý do
   - Thời gian trung bình hoàn thành
   - Đánh giá agent theo category

---

## 📝 Tài Liệu Chi Tiết

Xem file **`NewBusinessFlows.md`** (bản tiếng Anh) để biết:
- Chi tiết kỹ thuật triển khai
- Ví dụ code minh họa
- Bảng so sánh tính năng
- Checklist testing đầy đủ

---

## 🎉 Kết Luận

Với **6 luồng nghiệp vụ mới** này, Smart Service Platform đã có đầy đủ các tính năng để quản lý vòng đời yêu cầu dịch vụ từ đầu đến cuối:

✅ **Tạo** yêu cầu  
✅ **Đánh giá** độ phức tạp  
✅ **Phê duyệt** yêu cầu  
✅ **Tìm kiếm** agent phù hợp  
✅ **Gán** agent  
✅ **Bắt đầu** thực hiện  
✅ **Hoàn thành** công việc  
✅ **Đánh giá** dịch vụ  
✅ **Cập nhật** thông tin  
✅ **Hủy** khi cần thiết  

Hệ thống giờ đây đã sẵn sàng để triển khai và sử dụng trong môi trường thực tế! 🚀
