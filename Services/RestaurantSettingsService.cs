using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlinePizzaWebApplication.Data;
using OnlinePizzaWebApplication.Models;

namespace OnlinePizzaWebApplication.Services
{
    public interface IRestaurantSettingsService
    {
        /// <summary>Returns the current (cached) restaurant profile, creating a default row if none exists.</summary>
        Task<RestaurantSetting> GetAsync();
        Task UpdateAsync(RestaurantSetting updated);
    }

    /// <summary>
    /// Provides the dynamic restaurant profile. Values are cached in memory and the cache
    /// is invalidated on update, so changes apply instantly across the app without a restart.
    /// </summary>
    public class RestaurantSettingsService : IRestaurantSettingsService
    {
        private const string CacheKey = "restaurant-settings";
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public RestaurantSettingsService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<RestaurantSetting> GetAsync()
        {
            if (_cache.TryGetValue(CacheKey, out RestaurantSetting? cached) && cached != null)
            {
                return cached;
            }

            var setting = await _context.RestaurantSettings.AsNoTracking().FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new RestaurantSetting { Name = "المطعم", UpdatedAtUtc = DateTime.UtcNow };
                _context.RestaurantSettings.Add(setting);
                await _context.SaveChangesAsync();
            }

            _cache.Set(CacheKey, setting, TimeSpan.FromMinutes(30));
            return setting;
        }

        public async Task UpdateAsync(RestaurantSetting updated)
        {
            var setting = await _context.RestaurantSettings.FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new RestaurantSetting();
                _context.RestaurantSettings.Add(setting);
            }

            setting.Name = updated.Name;
            setting.LogoUrl = updated.LogoUrl;
            setting.Address = updated.Address;
            setting.Phone = updated.Phone;
            setting.Description = updated.Description;
            setting.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _cache.Remove(CacheKey);
        }
    }
}
