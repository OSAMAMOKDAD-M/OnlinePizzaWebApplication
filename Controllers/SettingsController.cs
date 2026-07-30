using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlinePizzaWebApplication.Data;
using OnlinePizzaWebApplication.Models;
using OnlinePizzaWebApplication.Services;
using OnlinePizzaWebApplication.ViewModels;

namespace OnlinePizzaWebApplication.Controllers
{
    [Authorize(Roles = Roles.Management)]
    public class SettingsController : Controller
    {
        private readonly IRestaurantSettingsService _settings;
        private readonly ILicenseService _licenseService;

        public SettingsController(IRestaurantSettingsService settings, ILicenseService licenseService)
        {
            _settings = settings;
            _licenseService = licenseService;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new SettingsViewModel
            {
                Restaurant = await _settings.GetAsync(),
                License = await _licenseService.GetStatusAsync(),
                WarningDays = _licenseService.WarningDays
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRestaurant(RestaurantSetting model)
        {
            if (!ModelState.IsValid)
            {
                var vm = new SettingsViewModel
                {
                    Restaurant = model,
                    License = await _licenseService.GetStatusAsync(),
                    WarningDays = _licenseService.WarningDays
                };
                return View(nameof(Index), vm);
            }

            await _settings.UpdateAsync(model);
            TempData["Success"] = "تم تحديث بيانات المطعم.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateKey(string newLicenseKey)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _licenseService.ActivateAsync(newLicenseKey ?? string.Empty, ip);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
