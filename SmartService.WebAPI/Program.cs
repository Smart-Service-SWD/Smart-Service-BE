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
using SmartService.API.Middleware;

using SmartService.API.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new GuidConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableGuidConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .Select(e => new
                {
                    PropertyName = e.Key,
                    ErrorMessage = e.Value?.Errors.First().ErrorMessage,
                    ExceptionMsg = e.Value?.Errors.First().Exception?.Message
                }).ToList();

            var errorResponse = new SmartService.Application.Common.Errors.ErrorResponse
            {
                Success = false,
                ErrorCode = SmartService.Application.Common.Errors.ErrorCodes.REQUEST_400_VALIDATION_FAILED,
                Message = "Validation failed.",
                Details = errors
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(errorResponse);
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

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

builder.Services.AddSignalR();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<SmartService.Application.Abstractions.Notifications.IServiceRequestNotificationService, SmartService.API.Notifications.SignalRServiceRequestNotificationService>();
builder.Services.AddHostedService<SmartService.Infrastructure.BackgroundServices.ServiceRequestAnalysisBackgroundService>();

var app = builder.Build();

var webRootPath = app.Environment.WebRootPath;
if (string.IsNullOrWhiteSpace(webRootPath))
{
    webRootPath = System.IO.Path.Combine(app.Environment.ContentRootPath, "wwwroot");
}
System.IO.Directory.CreateDirectory(System.IO.Path.Combine(webRootPath, "uploads", "price-adjustments"));
System.IO.Directory.CreateDirectory(System.IO.Path.Combine(webRootPath, "uploads", "completion-evidences"));

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    if (context.Request.IsHttps)
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }
    await next();
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

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

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SmartService.API.Hubs.ServiceRequestHub>("/hubs/service-request");
app.MapGraphQL()
    .WithOptions(new HotChocolate.AspNetCore.GraphQLServerOptions
    {
        AllowedGetOperations = HotChocolate.AspNetCore.AllowedGetOperations.Query
    });

app.Run();

