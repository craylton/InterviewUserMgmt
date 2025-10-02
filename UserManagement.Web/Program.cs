using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Data.Extensions;
using UserManagement.Services.Extensions;
using UserManagement.Web.Middleware;
using Westwind.AspNetCore.Markdown;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddDataAccess()
    .AddDomainServices()
    .AddMarkdown()
    .AddControllersWithViews();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseMarkdown();

app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
