using OnlinePizzaWebApplication.Models;

namespace OnlinePizzaWebApplication.Repositories
{
    public interface IPizzaRepository
    {
        Task<IEnumerable<Pizza>> GetAllAsync();
        Task<IEnumerable<Pizza>> GetAvailableAsync();
        Task<Pizza?> GetByIdAsync(int id);
        Task AddAsync(Pizza pizza);
        Task UpdateAsync(Pizza pizza);
        Task DeleteAsync(int id);
    }

    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }

    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task CreateOrderAsync(Order order, IEnumerable<ShoppingCartItem> items);
    }
}
