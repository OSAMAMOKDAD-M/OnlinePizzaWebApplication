using System.ComponentModel.DataAnnotations;

namespace OnlinePizzaWebApplication.Models
{
    /// <summary>
    /// Dynamic restaurant profile. A single row holds the values that are surfaced
    /// across the whole system (name, logo, contact details) so nothing is hardcoded.
    /// </summary>
    public class RestaurantSetting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "اسم المطعم")]
        public string Name { get; set; } = string.Empty;

        [StringLength(400)]
        [Display(Name = "شعار المطعم")]
        public string? LogoUrl { get; set; }

        [StringLength(250)]
        [Display(Name = "العنوان")]
        public string? Address { get; set; }

        [StringLength(40)]
        [Display(Name = "الهاتف")]
        public string? Phone { get; set; }

        [StringLength(1000)]
        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
