using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using SmartService.Domain.Entities;
using SmartService.Infrastructure.Persistence;

namespace SmartService.API.GraphQL.Types;

public class AssignmentType : ObjectType<Assignment>
{
    protected override void Configure(IObjectTypeDescriptor<Assignment> descriptor)
    {
        descriptor.Field("serviceRequest")
            .ResolveWith<AssignmentResolvers>(r => r.GetServiceRequest(default!, default!))
            .Description("Lấy thông tin yêu cầu dịch vụ liên quan đến phân công này.");
        
        // Ensure other fields are mapped correctly
        descriptor.Field(x => x.Id).Type<NonNullType<IdType>>();
        descriptor.Field(x => x.ServiceRequestId).Type<NonNullType<IdType>>();
        descriptor.Field(x => x.AgentId).Type<NonNullType<IdType>>();
        descriptor.Field(x => x.AssignedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(x => x.EstimatedCost).Type<NonNullType<MoneyType>>();
    }

    private class AssignmentResolvers
    {
        public async Task<ServiceRequest?> GetServiceRequest(
            [Parent] Assignment assignment,
            [Service] IDbContextFactory<AppDbContext> factory)
        {
            using var db = await factory.CreateDbContextAsync();
            return await db.ServiceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(sr => sr.Id == assignment.ServiceRequestId);
        }
    }
}

public class MoneyType : ObjectType<SmartService.Domain.ValueObjects.Money>
{
    protected override void Configure(IObjectTypeDescriptor<SmartService.Domain.ValueObjects.Money> descriptor)
    {
        descriptor.Field(x => x.Amount).Type<NonNullType<DecimalType>>();
        descriptor.Field(x => x.Currency).Type<NonNullType<StringType>>();
    }
}
