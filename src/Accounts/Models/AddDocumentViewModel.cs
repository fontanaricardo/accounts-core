namespace Accounts.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Http;

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