using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlinePizzaWebApplication.Data;
using OnlinePizzaWebApplication.Models;
using OnlinePizzaWebApplication.Services;
using OnlinePizzaWebApplication.ViewModels;

namespace OnlinePizzaWebApplication.Controllers
{
    [Authorize(Roles = Roles.SuperAdmin)]
    public class LicenseAdminController : Controller
    {
        private readonly ILicenseService _licenseService;

        public LicenseAdminController(ILicenseService licenseService) => _licenseService = licenseService;

        public async Task<IActionResult> Index(string? q)
        {
            ViewBag.Query = q;
            var licenses = await _licenseService.SearchAsync(q);
            return View(licenses);
        }

        [HttpGet]
        public IActionResult Create() => View(new CreateLicenseViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLicenseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (_, rawKey) = await _licenseService.CreateLicenseAsync(
                model.Duration, model.MaxDevices, model.SingleDevice, model.Notes);

            // The raw key is shown exactly once; it is never stored in plaintext.
            TempData["NewKey"] = rawKey;
            TempData["Success"] = "تم إنشاء المفتاح بنجاح. انسخه الآن فلن يظهر مرة أخرى.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var license = await _licenseService.GetByIdAsync(id);
            if (license == null)
            {
                return NotFound();
            }
            ViewBag.Activations = await _licenseService.GetActivationsAsync(id);
            return View(license);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable(int id)
        {
            await _licenseService.SetStatusAsync(id, LicenseStatus.Disabled);
            TempData["Success"] = "تم تعطيل المفتاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enable(int id)
        {
            await _licenseService.SetStatusAsync(id, LicenseStatus.Active);
            TempData["Success"] = "تم تفعيل المفتاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _licenseService.DeleteAsync(id);
            TempData["Success"] = "تم حذف المفتاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDevice(int id, int licenseId)
        {
            await _licenseService.RemoveActivationAsync(id);
            TempData["Success"] = "تم إلغاء تفعيل الجهاز.";
            return RedirectToAction(nameof(Details), new { id = licenseId });
        }
    }
}
