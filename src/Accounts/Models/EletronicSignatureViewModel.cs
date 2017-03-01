namespace Accounts.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

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
        /// Generate document with user data changes, for sending to SEI.
        /// </summary>
        /// <param name="oldPerson">Object with unchanged data.</param>
        /// <param name="newPerson">Object with new data.</param>
        /// <returns>Document with the data changed, or null if there is no change.</returns>
        public static string UserDataChangeDocument(Person oldPerson, Person newPerson)
        {
            string[] ignoredProperties = { "EletronicSignatureStatus" };
            var diff = newPerson.Diff(oldObject: oldPerson, ignoredProperties: ignoredProperties);

            if (diff.Count == 0)
            {
                return null;
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

            return fileContent.ToString();
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

        public string UserDataDocument(Person person)
        {
            StringBuilder fileContent = new StringBuilder();

            fileContent.AppendLine("Dados do usuário externo");
            fileContent.AppendLine("========================");
            fileContent.AppendLine(string.Empty);
            fileContent.Append(person.ToString());
            fileContent.AppendLine("Telefones: ");
            person.Phones.ForEach(p => fileContent.AppendLine(p.Number));

            return fileContent.ToString();
        }
    }
}