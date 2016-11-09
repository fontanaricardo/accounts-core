namespace Accounts.Models.ManageViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Senha atual")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0} senha deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova senha")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmação da nova senha")]
        [Compare("NewPassword", ErrorMessage = "A nova senha e sua confirmação não conferem.")]
        public string ConfirmPassword { get; set; }
    }
}
