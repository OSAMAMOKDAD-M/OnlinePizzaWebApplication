using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlinePizzaWebApplication.Data;

namespace OnlinePizzaWebApplication.Models
{
    public class ShoppingCart
    {
        private readonly AppDbContext _context;

        private ShoppingCart(AppDbContext context)
        {
            _context = context;
        }

        public string ShoppingCartId { get; set; } = string.Empty;

        public List<ShoppingCartItem> ShoppingCartItems { get; set; } = new();

        public static ShoppingCart GetCart(IServiceProvider services)
        {
            var session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.Session;
            var context = services.GetService<AppDbContext>()
                ?? throw new InvalidOperationException("AppDbContext is not registered.");

            string cartId = session?.GetString("CartId") ?? Guid.NewGuid().ToString();
            session?.SetString("CartId", cartId);

            return new ShoppingCart(context) { ShoppingCartId = cartId };
        }

        public void AddToCart(Pizza pizza, int amount)
        {
            var cartItem = _context.ShoppingCartItems.SingleOrDefault(
                s => s.PizzaId == pizza.PizzaId && s.ShoppingCartId == ShoppingCartId);

            if (cartItem == null)
            {
                cartItem = new ShoppingCartItem
                {
                    ShoppingCartId = ShoppingCartId,
                    PizzaId = pizza.PizzaId,
                    Amount = amount,
                    Price = pizza.Price
                };
                _context.ShoppingCartItems.Add(cartItem);
            }
            else
            {
                cartItem.Amount += amount;
            }
            _context.SaveChanges();
        }

        public int RemoveFromCart(Pizza pizza)
        {
            var cartItem = _context.ShoppingCartItems.SingleOrDefault(
                s => s.PizzaId == pizza.PizzaId && s.ShoppingCartId == ShoppingCartId);

            int localAmount = 0;
            if (cartItem != null)
            {
                if (cartItem.Amount > 1)
                {
                    cartItem.Amount--;
                    localAmount = cartItem.Amount;
                }
                else
                {
                    _context.ShoppingCartItems.Remove(cartItem);
                }
            }
            _context.SaveChanges();
            return localAmount;
        }

        public List<ShoppingCartItem> GetShoppingCartItems()
        {
            return ShoppingCartItems.Count > 0
                ? ShoppingCartItems
                : (ShoppingCartItems = _context.ShoppingCartItems
                    .Where(c => c.ShoppingCartId == ShoppingCartId)
                    .Include(s => s.Pizza)
                    .ToList());
        }

        public void ClearCart()
        {
            var cartItems = _context.ShoppingCartItems.Where(c => c.ShoppingCartId == ShoppingCartId);
            _context.ShoppingCartItems.RemoveRange(cartItems);
            _context.SaveChanges();
            ShoppingCartItems.Clear();
        }

        public decimal GetShoppingCartTotal()
        {
            return _context.ShoppingCartItems
                .Where(c => c.ShoppingCartId == ShoppingCartId)
                .AsEnumerable()
                .Sum(c => c.Price * c.Amount);
        }
    }
}
