namespace Accounts.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Text;

    public class Address
    {
        [Key]
        public int AddressID { get; set; }

        [Required]
        [Display(Name = "CEP")]
        public int? ZipCode { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Bairro")]
        public string District { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Cidade")]
        public string City { get; set; }

        [DataType(DataType.MultilineText)]
        [MaxLength(2000)]
        [Display(Name = "Complemento")]
        public string Complement { get; set; }

        [Required]
        [MaxLength(2000)]
        [Display(Name = "Logradouro")]
        public string Street { get; set; }

        [Display(Name = "Número")]
        public int? Number { get; set; }

        [Required]
        [RegularExpression("[A-Z]{2}", ErrorMessage = "UF inválida, valor deve possuir duas letras maiúsculas.")]
        [Display(Name = "UF")]
        public string State { get; set; }

        public string StreetAndNumber
        {
            get
            {
                return Street + " " + Number;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("CEP: ");
            sb.AppendLine(ZipCode == null ? string.Empty : ZipCode.ToString());

            sb.Append("Logradouro: ");
            sb.AppendLine(Street);

            sb.Append("Número: ");
            sb.AppendLine(Number == null ? string.Empty : Number.ToString());

            sb.Append("Complemento: ");
            sb.AppendLine(Complement);

            sb.Append("Bairro: ");
            sb.AppendLine(District);

            sb.Append("Cidade: ");
            sb.AppendLine(City);

            sb.Append("Estado: ");
            sb.AppendLine(State);

            return sb.ToString();
        }
    }
}
