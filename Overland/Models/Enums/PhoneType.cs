using System.ComponentModel.DataAnnotations;

namespace Overland.Models.Enums;

public enum PhoneType {
    [Display(Name = "Cell (Optional)")] Cell = 1,

    [Display(Name = "Work")] Work = 2,

    [Display(Name = "Home")] Home = 3
}