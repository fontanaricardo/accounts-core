using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    /// <summary>
    /// Representa a aplicação para a qual o usuário irá solicitar acesso
    /// </summary>
    public class Application : IValidatableObject
    {
        /// <summary>
        /// Identificador único da aplicação
        /// </summary>
        [Key]
        [Display(Name = "Identificador")]
        public int ApplicationId { get; set; }

        /// <summary>
        /// Nome da aplicação com até 150 caracteres
        /// </summary>
        [Display(Name = "Nome")]
        [MinLength(5, ErrorMessage = "O nome da aplicação precisa ter no mínimo 5 caracteres")]
        [MaxLength(150, ErrorMessage = "O nome da aplicação não pode ser maior do que 150 caracteres")]
        public string ApplicationName { get; set; }

        /// <summary>
        /// Termos de uso da aplicação, caso não seja necessário ao usuário aceitar os termos, este campo deve ficar em branco
        /// </summary>
        [Display(Name = "Termos de uso")]
        [DataType(DataType.MultilineText)]
        public string UseTerms { get; set; }

        /// <summary>
        /// Define se o acesso deve ser aprovado internamente
        /// </summary>
        [Required]
        [Display(Name = "Requer aprovação")]
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Caso esteja desabilitada, não é possível acessar a aplicação
        /// </summary>
        [Required]
        [Display(Name = "Habilitada")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Define que tipo de usuário (pessoa, empresa ou todos) pode acessar a aplicação
        /// </summary>
        [Display(Name = "Tipo de usuário")]
        public UserType UserType { get; set; }

        /// <summary>
        /// Url de acesso a aplicação
        /// </summary>
        [Display(Name = "Endereço da aplicação")]
        public string Url { get; set; }

        /// <summary>
        /// Acesso relacionados a esta aplicação
        /// </summary>
        public ICollection<Access> Accesses { get; set; }

        /// <summary>
        /// Verifica se a aplição é destinada ao tipo do usuário (pessoa física ou jurídica)
        /// </summary>
        /// <param name="userDocument">CPF ou CNPJ do usuário</param>
        /// <param name="message">Mensagem de erro, caso não tenha erro este valor fica nulo</param>
        /// <returns>True se o usuário possui acesso</returns>
        public bool CheckAccess(string userDocument, out string message)
        {
            bool isPerson = userDocument.Length <= 11;
            message = null;
            
            if (!Enabled)
            {
                message = "A aplicação " + ApplicationName + " está desabilitada.";
                return false;
            }
            else if (UserType == UserType.Companies && isPerson)
            {
                message = "A aplicação " + ApplicationName + " pode ser acessada somente por usuários do tipo pessoa jurídica.";
                return false;
            }
            else if (UserType == UserType.People && !isPerson)
            {
                message = "A aplicação " + ApplicationName + " pode ser acessada somente por usuários do tipo pessoa física.";
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Data de criação do registro, alimentado automaticamente na criação do registro.
        /// </summary>
        [Required]
        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Data de atualização do registro, atualizado automaticamente na atualização do registro.
        /// </summary>
        [Required]
        [Display(Name = "Atualizado em")]
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Atualiza os campos de timestamp ao persistir o objeto
        /// </summary>
        private void UpdateTimeStamp()
        {
            UpdatedAt = DateTime.Now;
            CreatedAt = ApplicationId == 0 ? DateTime.Now : CreatedAt;
        }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            
            if (results.Count == 0)
            {
                UpdateTimeStamp();
            }

            return results;
        }

    }

    public enum UserType
    {
        [Display(Name = "Todos")]
        All,

        [Display(Name = "Pessoas")]
        People,

        [Display(Name = "Empresas")]
        Companies
    }
}
