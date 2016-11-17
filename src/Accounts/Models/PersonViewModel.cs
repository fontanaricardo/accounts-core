namespace Accounts.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Usada para validações na edição do dados do usuário
    /// </summary>
    public class PersonViewModel
    {
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [MaxLength(1500)]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Display(Name = "CPF")]
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [MaxLength(11)]
        [MinLength(11)]
        public string CPF { get; set; }

        [Display(Name = "Identidade")]
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [MaxLength(500)]
        public string RG { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [MaxLength(2000)]
        [Display(Name = "Órgão expedidor")]
        public string Dispatcher { get; set; }
    }
}