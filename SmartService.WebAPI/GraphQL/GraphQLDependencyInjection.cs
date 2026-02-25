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
            .AddQueryType<Query>() // Root Query
            // Note: Mutations removed - Auth is now handled via REST API
            .AddType<ServiceRequestQuery>()
            .AddType<UserQuery>()
            .AddType<ServiceAgentQuery>()
            .AddType<ServiceCategoryQuery>()
            .AddType<AssignmentQuery>()
            .AddType<AgentCapabilityQuery>()
            .AddType<MatchingResultQuery>()
            .AddType<ServiceAttachmentQuery>()
            .AddType<ActivityLogQuery>()
            .AddType<ServiceFeedbackQuery>()
            .AddType<DashboardQuery>()
            .AddType<ServiceDefinitionQuery>()
            .AddAuthorization() // Enable authorization directive
            .AddHttpRequestInterceptor<HttpRequestInterceptor>() // Ensure HttpContext.User is available
            .ModifyRequestOptions(opt =>
            {
                opt.IncludeExceptionDetails = true;
            });

        return services;
    }
}
