using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Services;
using DigitalFormsSystem.Services;
using DigitalFormsSystem.Web.Services;
using DigitalFormsSystem.Core.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Classic Blazor Server setup (walang InteractiveServer)
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Register DbContext
builder.Services.AddDbContext<DigitalFormsSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Services
builder.Services.AddScoped<ICurrentUserService, SessionCurrentUserService>();
builder.Services.AddScoped<IFixedAssetRequestService, FixedAssetRequestService>();
builder.Services.AddScoped<IDamagedReportService, DamagedReportService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();