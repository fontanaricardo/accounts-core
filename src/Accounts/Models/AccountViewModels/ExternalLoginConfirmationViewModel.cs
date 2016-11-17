namespace Accounts.Models.AccountViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
