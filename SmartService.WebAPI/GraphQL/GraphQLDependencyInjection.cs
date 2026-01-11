using Microsoft.Extensions.DependencyInjection;
using SmartService.API.GraphQL.Queries;

namespace SmartService.API.GraphQL;

public static class GraphQLDependencyInjection
{
    public static IServiceCollection AddGraphQLServices(
        this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddQueryType<ServiceRequestQuery>()
            .ModifyRequestOptions(opt =>
            {
                opt.IncludeExceptionDetails = true;
            });

        return services;
    }
}
