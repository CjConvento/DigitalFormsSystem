using System.ComponentModel.DataAnnotations;

namespace FixedAssetSystem.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Employee No. is required")]
        [Display(Name = "Employee No.")]
        public string EmployeeNo { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}