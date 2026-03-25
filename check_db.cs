
using Microsoft.EntityFrameworkCore;
using SmartService.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("SmartService.WebAPI/appsettings.json");

var configuration = builder.Build();

var services = new ServiceCollection();
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

using var serviceProvider = services.BuildServiceProvider();
using var context = serviceProvider.GetRequiredService<AppDbContext>();

var latestRequests = await context.ServiceRequests
    .OrderByDescending(r => r.CreatedAt)
    .Take(5)
    .Select(r => new { r.Id, r.Status, r.PayOSOrderCode, r.CreatedAt })
    .ToListAsync();

foreach (var req in latestRequests)
{
    Console.WriteLine($"ID: {req.Id}, Status: {req.Status}, OrderCode: {req.PayOSOrderCode}, CreatedAt: {req.CreatedAt}");
}
