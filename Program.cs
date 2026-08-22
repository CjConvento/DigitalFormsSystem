    using DigitalFormsSystem.Core.Models;
    using DigitalFormsSystem.Core.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System.Threading;
    using DigitalFormsSystem.Data;  
    using QuestPDF.Infrastructure;
    using DigitalFormsSystem.Services;      // For SessionCurrentUserService
    using DigitalFormsSystem.Core.Services; // For FixedAssetRequestService
    using DigitalFormsSystem.Web.Services;  // For DamagedReportService, NotificationService
    using Microsoft.AspNetCore.Authentication.Cookies; 

    namespace DigitalFormsSystem.Web
    {
        public class Program
        {
            public static async Task Main(string[] args)
            {
                ThreadPool.SetMinThreads(100, 100);

                try
                {
                    var builder = WebApplication.CreateBuilder(args);

                    builder.Services.AddControllersWithViews();
                    builder.Services.AddDbContext<DigitalFormsSystemContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

                    builder.Services.AddDistributedMemoryCache();
                    builder.Services.AddSession(options =>
                    {
                        options.IdleTimeout = TimeSpan.FromMinutes(30);
                        options.Cookie.HttpOnly = true;
                        options.Cookie.IsEssential = true;
                    });

                    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/Account/Login";
                        options.LogoutPath = "/Account/Logout";
                        options.AccessDeniedPath = "/Account/AccessDenied";
                        options.Cookie.HttpOnly = true;
                        options.Cookie.IsEssential = true;
                        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                        options.SlidingExpiration = true;
                    });

                    builder.Services.AddHttpContextAccessor();
                    builder.Services.AddScoped<ICurrentUserService, SessionCurrentUserService>();
                    builder.Services.AddScoped<IFixedAssetRequestService, FixedAssetRequestService>();
                    builder.Services.AddScoped<IDamagedReportService, DamagedReportService>();
                    builder.Services.AddScoped<INotificationService, NotificationService>();
                    builder.Services.AddScoped<IAuditService, AuditService>();

                    var app = builder.Build();

                    QuestPDF.Settings.License = LicenseType.Community;

                    if (!app.Environment.IsDevelopment())
                    {
                        app.UseExceptionHandler("/Home/Error");
                        app.UseHsts();
                    }

                    app.UseHttpsRedirection();
                    app.UseStaticFiles();
                    app.UseRouting();

                    // ✅ ORDER IS IMPORTANT: Authentication BEFORE Authorization
                    app.UseSession();
                    app.UseAuthentication();
                    app.UseAuthorization();

                    app.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=FixedAsset}/{action=Index}/{id?}");

                    if (!IsDesignTime())
                    {
                        // ⭐ SEED EMPLOYEE PASSWORDS
                        using (var scope = app.Services.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<DigitalFormsSystemContext>();
                            await DbInitializer.SeedEmployeePasswords(context);
                        }   
                    }

                    app.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("===== FATAL EXCEPTION =====");
                    Console.WriteLine(ex.ToString());
                    throw;
                }
            }

            // ✅ Helper method to check if running in design-time (migrations)
            private static bool IsDesignTime()
            {
                // Check for design-time environment
                var designTime = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
                
                // Or check command line args
                var args = Environment.GetCommandLineArgs();
                if (args.Any(a => a.Contains("ef") || a.Contains("migrations")))
                    return true;

                return false;
            }
        }
    }