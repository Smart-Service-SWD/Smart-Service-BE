# AI-Driven Matching System & Real-time Notifications

## 📋 Tổng quan

Hệ thống SmartService sử dụng AI để tự động phân tích yêu cầu dịch vụ và đưa ra cảnh báo an toàn ngay lập tức cho khách hàng thông qua SignalR. Hệ thống được thiết kế với kiến trúc mở rộng, sẵn sàng tích hợp tính năng địa chỉ (Location) trong tương lai.

---

## 🤖 AI Analysis System

### Luồng xử lý

1. **Customer tạo ServiceRequest**
   - Status tự động set thành `AwaitingAnalysis`
   - Request được lưu vào database ngay lập tức
   - API trả về `202 Accepted` (async processing)

2. **BackgroundService tự động phân tích**
   - Chạy ngầm, polling mỗi 5 giây
   - Tìm các request có status `AwaitingAnalysis`
   - Gọi AI (Ollama/Qwen2.5-7B) để phân tích mô tả

3. **AI trả về kết quả**
   ```json
   {
     "complexity": 1-5,
     "urgency": 1-5,
     "summary": "...",
     "safetyAdvice": "Ngắt cầu dao ngay!"
   }
   ```

4. **Lưu kết quả vào ServiceAnalysis**
   - Tách biệt hoàn toàn với ServiceRequest (DDD)
   - Lưu Complexity, Urgency, SafetyAdvice, Summary

5. **Cập nhật status ServiceRequest**
   - `Urgency >= 4` → Status = `UrgentDispatch` (ưu tiên cao)
   - `Urgency < 4` → Status = `Created` (bình thường)

### Entity: ServiceAnalysis

```csharp
public class ServiceAnalysis
{
    public Guid Id { get; private set; }
    public Guid ServiceRequestId { get; private set; }
    public int ComplexityLevel { get; private set; }      // 1-5
    public int UrgencyLevel { get; private set; }         // 1-5
    public string? SafetyAdvice { get; private set; }     // Lời khuyên an toàn
    public string? Summary { get; private set; }          // Tóm tắt vấn đề
    public bool IsCritical => UrgencyLevel >= 4;          // Thẻ [NGUY CẤP]
    public DateTime AnalyzedAt { get; private set; }
}
```

### BackgroundService

- **File**: `SmartService.Infrastructure/BackgroundServices/ServiceRequestAnalysisBackgroundService.cs`
- **Chức năng**: Xử lý async, polling mỗi 5 giây
- **Xử lý**: Tối đa 10 requests mỗi lần để tránh quá tải
- **Error handling**: Log lỗi nhưng không dừng service

---

## 📡 SignalR Real-time Notifications

### Hub: ServiceRequestHub

**Endpoint**: `/hubs/service-request`

**Methods**:
- `JoinServiceRequestGroup(Guid serviceRequestId)` - Client join group để nhận updates
- `LeaveServiceRequestGroup(Guid serviceRequestId)` - Client rời group

### Event: SafetyAdviceReceived

Sau khi AI phân tích xong, hệ thống tự động push event này về client:

```javascript
// Client-side example
connection.on("SafetyAdviceReceived", function (data) {
    console.log("Safety Advice:", data.safetyAdvice);
    console.log("Urgency Level:", data.urgencyLevel);
    console.log("Is Critical:", data.isCritical);
    
    // Hiển thị cảnh báo ngay lập tức cho user
    showSafetyAlert(data.safetyAdvice, data.isCritical);
});
```

**Payload**:
```json
{
  "serviceRequestId": "guid",
  "safetyAdvice": "Ngắt cầu dao ngay!",
  "urgencyLevel": 4,
  "isCritical": true
}
```

### Implementation

- **Interface**: `IServiceRequestNotificationService` (Application layer)
- **Implementation**: `SignalRServiceRequestNotificationService` (WebAPI layer)
- **Sử dụng**: BackgroundService gọi `SendSafetyAdviceAsync()` sau khi AI phân tích xong

---

## 🗺️ Location Features (Skeleton Mode)

### Hiện trạng

Tính năng địa chỉ đã được **dựng sẵn khung (skeleton)** nhưng chưa kích hoạt để tránh lỗi runtime:

### ServiceRequest Entity

```csharp
public class ServiceRequest
{
    // Đã có sẵn
    public string? AddressText { get; private set; }  // Địa chỉ dạng text
    
    // Skeleton (commented out - chưa kích hoạt)
    // public double? Latitude { get; set; }
    // public double? Longitude { get; set; }
}
```

### DistanceCalculator Service

**File**: `SmartService.Domain/Services/DistanceCalculator.cs`

```csharp
public static class DistanceCalculator
{
    public static double CalculateDistance(double? lat1, double? lon1, double? lat2, double? lon2)
    {
        // Skeleton mode: return 0 if coordinates not available
        if (!lat1.HasValue || !lon1.HasValue || !lat2.HasValue || !lon2.HasValue)
            return 0;

        // TODO: Uncomment when ready to enable location features
        /*
        const double R = 6371; // Earth radius in kilometers
        var dLat = ToRadians(lat2.Value - lat1.Value);
        var dLon = ToRadians(lon2.Value - lon1.Value);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1.Value)) * Math.Cos(ToRadians(lat2.Value)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
        */
        
        return 0; // Placeholder
    }
}
```

### ServiceAgent Entity

