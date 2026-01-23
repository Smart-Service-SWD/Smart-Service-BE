using HotChocolate.Authorization;

namespace SmartService.API.GraphQL;

/// <summary>
/// Root GraphQL query type.
/// All query extensions will be merged into this type.
/// </summary>
[Authorize]
public class Query
{
    public string QuerySmartService() => "SmartService GraphQL is running";
}
