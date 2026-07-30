using Microsoft.AspNetCore.Mvc;
using OnlinePizzaWebApplication.Services;
using OnlinePizzaWebApplication.ViewModels;

namespace OnlinePizzaWebApplication.Controllers
{
    /// <summary>
    /// Public license gate: activation form and the "subscription expired/blocked" screen.
    /// These pages stay reachable even when the system is locked.
    /// </summary>
    public class LicenseController : Controller
    {
        private readonly ILicenseService _licenseService;

        public LicenseController(ILicenseService licenseService) => _licenseService = licenseService;

        [HttpGet]
        public async Task<IActionResult> Activate()
        {
            var status = await _licenseService.GetStatusAsync();
            if (status.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Status = status;
            return View(new ActivateLicenseViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(ActivateLicenseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Status = await _licenseService.GetStatusAsync();
                return View(model);
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _licenseService.ActivateAsync(model.LicenseKey, ip);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, result.Message);
            ViewBag.Status = await _licenseService.GetStatusAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Blocked()
        {
            var status = await _licenseService.GetStatusAsync();
            if (status.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.WarningDays = _licenseService.WarningDays;
            return View(status);
        }
    }
}
