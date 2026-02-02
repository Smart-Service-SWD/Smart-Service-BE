# New Business Flows Documentation

## Overview
This document describes the 6 new business flows added to the Smart Service Platform to complete the service request lifecycle management.

---

## 1. Start Service Request Flow

### Description
Allows an assigned service provider (agent) to begin work on a service request.

### Endpoint
```
PATCH /api/service-requests/{serviceRequestId}/start
```

### Business Rules
- Service request must be in **Assigned** status
- Only assigned providers can start the request
- Status transitions: `Assigned` → `InProgress`

### Use Case Example
1. Staff assigns a plumbing request to Agent John
2. Agent John arrives at the location
3. Agent John calls the start endpoint to begin work
4. System records the start time and changes status to InProgress

### Domain Method
```csharp
public void Start()
{
    if (Status != ServiceStatus.Assigned)
        throw new DomainException("Service request must be assigned.");
    
    Status = ServiceStatus.InProgress;
}
```

---

## 2. Complete Service Request Flow

### Description
Marks a service request as completed after the work is finished.

### Endpoint
```
PATCH /api/service-requests/{serviceRequestId}/complete
```

### Business Rules
- Service request must be in **InProgress** status
- Completion triggers eligibility for customer feedback
- Status transitions: `InProgress` → `Completed`

### Use Case Example
1. Agent finishes the plumbing repair
2. Agent calls the complete endpoint
3. System marks the request as completed
4. Customer receives notification to provide feedback

### Domain Method
```csharp
public void Complete()
{
    if (Status != ServiceStatus.InProgress)
        throw new DomainException("Service request must be in progress.");
    
    Status = ServiceStatus.Completed;
}
```

---

## 3. Cancel Service Request Flow

### Description
Allows customers or staff to cancel a service request with a reason.

### Endpoint
```
PATCH /api/service-requests/{serviceRequestId}/cancel
```

### Request Body
```json
{
  "cancellationReason": "Customer changed their mind"
}
```

### Business Rules
- Cannot cancel a **Completed** request
- Cannot cancel an already **Cancelled** request
- Cancellation reason is mandatory (max 500 characters)
- Cancellation reason is stored for audit purposes

### Use Case Examples
1. **Customer cancellation**: Customer realizes they don't need the service anymore
2. **Staff cancellation**: Service cannot be fulfilled due to lack of qualified agents
3. **Provider cancellation**: Provider is no longer available

### Domain Method
```csharp
public void Cancel(string reason)
{
    if (Status == ServiceStatus.Completed)
        throw new DomainException("Cannot cancel a completed service request.");
    
    if (Status == ServiceStatus.Cancelled)
        throw new DomainException("Service request is already cancelled.");
    
    if (string.IsNullOrWhiteSpace(reason))
        throw new DomainException("Cancellation reason is required.");
    
    CancellationReason = reason;
    Status = ServiceStatus.Cancelled;
}
```

---

## 4. Update Service Request Flow

### Description
Allows modification of a service request's description before it has been assigned to a provider.

### Endpoint
```
PATCH /api/service-requests/{serviceRequestId}/update
```

### Request Body
```json
{
  "description": "Updated description with more details about the issue"
}
```

### Business Rules
- Can only update requests in **Created**, **PendingReview**, or **Approved** status
- Cannot update once the request has been **Assigned** or later
- Description is required and limited to 2000 characters

### Use Case Example
1. Customer creates a request: "Fix sink"
2. Customer realizes they need to add details
3. Customer calls update endpoint with: "Fix leaking kitchen sink, water pooling under cabinet"
4. System updates the description before any assignment

### Domain Method
```csharp
public void Update(string description)
{
    if (Status == ServiceStatus.Assigned || 
        Status == ServiceStatus.InProgress || 
        Status == ServiceStatus.Completed)
        throw new DomainException("Cannot update service request after it has been assigned.");
    
    if (string.IsNullOrWhiteSpace(description))
        throw new DomainException("Description is required.");
    
    Description = description;
}
```

