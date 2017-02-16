namespace Accounts.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Services;

    [NotMapped]
    public class EletronicSignatureViewModel : IValidatableObject
    {
        [Display(Name = "Declaro que lí e concordo com o artigo nº 229")]
        public bool Agree { get; set; }

        [Display(Name = "Termo de responsabilidade assinado")]
        public IFormFile Term { get; set; }

        [Display(Name = "Documento com foto")]
        public IFormFile Document { get; set; }

        [Required(ErrorMessage = "Este campo é obrigatório.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        /// <summary>
        /// Registra as alterações dos dados de usuários no protocolo no SEI.
        /// </summary>
        /// <remarks>
        /// Caso os objetos sejam idênticos, nenhuma ação é tomada.
        /// </remarks>
        /// <param name="seiProtocol">Número formatado do protocolo do SEI que irá receber o documento com as alterações.</param>
        /// <param name="oldPerson">Objeto com os dados anteriores.</param>
        /// <param name="newPerson">Objeto com os novos dados.</param>
        /// <param name="appSettings">Objeto com as configurações do ambiente.</param>
        public static void AddUserDataChange(string seiProtocol, Person oldPerson, Person newPerson, AppSettings appSettings)
        {
            string[] ignoredProperties = { "EletronicSignatureStatus" };
            var diff = newPerson.Diff(oldObject: oldPerson, ignoredProperties: ignoredProperties);

            if (diff.Count == 0)
            {
                return;
            }

            StringBuilder fileContent = new StringBuilder();

            fileContent.AppendLine("Alteração nos dados do usuário externo");
            fileContent.AppendLine("======================================");
            fileContent.AppendLine(string.Empty);

            diff.ForEach(t =>
            {
                var line = $"{t.Item1}: {t.Item2} => {t.Item3}";
                fileContent.Append(line);
            });

            SendTextDocument(seiProtocol, "Alteração nos dados do usuário", fileContent.ToString(), appSettings);
        }

        public static void SendTextDocument(string seiProtocol, string title, string fileContent, AppSettings appSettings)
        {
            using (var formData = new System.Net.Http.MultipartFormDataContent())
            {
                byte[] toBytes = Encoding.GetEncoding(appSettings.SeiEncoding).GetBytes(fileContent);
                var fileName = title.RemoveDiacritics().ToLowerInvariant().Replace(' ', '_');

                formData.Add(new StringContent(seiProtocol), "procFormatado");
                formData.Add(new StringContent(appSettings.Unidade), "idUnidade");
                formData.Add(new ByteArrayContent(toBytes), "file", fileName + ".txt");
                formData.Add(new StringContent(title), "descricao");
                formData.Add(new StringContent(appSettings.Formulario), "idSerie");
                var resp = ExtendableType.Post(appSettings.VirtualUrl + "/SeiDocumentos/Create", title, formData);
            }
        }

        public void ChangePassword(Person person, AppSettings appSettings)
        {
            person.ChangePasswordSei(Password, appSettings);
        }

        /// <summary>
        /// Cria ou reabre o protocolo do usuário no SEI
        /// </summary>
        /// <param name="person">
        /// Usuário relacionado a
        /// </param>
        public void CreateOrReopenProtocol(Person person, AppSettings appSettings)
        {
            if (string.IsNullOrEmpty(person.SeiProtocol))
            {
                CreateProtocol(person, appSettings);
            }
            else
            {
                ReopenProtocol(person, appSettings);
            }
        }

        /// <summary>
        /// Anexando documentos e dados do usuário ao protocolo do SEI
        /// </summary>
        public void AddDocumentsAndUserData(Person person, AppSettings appSettings)
        {
            AddUserData(person, appSettings);
            AddDocumentsToProtocol(person, appSettings);
        }

        public void CreateOrUpdateProtocol(Person person, AppSettings appSettings)
        {
            if (string.IsNullOrEmpty(person.SeiProtocol))
            {
                CreateProtocol(person, appSettings);
            }
            else
            {
                ReopenProtocol(person, appSettings);
            }

            // Anexando os documentos, os dados do usuário e atualizando o status
            AddUserData(person, appSettings);
            AddDocumentsToProtocol(person, appSettings);
            person.EletronicSignatureStatus = EletronicSignatureStatus.UnderApproval;
        }

        /// <summary>
        /// Verifica se o usuário aceitou os termos e efetua validações conforme <see cref="Accounts.Models.FileValidator.Validate(ValidationContext)"/>
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (!Agree)
            {
                results.Add(new ValidationResult("Você deve aceitar os termos dara dar continuidade no processo.", new string[] { "Agree" }));
            }

            // TODO: obter os rótulos a partir do Display name
            Dictionary<string, IFormFile> files = new Dictionary<string, IFormFile>()
            {
                { "Termo de responsabilidade", Term },
                { "Documento com foto", Document },
            };

            foreach (var file in files)
            {
                var fileValidator = new FileValidator(file.Value);
                fileValidator.FileType = file.Key;
                results.AddRange(fileValidator.Validate(validationContext));
            }

            return results;
        }

        /// <summary>
        /// Cria um protocolo no SEI adicionando a pessoa como interessado
        /// </summary>
        /// <remarks>
        /// Este método utiliza a api /api/Seiprotocolos/Gerar do Virtual para gerar o protocolo do SEI
        /// </remarks>
        /// <param name="person">
        /// Pessoa a ser adicionada como interessado
        /// </param>
        private void CreateProtocol(Person person, AppSettings appSettings)
        {
            using (var client = new HttpClient())
            {
                var param = JsonConvert.SerializeObject(new
                {
                    IdUnidade = appSettings.Unidade,
                    IdProcedimento = appSettings.Procedimento,
                    IdTipoProcedimento = appSettings.TipoProcedimento,
                    IdServico = appSettings.Servico,
                    Interessados = new[]
                    {
                        new
                        {
                            siglaField = person.Email, nomeField = person.Name
                        }
                    }
                });

                HttpContent contentPost = new StringContent(param, Encoding.UTF8, "application/json");

                var response = client.PostAsync(appSettings.VirtualUrl + "/api/Seiprotocolos/Gerar", contentPost).Result;

                var result = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                person.SeiProtocol = result.ProcedimentoFormatado;
                person.LinkSeiProtocol = result.Link;
            }
        }

        private void ReopenProtocol(Person person, AppSettings appSettings)
        {
            var values = new List<KeyValuePair<string, string>>();

            values.Add(new KeyValuePair<string, string>("IdUnidade", appSettings.Unidade));
            values.Add(new KeyValuePair<string, string>("ProcedimentoFormatado", person.SeiProtocol));
            values.Add(new KeyValuePair<string, string>("IdTipoProcedimento", appSettings.TipoProcedimento));
            values.Add(new KeyValuePair<string, string>("IdServico", appSettings.Servico));

            var response = ExtendableType.Post(appSettings.VirtualUrl + "/api/Seiprotocolos/Reabrir", values);
        }

        private void AddDocumentsToProtocol(Person person, AppSettings appSettings)
        {
            Dictionary<string, IFormFile> files = new Dictionary<string, IFormFile>()
            {
                { "Termo de responsabilidade", Term },
                { "Documento com foto", Document },
            };

            foreach (var file in files)
            {
                using (var formData = new MultipartFormDataContent())
                {
                    byte[] content = null;
                    BinaryReader reader = new BinaryReader(file.Value.OpenReadStream());
                    content = reader.ReadBytes((int)file.Value.Length);

                    formData.Add(new StringContent(person.SeiProtocol), "procFormatado");
                    formData.Add(new StringContent(appSettings.Unidade), "idUnidade");
                    formData.Add(new ByteArrayContent(content), "file", file.Key + ".pdf");
                    formData.Add(new StringContent(file.Key), "descricao");
                    formData.Add(new StringContent(appSettings.Anexo), "idSerie");
                    var resp = ExtendableType.Post(appSettings.VirtualUrl + "/SeiDocumentos/Create", file.Key, formData);
                }
            }
        }

        private void AddUserData(Person person, AppSettings appSettings)
        {
            StringBuilder fileContent = new StringBuilder();

            fileContent.AppendLine("Dados do usuário externo");
            fileContent.AppendLine("========================");
            fileContent.AppendLine(string.Empty);
            fileContent.Append(person.ToString());
            fileContent.AppendLine("Telefones: ");
            person.Phones.ForEach(p => fileContent.AppendLine(p.Number));
            SendTextDocument(person.SeiProtocol, "Dados do usuário", fileContent.ToString(), appSettings);
        }
    }
}