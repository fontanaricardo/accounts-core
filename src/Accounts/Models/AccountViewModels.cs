namespace Accounts.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }

        public string ReturnUrl { get; set; }

        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "CPF")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Display(Name = "Permanecer conectado?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterPersonViewModel
    {
        [Required]
        public Person Person { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmação da senha")]
        [Compare("Password", ErrorMessage = "A senha e a confirmação não conferem.")]
        public string ConfirmPassword { get; set; }

        // TODO: Centralizar regex no código
        [Display(Name = "Email")]
        [Required]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; }

        [Display(Name = "Confirmação do e-mail")]
        [Compare("Email", ErrorMessage = "O e-mail e sua confirmação não conferem.")]
        public string ConfirmEmail { get; set; }
    }

    public class RegisterCompanyViewModel
    {
        [Required]
        public Company Company { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmação da senha")]
        [Compare("Password", ErrorMessage = "A senha e a confirmação não conferem.")]
        public string ConfirmPassword { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmação da senha")]
        [Compare("Password", ErrorMessage = "A senha e a confirmação não conferem.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [Display(Name = "CPF")]
        public string Document { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmação da senha")]
        [Compare("Password", ErrorMessage = "A senha e a confirmação não conferem.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "CPF")]
        public string CPF { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ConfirmationEmailViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "CPF")]
        public string CPF { get; set; }
    }

    public class ChangeEmailViewModel
    {
        [Display(Name = "Endereço de e-mail atual")]
        public string CurrentEmail { get; set; }

        [Display(Name = "Email")]
        [Required]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; }

        [Display(Name = "Confirmação do e-mail")]
        [Compare("Email", ErrorMessage = "O e-mail e sua confirmação não conferem.")]
        public string ConfirmEmail { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }
    }
}