---

## 5. Approve Service Request Flow

### Description
Allows staff to approve a service request that has been evaluated and is pending review.

### Endpoint
```
PATCH /api/service-requests/{serviceRequestId}/approve
```

### Business Rules
- Service request must be in **PendingReview** status
- Approval is typically done by staff after verifying the complexity evaluation
- Status transitions: `PendingReview` → `Approved`
- After approval, the request is ready for agent assignment

### Use Case Example
1. Customer creates a complex electrical repair request
2. System or staff evaluates complexity as Level 4
3. Staff reviews the evaluation and approves it
4. Request is now ready for qualified electricians to be matched

### Domain Method
```csharp
public void Approve()
{
    if (Status != ServiceStatus.PendingReview)
        throw new DomainException("Service request must be pending review to approve.");
    
    Status = ServiceStatus.Approved;
}
```

---

## 6. Smart Agent Matching Flow

### Description
Implements an intelligent algorithm to find and rank suitable agents for a service request based on their capabilities and the request complexity.

### Endpoint
```
POST /api/service-requests/{serviceRequestId}/match-agents
```

### Response
```json
[
  {
    "agentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "agentName": "John Smith",
    "score": 100.0,
    "isRecommended": true,
    "reason": "Perfect complexity match"
  },
  {
    "agentId": "7ba95f64-5717-4562-b3fc-2c963f66afa9",
    "agentName": "Jane Doe",
    "score": 90.0,
    "isRecommended": true,
    "reason": "Slight overqualified"
  }
]
```

### Matching Algorithm

The algorithm considers:
1. **Category Match**: Agent must have capability for the request's service category
2. **Complexity Match**: Agent's max complexity level must meet or exceed request complexity
3. **Availability**: Only active agents are considered
4. **Score Calculation**: 
   - Perfect match (same complexity level) = 100 points
   - Each level of over-qualification reduces score by 10 points
   - Example: Level 5 agent for Level 3 request = 100 - (2 × 10) = 80 points

### Scoring Examples

| Request Complexity | Agent Max Complexity | Score | Recommended? | Reason |
|-------------------|---------------------|-------|--------------|--------|
| Level 3 | Level 3 | 100 | ✓ Yes | Perfect complexity match |
| Level 3 | Level 4 | 90 | ✓ Yes | Slight overqualified |
| Level 3 | Level 5 | 80 | ✓ Yes | Significantly overqualified |
| Level 3 | Level 2 | N/A | ✗ No | Not qualified (excluded) |

### Business Rules
- Request must have complexity evaluated before matching
- Only agents with matching category capability are considered
- Agents must be able to handle the request complexity level
- Results are sorted by score (best matches first)
- Recommended matches have score ≥ 80

### Use Case Example
1. Service request for "Complex electrical panel upgrade" (Category: Electrical, Complexity: Level 4)
2. Staff calls the match-agents endpoint
3. System finds:
   - Agent A: Max Level 4 electrical → Score 100 (perfect match)
   - Agent B: Max Level 5 electrical → Score 90 (overqualified)
   - Agent C: Max Level 3 electrical → Excluded (under-qualified)
   - Agent D: Max Level 5 plumbing → Excluded (wrong category)
4. Staff assigns the request to Agent A or B based on other factors (availability, cost, etc.)

### Algorithm Implementation
```csharp
var complexityDiff = capability.MaxComplexity.Level - serviceRequest.Complexity.Level;
var score = 100 - (complexityDiff * 10);
var isRecommended = score >= 80;
```

---

## Complete Service Request Lifecycle

```
Created → PendingReview → Approved → Assigned → InProgress → Completed
                                ↓                      ↓
                            Cancelled              Cancelled
```

### Status Flow with New Endpoints

