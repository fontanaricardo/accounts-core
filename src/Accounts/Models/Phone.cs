namespace Accounts.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Phone
    {
        public int PhoneID { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [RegularExpression(pattern: @"^\(\d{2}\) ?\d?\d{8}$", ErrorMessage = "O telefone deve estar no formato (XX) XXXXXXXX")]
        [Display(Name = "Número")]
        public string Number { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [Display(Name = "Atualizado em")]
        public DateTime UpdatedAt { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [MinLength(11)]
        [MaxLength(14)]
        [Display(Name = "CPF ou CNPJ do proprietário do telefone")]
        public string Document { get; set; }
    }
}
