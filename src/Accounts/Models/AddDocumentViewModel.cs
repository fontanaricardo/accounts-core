using Accounts.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;


namespace Accounts.Models
{
    /// <summary>
    /// Utilizada para envio de documentos adicionais, para o SEI, durante o processo de análise do certificado
    /// </summary>
    public class AddDocumentViewModel : IValidatableObject
    {
        /// <summary>
        /// Documento a ser enviado para o SEI
        /// </summary>
        [Display(Name = "Documento")]
        public IFormFile Document { get; set; }

        /// <summary>
        /// Adiciona o documento ao protocolo do usuário no SEI
        /// </summary>
        /// <param name="person"></param>
        public void AddDocumentsToProtocol(Person person, AppSettings appSettings)
        {
            Dictionary<string, IFormFile> files = new Dictionary<string, IFormFile>(){
                { "Documento" , Document }
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
        
        /// <summary>
        /// Efetua validações conforme <see cref="Accounts.Models.FileValidator.Validate(ValidationContext)"/>
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var file = new FileValidator(Document);
            file.FileType = "Documento";
            return file.Validate(validationContext);
        }

    }
}