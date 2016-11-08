using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    /// <summary>
    /// Utilizada para validações na alteração do endereço de e-mail
    /// </summary>
    public class AddressViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Required]
        public Address Address { get; set; }
    }
}