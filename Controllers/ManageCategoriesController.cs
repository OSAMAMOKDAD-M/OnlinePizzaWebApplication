using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlinePizzaWebApplication.Data;
using OnlinePizzaWebApplication.Models;
using OnlinePizzaWebApplication.Repositories;

namespace OnlinePizzaWebApplication.Controllers
{
    [Authorize(Roles = Roles.Management)]
    public class ManageCategoriesController : Controller
    {
        private readonly ICategoryRepository _categories;

        public ManageCategoriesController(ICategoryRepository categories) => _categories = categories;

        public async Task<IActionResult> Index() => View(await _categories.GetAllAsync());

        [HttpGet]
        public IActionResult Create() => View(new Category());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            await _categories.AddAsync(category);
            TempData["Success"] = "تمت إضافة التصنيف.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categories.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            await _categories.UpdateAsync(category);
            TempData["Success"] = "تم تحديث التصنيف.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categories.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categories.DeleteAsync(id);
            TempData["Success"] = "تم حذف التصنيف.";
            return RedirectToAction(nameof(Index));
        }
    }
}
