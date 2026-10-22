    using DigitalFormsSystem.Core.Models;
    using DigitalFormsSystem.Core.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System.Threading;
    using DigitalFormsSystem.Data;  
    using QuestPDF.Infrastructure;
    using DigitalFormsSystem.Core.Services; // For AppwriteStorageService, NotificationService, SessionCurrentUserService, AuditService, FixedAssetRequestService, DamagedReportService      
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.HttpOverrides; 
    using DigitalFormsSystem.Middleware; // For GlobalExceptionMiddleware

    namespace DigitalFormsSystem
    {
        public class Program
        {
            public static async Task Main(string[] args)
            {
                ThreadPool.SetMinThreads(100, 100);

                try
                {
                    Console.SetOut(TextWriter.Synchronized(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true }));
                    
                    var builder = WebApplication.CreateBuilder(args);

                    // ✅ Kestrel — allow large request bodies (default 30 MB)
                    builder.WebHost.ConfigureKestrel(options =>
                    {
                        options.Limits.MaxRequestBodySize = 100L * 1024 * 1024;  // 100 MB
                    });

                    // ✅ Form options — allow large multipart form uploads
                    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
                    {
                        options.MultipartBodyLengthLimit = 100L * 1024 * 1024;  // 100 MB
                    });

                    builder.Services.AddControllersWithViews();

                    builder.Services.AddDbContext<DigitalFormsSystemContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("DefaultConnection"),
                        sqlServer => sqlServer.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null)));

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
                    builder.Services.AddScoped<IStorageService, LocalStorageService>();
                    builder.Services.AddScoped<IAccountService, AccountService>();

                    var app = builder.Build();

                    QuestPDF.Settings.License = LicenseType.Community;

                    if (!app.Environment.IsDevelopment())
                    {
                        // app.UseExceptionHandler("/Home/Error"); // middleware handles exceptions globally
                        app.UseHsts();
                    }

                    // ✅ Must be BEFORE UseHttpsRedirection (Render / reverse proxies)
                    app.UseForwardedHeaders(new ForwardedHeadersOptions
                    {
                        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                    });

                    app.UseHttpsRedirection();
                    app.UseStaticFiles();
                    app.UseRouting();

                    // Global Exception Handling Middleware
                    app.UseMiddleware<GlobalExceptionMiddleware>(); 

                    // ✅ ORDER IS IMPORTANT: Authentication BEFORE Authorization
                    app.UseSession();
                    app.UseAuthentication();
                    app.UseAuthorization();

                    app.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=FixedAsset}/{action=Index}/{id?}");

                        // ⭐ SEED EMPLOYEE PASSWORDS
                        using (var scope = app.Services.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<DigitalFormsSystemContext>();
                            await DbInitializer.SeedEmployeePasswords(context);
                        }   

                    app.Run();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("FATAL: Application startup failed.");
                    Console.Error.WriteLine($"Exception type: {ex.GetType().Name}");
                    throw;
                }
            }
        }
    }