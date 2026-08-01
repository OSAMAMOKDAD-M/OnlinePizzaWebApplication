using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlinePizzaWebApplication.Models
{
    public class Pizza
    {
        public int PizzaId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "اسم البيتزا")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000)]
        [Display(Name = "السعر")]
        public decimal Price { get; set; }

        [StringLength(400)]
        [Display(Name = "رابط الصورة")]
        public string? ImageUrl { get; set; }

        [Display(Name = "متوفر")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "التصنيف")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
