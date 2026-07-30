using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlinePizzaWebApplication.Data;
using OnlinePizzaWebApplication.Models;
using OnlinePizzaWebApplication.Repositories;

namespace OnlinePizzaWebApplication.Controllers
{
    [Authorize(Roles = Roles.Management)]
    public class ManagePizzasController : Controller
    {
        private readonly IPizzaRepository _pizzas;
        private readonly ICategoryRepository _categories;

        public ManagePizzasController(IPizzaRepository pizzas, ICategoryRepository categories)
        {
            _pizzas = pizzas;
            _categories = categories;
        }

        public async Task<IActionResult> Index() => View(await _pizzas.GetAllAsync());

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesAsync();
            return View(new Pizza());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pizza pizza)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(pizza.CategoryId);
                return View(pizza);
            }
            await _pizzas.AddAsync(pizza);
            TempData["Success"] = "تمت إضافة البيتزا.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var pizza = await _pizzas.GetByIdAsync(id);
            if (pizza == null)
            {
                return NotFound();
            }
            await PopulateCategoriesAsync(pizza.CategoryId);
            return View(pizza);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Pizza pizza)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(pizza.CategoryId);
                return View(pizza);
            }
            await _pizzas.UpdateAsync(pizza);
            TempData["Success"] = "تم تحديث البيتزا.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var pizza = await _pizzas.GetByIdAsync(id);
            if (pizza == null)
            {
                return NotFound();
            }
            return View(pizza);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _pizzas.DeleteAsync(id);
            TempData["Success"] = "تم حذف البيتزا.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategoriesAsync(int? selected = null)
        {
            var categories = await _categories.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, nameof(Category.CategoryId), nameof(Category.Name), selected);
        }
    }
}
