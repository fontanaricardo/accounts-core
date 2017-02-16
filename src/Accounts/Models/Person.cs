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
    using Services;

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
        /// Retorna o objeto person a partir dos dados do SEI. Utiliza o serviço /pmj/cadastro_usuario_externo_consulta.php
        /// </summary>
        /// <param name="id">Código do usuário no SEI</param>
        /// <returns>Objeto Person com os dados no SEI</returns>
        public static Person GetSeiPersonBy(long id, AppSettings appSettings)
        {
            return GetSeiPersonBy("id_usuario", id.ToString(), appSettings);
        }

        /// <summary>
        /// Retorna o objeto person a partir dos dados do SEI. Utiliza o serviço /pmj/cadastro_usuario_externo_consulta.php
        /// </summary>
        /// <param name="email">Email do usuário no SEI</param>
        /// <returns>Objeto Person com os dados no SEI</returns>
        public static Person GetSeiPersonBy(string email, AppSettings appSettings)
        {
            return GetSeiPersonBy("email", email, appSettings);
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

        /// <summary>
        /// Cria um usuário no SEI
        /// </summary>
        /// <param name="password">Senha do usuário no SEI</param>
        public void CreateOrUpdateSeiUser(string password, AppSettings appSettings)
        {
            if (Phones == null || Phones.Count() == 0)
            {
                throw new ArgumentNullException(null, "Deve ser informado o número de telefone");
            }

            if (Address == null)
            {
                throw new ArgumentNullException("Address", "Endereço não pode ficar em branco.");
            }

            // Token para autenticação no serviço
            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();

            values.Add(new KeyValuePair<string, string>("token", appSettings.Token));

            Person seiPerson = GetSeiPersonBy(Email, appSettings);

            if (seiPerson != null)
            {
                if (!CPF.Equals(seiPerson.CPF))
                {
                    throw new ArgumentException("Este e-mail está relacionado a outro CPF no SEI");
                }
                else
                {
                    SeiId = seiPerson.SeiId;
                }
            }

            if (SeiId != null)
            {
                values.Add(new KeyValuePair<string, string>("valores[id_usuario]", SeiId.ToString()));
            }

            // Cria vetor no formato do PHP
            values.Add(new KeyValuePair<string, string>("valores[nome]", Name));
            values.Add(new KeyValuePair<string, string>("valores[cpf]", CPF));
            values.Add(new KeyValuePair<string, string>("valores[rg]", RG));
            values.Add(new KeyValuePair<string, string>("valores[orgao_expedidor]", Dispatcher));
            values.Add(new KeyValuePair<string, string>("valores[telefone]", PhoneLastUpdated));
            values.Add(new KeyValuePair<string, string>("valores[endereco]", Address.StreetAndNumber));
            values.Add(new KeyValuePair<string, string>("valores[bairro]", Address.District));
            values.Add(new KeyValuePair<string, string>("valores[cidade]", Address.City));
            values.Add(new KeyValuePair<string, string>("valores[estado]", Address.State));
            values.Add(new KeyValuePair<string, string>("valores[cep]", Address.ZipCode.ToString()));
            values.Add(new KeyValuePair<string, string>("valores[email]", Email));
            values.Add(new KeyValuePair<string, string>("valores[senha]", password));
            values.Add(new KeyValuePair<string, string>("valores[senha_confirmacao]", password));

            byte[] response = ExtendableType.Post(appSettings.UrlSei + "/pmj/cadastro_usuario_externo.php", values);

            var stringResponse = Encoding.GetEncoding(appSettings.SeiEncoding).GetString(response);
            var results = JsonConvert.DeserializeObject<dynamic>(stringResponse);

            if (results.status == 0)
            {
                var exception = new InvalidOperationException("Erro ao gerar a sua certificação.");
                exception.Data.Add("Response string", stringResponse);
            }
            else
            {
                SeiId = results.id_usuario;
            }
        }

        /// <summary>
        /// Altera a senha do usuário no SEI ignorando exceções de infraestrutura
        /// </summary>
        /// <param name="password">Nova senha do usuário</param>
        /// <param name="revokeSign">Caso True revoga a assinatura eletrônica do usuário</param>
        public void ChangePasswordSei(string password, AppSettings appSettings, bool revokeSign = false)
        {
            if (SeiId == null)
            {
                return;
            }

            var values = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("token", appSettings.Token),
                    new KeyValuePair<string, string>("id_usuario", SeiId.ToString()),
                    new KeyValuePair<string, string>("nova_senha", password)
            };

            if (revokeSign)
            {
                new KeyValuePair<string, string>("status", "P");
            }

            byte[] response = ExtendableType.Post(appSettings.UrlSei + "/pmj/cadastro_usuario_externo_senha.php", values);

            var result = JsonConvert.DeserializeObject<dynamic>(Encoding.GetEncoding(appSettings.SeiEncoding).GetString(response));

            if (result.status != 1)
            {
                throw new Exception("Erro ao alterar a senha no SEI");
            }

            if (revokeSign)
            {
                EletronicSignatureStatus = EletronicSignatureStatus.Unsolicited;
            }
        }

        /// <summary>
        /// Atualiza o campo <see cref="EletronicSignatureStatus"/> caso usuário esteja criado no SEI.
        /// </summary>
        /// <remarks>
        /// Caso a situação da assinatura esteja como em aprovação <see cref="EletronicSignatureStatus.UnderApproval"/>, a mesma não pode ser atualizada para não solicitada <see cref="EletronicSignatureStatus.Unsolicited"/>.
        /// </remarks>
        public void UpdateEletronicSignatureStatus(AppSettings appSettings)
        {
            if (SeiId != null)
            {
                Person seiPerson = GetSeiPersonBy((long)SeiId, appSettings);
                if (seiPerson != null)
                {
                    if (EletronicSignatureStatus == EletronicSignatureStatus.UnderApproval && seiPerson.EletronicSignatureStatus == EletronicSignatureStatus.Unsolicited)
                    {
                        return;
                    }

                    EletronicSignatureStatus = seiPerson.EletronicSignatureStatus;
                }
            }
        }

        /// <summary>
        /// Mostra se a pessoa possui uma assinatura eletrônica aprovada
        /// </summary>
        /// <remarks>
        /// Consulta diretamente a situação do usuário no SEI
        /// </remarks>
        /// <returns>True caso a assinatura eletrônica esteja aprovada</returns>
        public bool SignatureIsAproved(AppSettings appSettings)
        {
            if (SeiId != null)
            {
                Person seiPerson = Person.GetSeiPersonBy((long)SeiId, appSettings);
                if (seiPerson != null)
                {
                    return seiPerson.EletronicSignatureStatus == EletronicSignatureStatus.Approved;
                }
            }

            return false;
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

        private static Person GetSeiPersonBy(string key, string value, AppSettings appSettings)
        {
            Person person = null;

            var values = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("token", appSettings.Token),
                new KeyValuePair<string, string>(key, value)
            };

            var response = ExtendableType.Post(appSettings.UrlSei + "/pmj/cadastro_usuario_externo_consulta.php", values);

            string strResponse = Encoding.GetEncoding(appSettings.SeiEncoding).GetString(response);
            person = DeserializePersonJson(person, strResponse);

            return person;
        }
    }
}
