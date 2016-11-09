namespace Accounts.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Efetua validações nos arquivos
    /// </summary>
    [NotMapped]
    public class FileValidator : IValidatableObject
    {
        private static readonly int MaxSizeMb = 1;
        private static readonly string[] Extensions = { ".pdf" };
        private IFormFile file;

        /// <summary>
        /// Cria uma nova instância de um validador do arquivo
        /// </summary>
        /// <param name="file">Arquivo a ser validado</param>
        public FileValidator(IFormFile file)
        {
            this.file = file;
        }

        /// <summary>
        /// Tipo do arquivo (exemplo: Termo de aceite ou Documento com foto)
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// Valida se o arquivo tem tamanho inferior a 1 Mb, se é um arquivo vazio ou se tem extensão PDF
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = new List<ValidationResult>();

            if (file == null)
            {
                results.Add(new ValidationResult(string.Format("O arquivo \"{0}\" é obrigatório.", FileType)));
            }
            else
            {
                if (file.ContentDisposition.Length == default(int))
                {
                    results.Add(new ValidationResult(string.Format("O arquivo \"{0}\" não possui conteúdo.", FileType)));
                }

                if (file.ContentDisposition.Length > MaxSizeMb * 1024 * 1024)
                {
                    results.Add(new ValidationResult(string.Format("O arquivo \"{0}\" tem tamanho superior a {1} Mb.", FileType, MaxSizeMb)));
                }

                if (!Extensions.Contains(Path.GetExtension(file.FileName)))
                {
                    results.Add(new ValidationResult(string.Format("O arquivo \"{0}\" possui extensão diferente de \"{1}\"", FileType, string.Join(" ", Extensions))));
                }
            }

            return results;
        }
    }
}