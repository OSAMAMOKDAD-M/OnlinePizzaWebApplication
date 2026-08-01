using System.ComponentModel.DataAnnotations;

namespace OnlinePizzaWebApplication.Models
{
    public enum LicenseDuration
    {
        [Display(Name = "أسبوع")]
        Week = 0,

        [Display(Name = "شهر")]
        Month = 1,

        [Display(Name = "شهران")]
        TwoMonths = 2,

        [Display(Name = "ثلاثة أشهر")]
        ThreeMonths = 3,

        [Display(Name = "ستة أشهر")]
        SixMonths = 4,

        [Display(Name = "سنة")]
        Year = 5
    }

    public enum LicenseStatus
    {
        [Display(Name = "فعّال")]
        Active = 0,

        [Display(Name = "منتهٍ")]
        Expired = 1,

        [Display(Name = "معطّل")]
        Disabled = 2
    }
}
