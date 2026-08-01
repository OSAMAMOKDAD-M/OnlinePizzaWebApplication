using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlinePizzaWebApplication.Models;
using OnlinePizzaWebApplication.Services;

namespace OnlinePizzaWebApplication.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();
            var config = services.GetRequiredService<IConfiguration>();
            var env = services.GetRequiredService<IWebHostEnvironment>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            await context.Database.MigrateAsync();

            await SeedRolesAsync(services);
            await SeedSuperAdminAsync(services, config);
            await SeedRestaurantAsync(context, config);
            SeedCatalog(context);
            await SeedLicensesAsync(services, env, logger);
        }

        private static async Task SeedRolesAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in Roles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedSuperAdminAsync(IServiceProvider services, IConfiguration config)
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var email = config["Seed:SuperAdminEmail"] ?? "superadmin@pizza.local";
            var password = config["Seed:SuperAdminPassword"] ?? "SuperAdmin#2026";

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(user, new[] { Roles.SuperAdmin, Roles.Owner, Roles.Admin });
                }
            }
        }

        private static async Task SeedRestaurantAsync(AppDbContext context, IConfiguration config)
        {
            if (!await context.RestaurantSettings.AnyAsync())
            {
                context.RestaurantSettings.Add(new RestaurantSetting
                {
                    Name = config["Seed:RestaurantName"] ?? "بيتزا أونلاين",
                    Phone = config["Seed:RestaurantPhone"] ?? "01006765664",
                    Address = config["Seed:RestaurantAddress"] ?? "",
                    Description = "أشهى أنواع البيتزا الطازجة تُحضّر بعناية وتصل إلى بابك.",
                    UpdatedAtUtc = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }
        }

        private static void SeedCatalog(AppDbContext context)
        {
            if (context.Categories.Any())
            {
                return;
            }

            var classic = new Category { Name = "كلاسيكية", Description = "البيتزا التقليدية" };
            var special = new Category { Name = "مميزة", Description = "وصفات المطعم الخاصة" };
            context.Categories.AddRange(classic, special);
            context.SaveChanges();

            context.Pizzas.AddRange(
                new Pizza { Name = "مارغريتا", Description = "صلصة طماطم وجبنة موزاريلا وريحان.", Price = 90m, CategoryId = classic.CategoryId, IsAvailable = true },
                new Pizza { Name = "بيبروني", Description = "شرائح بيبروني وجبنة وفير.", Price = 120m, CategoryId = classic.CategoryId, IsAvailable = true },
                new Pizza { Name = "خضار", Description = "فلفل وزيتون وفطر وبصل.", Price = 100m, CategoryId = classic.CategoryId, IsAvailable = true },
                new Pizza { Name = "سوبريم المطعم", Description = "تشكيلة كاملة من اللحوم والخضار.", Price = 160m, CategoryId = special.CategoryId, IsAvailable = true }
            );
            context.SaveChanges();
        }

        private static async Task SeedLicensesAsync(
            IServiceProvider services, IWebHostEnvironment env, ILogger logger)
        {
            var context = services.GetRequiredService<AppDbContext>();
            if (await context.LicenseKeys.AnyAsync())
            {
                return;
            }

            var licenseService = services.GetRequiredService<ILicenseService>();

            var (_, yearKey) = await licenseService.CreateLicenseAsync(LicenseDuration.Year, 1, true, "مفتاح افتتاحي - سنة");
            var (_, monthKey) = await licenseService.CreateLicenseAsync(LicenseDuration.Month, 1, true, "مفتاح تجريبي - شهر");

            var lines = new[]
            {
                "# مفاتيح التفعيل المُولّدة عند أول تشغيل (احفظها في مكان آمن ثم احذف هذا الملف).",
                $"# {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC",
                $"سنة : {yearKey}",
                $"شهر : {monthKey}"
            };
            try
            {
                var path = Path.Combine(env.ContentRootPath, "App_Data", "seed-licenses.txt");
                await File.WriteAllLinesAsync(path, lines);
                logger.LogWarning("تم توليد مفاتيح تفعيل افتتاحية وحفظها في App_Data/seed-licenses.txt");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "فشل حفظ ملف مفاتيح التفعيل الافتتاحية.");
            }

            // In development, activate the year key on this device so the app is immediately usable.
            if (env.IsDevelopment())
            {
                await licenseService.ActivateAsync(yearKey, "127.0.0.1");
                logger.LogWarning("تم تفعيل المفتاح السنوي تلقائيًا على هذا الجهاز (بيئة التطوير).");
            }
        }
    }
}