1. **Created**: `POST /api/service-requests` (existing)
2. **PendingReview**: `PATCH /.../evaluate-complexity` (existing)
3. **Approved**: `PATCH /.../approve` ⭐ **NEW**
4. **Matching**: `POST /.../match-agents` ⭐ **NEW** (returns candidate agents)
5. **Assigned**: `PATCH /.../assign-provider` (existing)
6. **InProgress**: `PATCH /.../start` ⭐ **NEW**
7. **Completed**: `PATCH /.../complete` ⭐ **NEW**

At any point before completion:
- **Update**: `PATCH /.../update` ⭐ **NEW** (only before assignment)
- **Cancel**: `PATCH /.../cancel` ⭐ **NEW** (with reason)

---

## Integration Points

### With Existing Flows

1. **Activity Logging**: All new status changes should be logged via `CreateActivityLogCommand`
2. **Feedback Collection**: After completion, customers can use `CreateServiceFeedbackCommand`
3. **Assignment Tracking**: Matching results can be saved using `CreateMatchingResultCommand` before actual assignment

### Recommended Workflow

```
1. Customer creates request → Created
2. Staff/AI evaluates complexity → PendingReview
3. Staff approves → Approved
4. Staff runs matching algorithm → Returns ranked agents
5. Staff assigns best agent → Assigned
6. Agent starts work → InProgress
7. Agent completes work → Completed
8. Customer provides feedback → (Feedback stored)
```

---

## Authorization Considerations (Future Enhancement)

While not implemented in this version, recommended role-based access:

| Endpoint | Customer | Agent | Staff | Admin |
|----------|----------|-------|-------|-------|
| Start | ✗ | ✓ | ✓ | ✓ |
| Complete | ✗ | ✓ | ✓ | ✓ |
| Cancel | ✓ | ✓ | ✓ | ✓ |
| Update | ✓ | ✗ | ✓ | ✓ |
| Approve | ✗ | ✗ | ✓ | ✓ |
| Match Agents | ✗ | ✗ | ✓ | ✓ |

---

## API Usage Examples

### Example 1: Complete Service Request Lifecycle (Happy Path)

```bash
# 1. Create service request
POST /api/service-requests
{
  "customerId": "...",
  "categoryId": "...",
  "description": "Fix leaking sink"
}
# Response: 201 Created, returns serviceRequestId

# 2. Evaluate complexity
PATCH /api/service-requests/{id}/evaluate-complexity
{
  "complexity": { "level": 2 }
}
# Response: 204 No Content

# 3. Approve request
PATCH /api/service-requests/{id}/approve
# Response: 204 No Content

# 4. Match agents
POST /api/service-requests/{id}/match-agents
# Response: 200 OK, returns list of matching agents

# 5. Assign provider
PATCH /api/service-requests/{id}/assign-provider
{
  "providerId": "...",
  "estimatedCost": { "amount": 150, "currency": "USD" }
}
# Response: 204 No Content

# 6. Start work
PATCH /api/service-requests/{id}/start
# Response: 204 No Content

# 7. Complete work
PATCH /api/service-requests/{id}/complete
# Response: 204 No Content
```

### Example 2: Cancellation Scenario

```bash
# After creating and evaluating, customer changes mind
PATCH /api/service-requests/{id}/cancel
{
  "cancellationReason": "Found another provider with better pricing"
}
# Response: 204 No Content
```

### Example 3: Update Before Assignment

```bash
# After creation, customer realizes they need to add details
PATCH /api/service-requests/{id}/update
{
  "description": "Fix leaking kitchen sink. Water is pooling under the cabinet and seems to be coming from the drain pipe."
}
# Response: 204 No Content
```

---

## Database Schema Changes

### ServiceRequest Table - New Column

```sql
ALTER TABLE ServiceRequests 
ADD COLUMN CancellationReason NVARCHAR(500) NULL;
```

This column stores the reason when a service request is cancelled. It's nullable because only cancelled requests will have a value.

