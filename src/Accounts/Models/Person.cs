namespace Accounts.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text;
    using CustomAttributes;
    using Extensions;
    using Newtonsoft.Json;

    public class Person : IValidatableObject
    {
        private string _name;

        [JsonIgnore]
        [Key]
        public int PersonID { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [MaxLength(1500)]
        [JsonProperty("nome")]
        [Display(Name = "Nome")]
        public string Name
        {
            get
            {
                return this._name;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this._name = value.ToNameCase();
                }
            }
        }

        [Display(Name = "CPF")]
        [JsonProperty("cpf")]
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [MaxLength(11)]
        [MinLength(11)]
        public string CPF { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [Display(Name = "E-mail")]
        [JsonProperty("email")]
        public string Email { get; set; }

        [Display(Name = "Identidade")]
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [MaxLength(500)]
        [JsonProperty("rg")]
        public string RG { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [MaxLength(2000)]
        [Display(Name = "Órgão expedidor")]
        [JsonProperty("orgao_expedidor")]
        public string Dispatcher { get; set; }

        [JsonIgnore]
        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [ForeignKey("Address")]
        public int AddressID { get; set; }

        [JsonProperty("id_usuario")]
        [Display(Name = "Código no SEI")]
        public long? SeiId { get; set; }

        [JsonIgnore]
        [Display(Name = "Protocolo do SEI")]
        public string SeiProtocol { get; set; }

        [JsonIgnore]
        [Display(Name = "Link do protocolo do SEI")]
        public string LinkSeiProtocol { get; set; }

        /// <summary>
        /// Informa se o usuário possui certificação
        /// </summary>
        /// <remarks>
        /// Utilize o método <see cref="UpdateEletronicSignatureStatus"/> para atualizar este campo com base no serviço do SEI
        /// </remarks>
        [JsonIgnore]
        [Display(Name = "Situação da certificação")]
        public EletronicSignatureStatus EletronicSignatureStatus { get; set; }

        [JsonIgnore]
        public Address Address { get; set; }

        [JsonIgnore]
        public ICollection<Access> Access { get; set; }

        /// <summary>
        /// Lista de telefones do usuário, este attributo não é mapeado, carregar antes de usar.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public List<Phone> Phones { get; set; }

        /// <summary>
        /// Retorna o último telefone alterado ou editado pelo usuário
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public string PhoneLastUpdated
        {
            get
            {
                var lastPhone = string.Empty;

                if (Phones != null && Phones.Count() > 0)
                {
                    lastPhone = Phones.OrderByDescending(p => p.UpdatedAt).ThenBy(p => p.PhoneID).Select(p => p.Number).FirstOrDefault();
                }

                return lastPhone;
            }
        }

        /// <summary>
        /// Converte uma string no formato Json em pessoa
        /// </summary>
        /// <param name="person">Objeto do tipo pessoa</param>
        /// <param name="json">Json a ser convertido</param>
        /// <returns>Objeto do tipo Person</returns>
        public static Person DeserializePersonJson(Person person, string json)
        {
            var result = JsonConvert.DeserializeObject<dynamic>(json);

            if (((string)result.msg).ToLowerInvariant().Equals("usuário encontrado"))
            {
                person = JsonConvert.DeserializeObject<Person>(json);

                if (((string)result.sta_tipo_descricao).ToLowerInvariant().Contains("liberado"))
                {
                    person.EletronicSignatureStatus = EletronicSignatureStatus.Approved;
                }

                person.CPF = person.CPF.PadLeft(11, '0');
            }

            return person;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Nome: ");
            sb.AppendLine(Name);

            sb.Append("CPF: ");
            sb.AppendLine(CPF);

            sb.Append("RG: ");
            sb.Append(RG);
            sb.Append(" Órgão emissor: ");
            sb.AppendLine(Dispatcher);

            if (Address != null)
            {
                sb.Append(Address.ToString());
            }

            sb.Append("Email: ");
            sb.AppendLine(Email);

            return sb.ToString();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            var rUtils = new RegexUtilities();

            // TODO: Ver uma maneira melhor de fazer isso
            if (!CPFAttribute.ValidarCPF(CPF))
            {
                results.Add(new ValidationResult("CPF inválido", new string[] { "CPF" }));
            }

            if (!rUtils.IsValidEmail(Email))
            {
                results.Add(new ValidationResult("E-mail inválido", new string[] { "Email" }));
            }

            return results;
        }
    }
}
