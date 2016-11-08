using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class PhoneViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Required]
        public Phone Phone { get; set; }
    }
}