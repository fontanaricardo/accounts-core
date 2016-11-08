using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.Models.ManageViewModels
{
    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "A {0} senha deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "A nova senha e sua confirmação não conferem.")]
        public string ConfirmPassword { get; set; }
    }
}
