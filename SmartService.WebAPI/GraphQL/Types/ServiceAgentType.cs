using HotChocolate;
using HotChocolate.Types;
using SmartService.Domain.Entities;

namespace SmartService.API.GraphQL.Types;

public class ServiceAgentType : ObjectType<ServiceAgent>
{
    protected override void Configure(IObjectTypeDescriptor<ServiceAgent> descriptor)
    {
        descriptor.Field(x => x.Id).Type<NonNullType<IdType>>();
        descriptor.Field(x => x.FullName).Type<NonNullType<StringType>>();
        descriptor.Field(x => x.IsActive).Type<NonNullType<BooleanType>>();
        descriptor.Field(x => x.UserId).Type<IdType>();
        
        descriptor.Field(x => x.Balance)
            .Type<NonNullType<MoneyType>>()
            .Description("Số dư trong ví của thợ.");
            
        descriptor.Field(x => x.Capabilities)
            .Description("Danh sách các kỹ năng/khả năng của thợ.");
    }
}
