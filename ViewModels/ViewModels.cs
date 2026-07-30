using System.ComponentModel.DataAnnotations;
using OnlinePizzaWebApplication.Models;
using OnlinePizzaWebApplication.Services;

namespace OnlinePizzaWebApplication.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "تذكرني")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "كلمتا المرور غير متطابقتين")]
        [Display(Name = "تأكيد كلمة المرور")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ActivateLicenseViewModel
    {
        [Required(ErrorMessage = "الرجاء إدخال مفتاح التفعيل")]
        [Display(Name = "مفتاح التفعيل")]
        public string LicenseKey { get; set; } = string.Empty;
    }

    public class CreateLicenseViewModel
    {
        [Display(Name = "مدة الاشتراك")]
        public LicenseDuration Duration { get; set; } = LicenseDuration.Month;

        [Display(Name = "قفل على جهاز واحد")]
        public bool SingleDevice { get; set; } = true;

        [Range(1, 1000)]
        [Display(Name = "عدد الأجهزة المسموح بها")]
        public int MaxDevices { get; set; } = 1;

        [StringLength(300)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }
    }

    public class SettingsViewModel
    {
        public RestaurantSetting Restaurant { get; set; } = new();
        public LicenseStatusResult License { get; set; } = new();
        public int WarningDays { get; set; }

        [Display(Name = "مفتاح تفعيل جديد")]
        public string? NewLicenseKey { get; set; }
    }
}
