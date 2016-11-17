namespace Accounts.Models.ManageViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class VerifyPhoneNumberViewModel
    {
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
    }
}
