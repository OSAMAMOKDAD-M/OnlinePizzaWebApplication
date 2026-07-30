using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlinePizzaWebApplication.Models;

namespace OnlinePizzaWebApplication.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Pizza> Pizzas => Set<Pizza>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
        public DbSet<ShoppingCartItem> ShoppingCartItems => Set<ShoppingCartItem>();

        public DbSet<RestaurantSetting> RestaurantSettings => Set<RestaurantSetting>();
        public DbSet<LicenseKey> LicenseKeys => Set<LicenseKey>();
        public DbSet<LicenseActivation> LicenseActivations => Set<LicenseActivation>();
        public DbSet<LicenseAttempt> LicenseAttempts => Set<LicenseAttempt>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Pizza>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Pizzas)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LicenseKey>()
                .HasIndex(l => l.KeyHash)
                .IsUnique();

            builder.Entity<LicenseActivation>()
                .HasOne(a => a.LicenseKey)
                .WithMany(l => l.Activations)
                .HasForeignKey(a => a.LicenseKeyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LicenseActivation>()
                .HasIndex(a => new { a.LicenseKeyId, a.DeviceId })
                .IsUnique();
        }
    }
}
