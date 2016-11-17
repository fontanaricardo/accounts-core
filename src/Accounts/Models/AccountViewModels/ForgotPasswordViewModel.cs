namespace Accounts.Models.AccountViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
