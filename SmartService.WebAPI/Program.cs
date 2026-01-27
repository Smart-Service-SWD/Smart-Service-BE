using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SmartService.API.GraphQL;
using SmartService.Application;
using SmartService.Application.Abstractions.AI;
using SmartService.Application.Abstractions.Auth;
using SmartService.Application.UseCases.AnalyzeServiceRequest;
using SmartService.Infrastructure;
using SmartService.Infrastructure.AI.Ollama;
using SmartService.Infrastructure.KnowledgeBase.Complexity;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<TokenConfiguration>();

// Configure default authentication scheme
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    if (jwtSettings != null)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
        
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.NoResult();
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader) && string.IsNullOrEmpty(context.Token))
                {
                    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Token = authHeader.Substring("Bearer ".Length).Trim();
                    }
                    else
                    {
                        context.Token = authHeader.Trim();
                    }
                }
                return Task.CompletedTask;
            }
        };
    }
});

builder.Services.AddAuthorization();

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

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.EnableAnnotations();
    
    c.TagActionsBy(api =>
    {
        var tags = api.ActionDescriptor.EndpointMetadata
            .OfType<Swashbuckle.AspNetCore.Annotations.SwaggerOperationAttribute>()
            .FirstOrDefault()?.Tags;
        
        if (tags != null && tags.Length > 0)
        {
            return tags;
        }
        
        return new[] { api.ActionDescriptor.RouteValues["controller"] };
    });

    c.OrderActionsBy(apiDesc =>
    {
        var tag = apiDesc.ActionDescriptor.EndpointMetadata
            .OfType<Swashbuckle.AspNetCore.Annotations.SwaggerOperationAttribute>()
            .FirstOrDefault()?.Tags?.FirstOrDefault()
            ?? apiDesc.GroupName ?? apiDesc.ActionDescriptor.RouteValues["controller"];
        
        return tag;
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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartService API v1");
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); 
app.MapGraphQL()
    .WithOptions(new HotChocolate.AspNetCore.GraphQLServerOptions
    {
        AllowedGetOperations = HotChocolate.AspNetCore.AllowedGetOperations.Query
    });

app.Run();

