namespace Accounts.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Utilizada para validações na alteração do endereço de e-mail
    /// </summary>
    public class AddressViewModel
    {
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        public Address Address { get; set; }
    }
}