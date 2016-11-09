namespace Accounts.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Phone
    {
        public int PhoneID { get; set; }

        [Required]
        [RegularExpression(pattern: @"^\(\d{2}\) ?\d{0,1}\d{8}$", ErrorMessage = "O telefone deve estar no formato (XX) XXXXXXXX")]
        [Display(Name = "Número")]
        public string Number { get; set; }

        [Required]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Display(Name = "Atualizado em")]
        public DateTime UpdatedAt { get; set; }

        [Required]
        [MinLength(11)]
        [MaxLength(14)]
        [Display(Name = "CPF ou CNPJ do proprietário do telefone")]
        public string Document { get; set; }
    }
}
