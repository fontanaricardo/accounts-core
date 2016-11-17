namespace Accounts.Models.AccountViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class VerifyCodeViewModel
    {
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        public string Provider { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
