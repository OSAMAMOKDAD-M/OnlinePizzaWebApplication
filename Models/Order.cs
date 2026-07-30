using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlinePizzaWebApplication.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "الاسم")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "العنوان")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(30)]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(150)]
        [Display(Name = "البريد الإلكتروني")]
        public string? Email { get; set; }

        [Display(Name = "تاريخ الطلب")]
        public DateTime OrderPlaced { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "الإجمالي")]
        public decimal OrderTotal { get; set; }

        public string? UserId { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
