using System.ComponentModel.DataAnnotations.Schema;

namespace OnlinePizzaWebApplication.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int PizzaId { get; set; }
        public Pizza? Pizza { get; set; }

        public int Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
