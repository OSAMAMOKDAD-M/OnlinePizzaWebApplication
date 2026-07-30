using Microsoft.AspNetCore.Mvc;
using OnlinePizzaWebApplication.Models;
using OnlinePizzaWebApplication.Repositories;

namespace OnlinePizzaWebApplication.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IPizzaRepository _pizzas;
        private readonly ShoppingCart _cart;

        public ShoppingCartController(IPizzaRepository pizzas, ShoppingCart cart)
        {
            _pizzas = pizzas;
            _cart = cart;
        }

        public IActionResult Index()
        {
            var items = _cart.GetShoppingCartItems();
            ViewBag.Total = _cart.GetShoppingCartTotal();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int id)
        {
            var pizza = await _pizzas.GetByIdAsync(id);
            if (pizza != null)
            {
                _cart.AddToCart(pizza, 1);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var pizza = await _pizzas.GetByIdAsync(id);
            if (pizza != null)
            {
                _cart.RemoveFromCart(pizza);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
