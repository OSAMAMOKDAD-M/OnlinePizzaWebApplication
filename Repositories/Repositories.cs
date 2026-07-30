using Microsoft.EntityFrameworkCore;
using OnlinePizzaWebApplication.Data;
using OnlinePizzaWebApplication.Models;

namespace OnlinePizzaWebApplication.Repositories
{
    public class PizzaRepository : IPizzaRepository
    {
        private readonly AppDbContext _context;
        public PizzaRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Pizza>> GetAllAsync() =>
            await _context.Pizzas.Include(p => p.Category).OrderBy(p => p.Name).ToListAsync();

        public async Task<IEnumerable<Pizza>> GetAvailableAsync() =>
            await _context.Pizzas.Include(p => p.Category)
                .Where(p => p.IsAvailable).OrderBy(p => p.Name).ToListAsync();

        public Task<Pizza?> GetByIdAsync(int id) =>
            _context.Pizzas.Include(p => p.Category).FirstOrDefaultAsync(p => p.PizzaId == id);

        public async Task AddAsync(Pizza pizza)
        {
            _context.Pizzas.Add(pizza);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Pizza pizza)
        {
            _context.Pizzas.Update(pizza);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var pizza = await _context.Pizzas.FindAsync(id);
            if (pizza != null)
            {
                _context.Pizzas.Remove(pizza);
                await _context.SaveChangesAsync();
            }
        }
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;
        public CategoryRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Category>> GetAllAsync() =>
            await _context.Categories.OrderBy(c => c.Name).ToListAsync();

        public Task<Category?> GetByIdAsync(int id) =>
            _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);

        public async Task AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        public OrderRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Order>> GetAllAsync() =>
            await _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(d => d.Pizza)
                .OrderByDescending(o => o.OrderPlaced).ToListAsync();

        public Task<Order?> GetByIdAsync(int id) =>
            _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(d => d.Pizza)
                .FirstOrDefaultAsync(o => o.OrderId == id);

        public async Task CreateOrderAsync(Order order, IEnumerable<ShoppingCartItem> items)
        {
            order.OrderPlaced = DateTime.UtcNow;
            order.OrderTotal = items.Sum(i => i.Price * i.Amount);
            order.OrderDetails = items.Select(i => new OrderDetail
            {
                PizzaId = i.PizzaId,
                Amount = i.Amount,
                Price = i.Price
            }).ToList();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }
    }
}
