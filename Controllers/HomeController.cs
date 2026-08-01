using Microsoft.AspNetCore.Mvc;
using OnlinePizzaWebApplication.Repositories;

namespace OnlinePizzaWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPizzaRepository _pizzas;

        public HomeController(IPizzaRepository pizzas) => _pizzas = pizzas;

        public async Task<IActionResult> Index()
        {
            var pizzas = await _pizzas.GetAvailableAsync();
            return View(pizzas);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}
