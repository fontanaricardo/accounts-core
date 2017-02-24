namespace Accounts.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using Accounts.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    public class SeiService : ISeiService
    {
        private IConfigurationRoot _configuration;

        public SeiService(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Add an document to a existing SEI protocol.
        /// </summary>
        /// <param name="protocol">Formated protocol number, example 16.0.019934-5.</param>
        /// <param name="title">Title of the document.</param>
        /// <param name="document">The document to be added.</param>
        public void AddDocument(string protocol, string title, IFormFile document)
        {
            using (var formData = new MultipartFormDataContent())
                {
                    byte[] content = null;
                    BinaryReader reader = new BinaryReader(document.OpenReadStream());
                    content = reader.ReadBytes((int)document.Length);

                    formData.Add(new StringContent(protocol), "procFormatado");
                    formData.Add(new StringContent(_configuration["Unidade"]), "idUnidade");
                    formData.Add(new ByteArrayContent(content), "file", title + ".pdf");
                    formData.Add(new StringContent(title), "descricao");
                    formData.Add(new StringContent(_configuration["Anexo"]), "idSerie");
                    var resp = ExtendableType.Post(_configuration["VirtualUrl"] + "/SeiDocumentos/Create", title, formData);
                }
        }

        /// <summary>
        /// Add and text document to an existing sei protocol.
        /// </summary>
        /// <param name="protocol">Formated protocol number, example 16.0.019934-5.</param>
        /// <param name="title">Title of the document.</param>
        /// <param name="content">The content, in plain text, of the document. If null or empty the method does nothing.</param>
        public void AddTextDocument(string protocol, string title, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            using (var formData = new System.Net.Http.MultipartFormDataContent())
            {
                byte[] toBytes = Encoding.GetEncoding(_configuration["SeiEncoding"]).GetBytes(content);
                var fileName = title.RemoveDiacritics().ToLowerInvariant().Replace(' ', '_');

                formData.Add(new StringContent(protocol), "procFormatado");
                formData.Add(new StringContent(_configuration["Unidade"]), "idUnidade");
                formData.Add(new ByteArrayContent(toBytes), "file", fileName + ".txt");
                formData.Add(new StringContent(title), "descricao");
                formData.Add(new StringContent(_configuration["Formulario"]), "idSerie");
                var resp = ExtendableType.Post(_configuration["VirtualUrl"] + "/SeiDocumentos/Create", title, formData);
            }
        }

        /// <summary>
        /// Create an SEI protocol an add the person as interested.
        /// </summary>
        /// <remarks>
        /// The fields SeiProtocol and LinkSeiProtocol will be filled with protocol info.
        /// </remarks>
        /// <param name="person">Person to be added as interested.</param>
        public void CreateProtocol(Person person)
        {
            var param = JsonConvert.SerializeObject(new
                {
                    IdUnidade = _configuration["Unidade"],
                    IdProcedimento = _configuration["Procedimento"],
                    IdTipoProcedimento = _configuration["TipoProcedimento"],
                    IdServico = _configuration["Servico"],
                    Interessados = new[]
                    {
                        new
                        {
                            siglaField = person.Email,
                            nomeField = person.Name
                        }
                    }
                });

                HttpContent contentPost = new StringContent(param, Encoding.UTF8, "application/json");

                var response = ExtendableType.Post(
                    url: _configuration["VirtualUrl"] + "/api/Seiprotocolos/Gerar",
                    data: contentPost);

                var strResult = Encoding.GetEncoding(_configuration["SeiEncoding"]).GetString(response);

                var result = JsonConvert.DeserializeObject<dynamic>(strResult);

                person.SeiProtocol = result.ProcedimentoFormatado;
                person.LinkSeiProtocol = result.Link;
        }

        /// <summary>
        /// Reopen an existing SEI paramref name="protocol".
        /// </summary>
        /// <param name="protocol">Formated protocol number, example 16.0.019934-5</param>
        public void ReopenProtocol(string protocol)
        {
            var values = new List<KeyValuePair<string, string>>();

            values.Add(new KeyValuePair<string, string>("IdUnidade", _configuration["Unidade"]));
            values.Add(new KeyValuePair<string, string>("ProcedimentoFormatado", protocol));
            values.Add(new KeyValuePair<string, string>("IdTipoProcedimento", _configuration["TipoProcedimento"]));
            values.Add(new KeyValuePair<string, string>("IdServico", _configuration["Servico"]));

            var response = ExtendableType.Post(_configuration["VirtualUrl"] + "/api/Seiprotocolos/Reabrir", values);
        }

        /// <summary>
        /// Changes user's password in SEI ignoring infrastructure exceptions.
        /// </summary>
        /// <remarks>
        /// If person.SeiId is null do nothing.
        /// </remarks>
        /// <param name="person">Person who will have his password changed.</param>
        /// <param name="password">New user password.</param>
        /// <param name="revokeSign">If true revoke user's signature.</param>
        public void ChangePasswordSei(Person person, string password, bool revokeSign = false)
        {
            if (person.SeiId == null)
            {
                return;
            }

            var values = new List<KeyValuePair<string, string>>
            {
                    new KeyValuePair<string, string>("token", _configuration["Token"]),
                    new KeyValuePair<string, string>("id_usuario", person.SeiId.ToString()),
                    new KeyValuePair<string, string>("nova_senha", password)
            };

            if (revokeSign)
            {
                new KeyValuePair<string, string>("status", "P");
            }

            byte[] response = ExtendableType.Post(_configuration["UrlSei"] + "/pmj/cadastro_usuario_externo_senha.php", values);

            var result = JsonConvert.DeserializeObject<dynamic>(Encoding.GetEncoding(_configuration["SeiEncoding"]).GetString(response));

            if (result.status != 1)
            {
                throw new Exception("Erro ao alterar a senha no SEI");
            }

            if (revokeSign)
            {
                person.EletronicSignatureStatus = EletronicSignatureStatus.Unsolicited;
            }
        }

        /// <summary>
        /// Returns the person object from the SEI data. Use the service /pmj/cadastro_usuario_externo_consulta.php
        /// </summary>
        /// <param name="id">SEI user's code.</param>
        /// <returns>Object with person SEI data.</returns>
        public Person GetSeiPersonBy(long id)
        {
            return GetSeiPersonBy("id_usuario", id.ToString());
        }

        /// <summary>
        /// Returns the person object from the SEI data. Use the service /pmj/cadastro_usuario_externo_consulta.php
        /// </summary>
        /// <param name="email">SEI user's e-mail.</param>
        /// <returns>Object with person SEI data.</returns>
        public Person GetSeiPersonBy(string email)
        {
            return GetSeiPersonBy("email", email);
        }

        /// <summary>
        /// Create or update an SEI user.
        /// </summary>
        /// <param name="person">Person to be created on the SEI.</param>
        /// <param name="password">SEI user's password.</param>
        public void CreateOrUpdateSeiUser(Person person, string password)
        {
            if (person.Phones == null || person.Phones.Count() == 0)
            {
                throw new ArgumentNullException(null, "Deve ser informado o número de telefone");
            }

            if (person.Address == null)
            {
                throw new ArgumentNullException("Address", "Endereço não pode ficar em branco.");
            }

            // Token to service authentication
            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();

            values.Add(new KeyValuePair<string, string>("token", _configuration["Token"]));

            Person seiPerson = GetSeiPersonBy(person.Email);

            if (seiPerson != null)
            {
                if (!person.CPF.Equals(seiPerson.CPF))
                {
                    throw new ArgumentException("Este e-mail está relacionado a outro CPF no SEI");
                }
                else
                {
                    person.SeiId = seiPerson.SeiId;
                }
            }

            if (person.SeiId != null)
            {
                values.Add(new KeyValuePair<string, string>("valores[id_usuario]", person.SeiId.ToString()));
            }

            // PHP's vector format
            values.Add(new KeyValuePair<string, string>("valores[nome]", person.Name));
            values.Add(new KeyValuePair<string, string>("valores[cpf]", person.CPF));
            values.Add(new KeyValuePair<string, string>("valores[rg]", person.RG));
            values.Add(new KeyValuePair<string, string>("valores[orgao_expedidor]", person.Dispatcher));
            values.Add(new KeyValuePair<string, string>("valores[telefone]", person.PhoneLastUpdated));
            values.Add(new KeyValuePair<string, string>("valores[endereco]", person.Address.StreetAndNumber));
            values.Add(new KeyValuePair<string, string>("valores[bairro]", person.Address.District));
            values.Add(new KeyValuePair<string, string>("valores[cidade]", person.Address.City));
            values.Add(new KeyValuePair<string, string>("valores[estado]", person.Address.State));
            values.Add(new KeyValuePair<string, string>("valores[cep]", person.Address.ZipCode.ToString()));
            values.Add(new KeyValuePair<string, string>("valores[email]", person.Email));
            values.Add(new KeyValuePair<string, string>("valores[senha]", password));
            values.Add(new KeyValuePair<string, string>("valores[senha_confirmacao]", password));

            byte[] response = ExtendableType.Post(_configuration["UrlSei"] + "/pmj/cadastro_usuario_externo.php", values);

            var stringResponse = Encoding.GetEncoding(_configuration["SeiEncoding"]).GetString(response);
            var results = JsonConvert.DeserializeObject<dynamic>(stringResponse);

            if (results.status == 0)
            {
                var exception = new InvalidOperationException("Erro ao gerar a sua certificação.");
                exception.Data.Add("Response string", stringResponse);
            }
            else
            {
                person.SeiId = results.id_usuario;
            }
        }

        /// <summary>
        /// Updates the field <see cref="EletronicSignatureStatus"/> if user created in the SEI.
        /// </summary>
        /// <remarks>
        /// If situation is <see cref="EletronicSignatureStatus.UnderApproval"/>, does nothing. <see cref="EletronicSignatureStatus.Unsolicited"/>.
        /// </remarks>
        public void UpdateEletronicSignatureStatus(Person person)
        {
            if (person.SeiId != null)
            {
                Person seiPerson = GetSeiPersonBy((long)person.SeiId);
                if (seiPerson != null)
                {
                    if (person.EletronicSignatureStatus == EletronicSignatureStatus.UnderApproval && seiPerson.EletronicSignatureStatus == EletronicSignatureStatus.Unsolicited)
                    {
                        return;
                    }

                    person.EletronicSignatureStatus = seiPerson.EletronicSignatureStatus;
                }
            }
        }

        /// <summary>
        /// Check if person has approved eletronic signature.
        /// </summary>
        /// <remarks>
        /// Query the SEI situation directly.
        /// </remarks>
        /// <returns>True if is approved./returns>
        public bool SignatureIsAproved(Person person)
        {
            if (person.SeiId != null)
            {
                Person seiPerson = GetSeiPersonBy((long)person.SeiId);
                if (seiPerson != null)
                {
                    return seiPerson.EletronicSignatureStatus == EletronicSignatureStatus.Approved;
                }
            }

            return false;
        }

        private Person GetSeiPersonBy(string key, string value)
        {
            Person person = null;

            var values = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("token", _configuration["Token"]),
                new KeyValuePair<string, string>(key, value)
            };

            var response = ExtendableType.Post(_configuration["UrlSei"] + "/pmj/cadastro_usuario_externo_consulta.php", values);

            string strResponse = Encoding.GetEncoding(_configuration["SeiEncoding"]).GetString(response);
            person = Person.DeserializePersonJson(person, strResponse);

            return person;
        }
    }
}