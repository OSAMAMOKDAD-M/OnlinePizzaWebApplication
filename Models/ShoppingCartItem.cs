using System.ComponentModel.DataAnnotations.Schema;

namespace OnlinePizzaWebApplication.Models
{
    public class ShoppingCartItem
    {
        public int ShoppingCartItemId { get; set; }

        public int PizzaId { get; set; }
        public Pizza? Pizza { get; set; }

        public int Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string ShoppingCartId { get; set; } = string.Empty;
    }
}
