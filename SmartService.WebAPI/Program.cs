using SmartService.API.GraphQL;
using SmartService.Application;
using SmartService.Application.Abstractions.AI;
using SmartService.Application.UseCases.AnalyzeServiceRequest;
using SmartService.Infrastructure;
using SmartService.Infrastructure.AI.Ollama;
using SmartService.Infrastructure.KnowledgeBase.Complexity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddGraphQLServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SmartService API",
        Version = "v1",
        Description = "API quản lý dịch vụ thông minh với CQRS pattern"
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Enable annotations
    c.EnableAnnotations();
    
    // Group endpoints by tags from SwaggerOperation attribute
    c.TagActionsBy(api =>
    {
        var tags = api.ActionDescriptor.EndpointMetadata
            .OfType<Swashbuckle.AspNetCore.Annotations.SwaggerOperationAttribute>()
            .FirstOrDefault()?.Tags;
        
        if (tags != null && tags.Length > 0)
        {
            return tags;
        }
        
        // Fallback to controller name if no tags in SwaggerOperation
        return new[] { api.ActionDescriptor.RouteValues["controller"] };
    });
});

builder.Services.AddHttpClient<OllamaClient>();
builder.Services.AddScoped<AnalyzeServiceRequestHandler>();


builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.


app.UseHttpsRedirection();
app.MapControllers(); 
app.MapGraphQL();

app.Run();