---

## Validation Rules Summary

| Command | Field | Validation |
|---------|-------|------------|
| Start | ServiceRequestId | Required, GUID |
| Complete | ServiceRequestId | Required, GUID |
| Cancel | ServiceRequestId | Required, GUID |
| Cancel | CancellationReason | Required, max 500 chars |
| Update | ServiceRequestId | Required, GUID |
| Update | Description | Required, max 2000 chars |
| Approve | ServiceRequestId | Required, GUID |
| MatchAgents | ServiceRequestId | Required, GUID |

---

## Error Scenarios

### Common Domain Exceptions

1. **"Service request not found"** - Invalid ID provided
2. **"Service request must be assigned"** - Trying to start before assignment
3. **"Service request must be in progress"** - Trying to complete before starting
4. **"Cannot cancel a completed service request"** - Completed requests are immutable
5. **"Service request is already cancelled"** - Duplicate cancellation attempt
6. **"Cannot update service request after it has been assigned"** - Update too late
7. **"Service request must be pending review to approve"** - Wrong status for approval
8. **"Service request must have complexity evaluated before matching agents"** - Matching requires evaluation

---

## Performance Considerations

### Agent Matching Algorithm
- Current implementation loads all active agents into memory
- For large agent pools (>1000 agents), consider:
  1. Database-level filtering before loading
  2. Caching of agent capabilities
  3. Pagination of results
  4. Async processing for large datasets

### Indexing Recommendations
```sql
CREATE INDEX IX_ServiceRequests_Status ON ServiceRequests(Status);
CREATE INDEX IX_ServiceAgents_IsActive ON ServiceAgents(IsActive);
CREATE INDEX IX_AgentCapabilities_CategoryId ON AgentCapabilities(CategoryId);
```

---

## Future Enhancements

1. **Notification System**: Trigger notifications on status changes
2. **Event Publishing**: Emit domain events for each lifecycle change
3. **Advanced Matching**: Incorporate agent ratings, location, availability calendar
4. **Bulk Operations**: Cancel/Update multiple requests at once
5. **Scheduled Start**: Allow agents to schedule future start times
6. **Partial Completion**: Support work sessions and partial completions
7. **Approval Workflow**: Multi-level approval for high-complexity requests
8. **Auto-Assignment**: Automatically assign top-matched agent based on rules

---

## Testing Checklist

- [ ] Start request in Assigned status → Success
- [ ] Start request in Created status → Fail with error
- [ ] Complete request in InProgress status → Success
- [ ] Complete request in Assigned status → Fail with error
- [ ] Cancel request with reason → Success, reason stored
- [ ] Cancel completed request → Fail with error
- [ ] Cancel already cancelled request → Fail with error
- [ ] Update request before assignment → Success
- [ ] Update request after assignment → Fail with error
- [ ] Approve request in PendingReview → Success
- [ ] Approve request in Created → Fail with error
- [ ] Match agents for request with complexity → Returns ranked list
- [ ] Match agents for request without complexity → Fail with error
- [ ] Matching excludes agents without category capability
- [ ] Matching excludes agents with insufficient complexity level
- [ ] Matching excludes inactive agents
- [ ] Matching scores are calculated correctly
- [ ] Validation errors for empty/invalid inputs

---

## Conclusion

These 6 new business flows complete the core service request lifecycle management in the Smart Service Platform. They provide:

1. **Complete Lifecycle Control**: From creation to completion or cancellation
2. **Flexible Updates**: Customers can modify requests before commitment
3. **Approval Workflow**: Staff oversight for quality control
4. **Intelligent Matching**: Data-driven agent selection
5. **Audit Trail**: Cancellation reasons for transparency
6. **Domain-Driven Design**: All business rules encapsulated in domain entities

The implementation follows Clean Architecture principles, uses CQRS pattern, and maintains consistency with the existing codebase.
