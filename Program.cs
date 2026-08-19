using DigitalFormsSystem.Models;
using DigitalFormsSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using DigitalFormsSystem.Services;      // For SessionCurrentUserService
using DigitalFormsSystem.Core.Services; // For FixedAssetRequestService
using DigitalFormsSystem.Web.Services;  // For DamagedReportService, NotificationService

namespace DigitalFormsSystem.Web
{
    public class Program
    {
        public static void Main(string[] args)
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

                builder.Services.AddHttpContextAccessor();
                builder.Services.AddScoped<ICurrentUserService, SessionCurrentUserService>();
                builder.Services.AddScoped<IFixedAssetRequestService, FixedAssetRequestService>();
                builder.Services.AddScoped<IDamagedReportService, DamagedReportService>();
                builder.Services.AddScoped<INotificationService, NotificationService>();

                var app = builder.Build();

                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseRouting();
                app.UseSession();
                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=FixedAsset}/{action=Index}/{id?}");

                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== FATAL EXCEPTION =====");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}