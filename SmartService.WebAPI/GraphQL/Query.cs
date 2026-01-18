namespace SmartService.API.GraphQL;

/// <summary>
/// Root GraphQL query type.
/// All query extensions will be merged into this type.
/// </summary>
public class Query
{
    public string QuerySmartService() => "SmartService GraphQL is running";
}