```csharp
public class ServiceAgent
{
    // Skeleton (commented out - chưa kích hoạt)
    // public double DefaultLat { get; set; }  // Vị trí thường trực
    // public double DefaultLng { get; set; }
}
```

---

## 🚀 Kế hoạch triển khai Location Features

### Bước 1: Kích hoạt Lat/Lng trong ServiceRequest

1. **Uncomment** các field trong `ServiceRequest.cs`:
   ```csharp
   public double? Latitude { get; set; }
   public double? Longitude { get; set; }
   ```

2. **Cập nhật CreateServiceRequestCommand** để nhận Lat/Lng từ frontend

3. **Tạo migration** để thêm columns vào database

### Bước 2: Kích hoạt Lat/Lng trong ServiceAgent

1. **Uncomment** các field trong `ServiceAgent.cs`:
   ```csharp
   public double DefaultLat { get; set; }
   public double DefaultLng { get; set; }
   ```

2. **Tạo migration** để thêm columns

### Bước 3: Kích hoạt DistanceCalculator

1. **Uncomment** code tính toán Haversine trong `DistanceCalculator.cs`

2. **Sử dụng trong Matching Logic**:
   ```csharp
   var distance = DistanceCalculator.CalculateDistance(
       request.Latitude, request.Longitude,
       agent.DefaultLat, agent.DefaultLng
   );
   ```

### Bước 4: Tích hợp Geocoding API (Optional)

- Sử dụng Google Maps Geocoding API hoặc OpenStreetMap
- Convert `AddressText` → `Latitude/Longitude` tự động
- Lưu vào database khi tạo ServiceRequest

### Bước 5: Matching với Distance

- Ưu tiên agents gần nhất (distance nhỏ nhất)
- Kết hợp với Complexity matching hiện có
- Hiển thị distance trong UI cho customer

---

## 📊 Workflow Diagram

```
┌─────────────┐
│  Customer   │
│  Tạo Request│
└──────┬──────┘
       │
       ▼
┌─────────────────────┐
│ ServiceRequest      │
│ Status: Awaiting... │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│ BackgroundService   │
│ (Polling mỗi 5s)    │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│ AI Analyzer         │
│ (Ollama/Qwen2.5)    │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│ ServiceAnalysis     │
│ + Update Status     │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│ SignalR Hub         │
│ Push SafetyAdvice   │
└──────┬──────────────┘
       │
       ▼
┌─────────────┐
│   Client    │
│  Nhận Alert │
└─────────────┘
```

---

## 🔧 Technical Details

### BackgroundService Configuration

- **Polling Interval**: 5 giây
- **Batch Size**: 10 requests mỗi lần
- **Error Handling**: Log và continue, không dừng service

### SignalR Configuration

- **Hub Path**: `/hubs/service-request`
- **Group Naming**: `ServiceRequest_{serviceRequestId}`
- **Event Name**: `SafetyAdviceReceived`

### AI Integration

- **Model**: Qwen2.5-7B-Instruct-1M (via Ollama)
- **Input**: Service description
- **Output**: JSON với Complexity, Urgency, SafetyAdvice, Summary
- **Knowledge Base**: JSON rules trong `Infrastructure/KnowledgeBase/`

---

## 📝 API Endpoints

### Create ServiceRequest

```http
POST /api/service-requests
Content-Type: application/json

{
  "customerId": "guid",
  "categoryId": "guid",
  "description": "Mô tả sự cố...",
  "addressText": "123 Đường ABC, Quận 1, TP.HCM"  // Optional
}
```

**Response**: `202 Accepted` với `serviceRequestId`

### SignalR Connection

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/service-request")
    .build();

// Join group để nhận updates
await connection.invoke("JoinServiceRequestGroup", serviceRequestId);

// Listen for safety advice
connection.on("SafetyAdviceReceived", (data) => {
    // Handle safety advice
});
```

---

## 🎯 Next Steps (Location Features)

1. ✅ **Skeleton đã sẵn sàng** - Code đã được comment sẵn
2. ⏳ **Uncomment Lat/Lng fields** - Khi sẵn sàng tích hợp
3. ⏳ **Tạo migration** - Thêm columns vào database
4. ⏳ **Tích hợp Geocoding** - Convert AddressText → Coordinates
5. ⏳ **Update Matching Logic** - Ưu tiên agents gần nhất
6. ⏳ **UI Updates** - Hiển thị map và distance

---

## 📚 Files Reference

- **ServiceAnalysis Entity**: `SmartService.Domain/Entities/ServiceAnalysis.cs`
- **BackgroundService**: `SmartService.Infrastructure/BackgroundServices/ServiceRequestAnalysisBackgroundService.cs`
- **SignalR Hub**: `SmartService.WebAPI/Hubs/ServiceRequestHub.cs`
- **Notification Service**: `SmartService.WebAPI/Notifications/SignalRServiceRequestNotificationService.cs`
- **Distance Calculator**: `SmartService.Domain/Services/DistanceCalculator.cs`
- **AI Analyzer**: `SmartService.Infrastructure/AI/Ollama/OllamaAiAnalyzer.cs`

---

## ⚠️ Lưu ý

- **Location features** hiện đang ở **skeleton mode** - không ảnh hưởng đến logic hiện tại
- Khi sẵn sàng kích hoạt, chỉ cần **uncomment code** và tạo migration
- **DistanceCalculator** hiện trả về `0` khi chưa có tọa độ - an toàn cho production
- **BackgroundService** tự động retry nếu có lỗi, không dừng service
