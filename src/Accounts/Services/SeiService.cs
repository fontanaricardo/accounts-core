namespace Accounts.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
    }
}