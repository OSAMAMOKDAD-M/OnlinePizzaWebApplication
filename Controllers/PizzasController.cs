using Microsoft.AspNetCore.Mvc;
using OnlinePizzaWebApplication.Repositories;

namespace OnlinePizzaWebApplication.Controllers
{
    public class PizzasController : Controller
    {
        private readonly IPizzaRepository _pizzas;

        public PizzasController(IPizzaRepository pizzas) => _pizzas = pizzas;

        public async Task<IActionResult> Index()
        {
            var pizzas = await _pizzas.GetAvailableAsync();
            return View(pizzas);
        }

        public async Task<IActionResult> Details(int id)
        {
            var pizza = await _pizzas.GetByIdAsync(id);
            if (pizza == null)
            {
                return NotFound();
            }
            return View(pizza);
        }
    }
}
