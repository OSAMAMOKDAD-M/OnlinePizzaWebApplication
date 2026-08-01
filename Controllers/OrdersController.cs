using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlinePizzaWebApplication.Data;
using OnlinePizzaWebApplication.Models;
using OnlinePizzaWebApplication.Repositories;

namespace OnlinePizzaWebApplication.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orders;
        private readonly ShoppingCart _cart;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(IOrderRepository orders, ShoppingCart cart, UserManager<IdentityUser> userManager)
        {
            _orders = orders;
            _cart = cart;
            _userManager = userManager;
        }

        public IActionResult Checkout() => View(new Order());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            var items = _cart.GetShoppingCartItems();
            if (items.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "سلة التسوق فارغة.");
            }

            if (!ModelState.IsValid)
            {
                return View(order);
            }

            order.UserId = _userManager.GetUserId(User);
            await _orders.CreateOrderAsync(order, items);
            _cart.ClearCart();
            return RedirectToAction(nameof(Confirmation), new { id = order.OrderId });
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var order = await _orders.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [Authorize(Roles = Roles.Management)]
        public async Task<IActionResult> Index()
        {
            var orders = await _orders.GetAllAsync();
            return View(orders);
        }

        [Authorize(Roles = Roles.Management)]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orders.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }
    }
}
