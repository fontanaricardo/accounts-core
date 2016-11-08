using Accounts.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Accounts.Models
{
    /// <summary>
    /// Acessos do usuário aos aplicativos e sua situação
    /// </summary>
    public class Access : IValidatableObject
    {
        /// <summary>
        /// Identificador único do acesso
        /// </summary>
        [Key]
        [Display(Name="Identificador")]
        public int AccessId { get; set; }

        /// <summary>
        /// Situação atual do acesso do usuário ao aplicativo.
        /// </summary>
        [Display(Name="Situação")]
        public AccessStatus Status { get; private set; }
        
        [Display(Name="Termos de uso aceitos")]
        public bool? AcceptedTerms { get; set; }

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
        /// Data da aprovação do acesso pelo usuário interno da prefeitura, atualizado automaticamente na aprovação do acesso.
        /// </summary>
        [Display(Name = "Aprovado em")]
        public DateTime? ApprovedAt { get; private set; }

        /// <summary>
        /// Login do funcionário que efetuou a aprovação ao acesso.
        /// </summary>
        [RegularExpression(@"^(u\d{5}|)$", ErrorMessage = "Login incorreto, informe o login do usuário no formato uXXXXXX")]
        [Display(Name = "Aprovado por")]
        public string ApprovedBy { get; private set; }

        /// <summary>
        /// Data da revogação do acesso, atualizado automaticamente na revogação do acesso.
        /// </summary>
        [Display(Name = "Negado em")]
        public DateTime? DeniedAt { get; private set; }

        /// <summary>
        /// Login do usuário que revogou o acesso.
        /// </summary>
        [Display(Name = "Negado por")]
        [RegularExpression(@"^(u\d{5}|)$", ErrorMessage = "Login incorreto, informe o login do usuário no formato uXXXXXX")]
        public string DeniedBy { get; private set; }

        /// <summary>
        /// Motivo da revogação, este campo é obrigatório caso o acesso venha a ser negado.
        /// </summary>
        [DataType(DataType.MultilineText)]
        [Display(Name = "Motivo da negação de acesso")]
        public string DeniedCause { get; private set; }
        
        /// <summary>
        /// Documento do usuário (CPF ou CNPJ)
        /// </summary>
        [Required, MaxLength(14), MinLength(11)]
        [Display(Name = "Documento do usuário (CPF ou CNPJ)")]
        public string Document { get; set; }

        /// <summary>
        /// Aplicação relacionada relacionada ao acesso
        /// </summary>
        [Required]
        public Application Application { get; set; }

        #region private methods

        /// <summary>
        /// Atualiza os campos de timestamp ao persistir o objeto
        /// </summary>
        private void UpdateTimeStamp()
        {
            UpdatedAt = DateTime.Now;
            CreatedAt = AccessId == 0 ? DateTime.Now : CreatedAt;
        }

        /// <summary>
        /// Caso status do acesso seja diferente de aprovado ou negado, 
        /// este método alimenta o mesmo conforme as características da aplicação.
        /// </summary>
        /// <exception cref="System.NullReferenceException">Exceção disparada caso a propriedade Application esteja nula</exception>
        private void UpdateStatus()
        {
            if (Status != AccessStatus.Performed)
            {
                return;
            }

            if (Application == null)
            {
                throw new NullReferenceException("Este acesso não possui aplicação relacionada.");
            }

            if (Application.RequiresApproval)
            {
                Status = AccessStatus.PendingRequest;
            }
            else
            {
                Status = AccessStatus.Performed;
            }
        }

        /// <summary>
        /// Atualiza a coluna accepted terms conforme dados da aplicação
        /// </summary>
        private void UpdateAcceptedTerms()
        {
            if (AcceptedTerms == true) return;
            else if (string.IsNullOrWhiteSpace(Application.UseTerms)) AcceptedTerms = null;
            else AcceptedTerms = false;
        }
        
        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (!Application.Enabled)
            {
                results.Add(new ValidationResult("Não é possível definir o acesso de uma aplicação desabilitada."));
            }
            
            if (results.Count == 0)
            {
                UpdateTimeStamp();
                UpdateStatus();
                UpdateAcceptedTerms();
            }

            return results;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Aprova o acesso de um usuário a um determinado aplicativo
        /// </summary>
        /// <remarks>
        /// Após efetuada esta operação é necessário persistir os dados no banco
        /// </remarks>
        /// <param name="login">Login do funcionário que está efetuando a aprovação no formato uXXXX</param>
        /// <exception cref="System.InvalidOperationException">Disparada a exceção caso a situação não seja passível de aprovação</exception>
        public void Approve(string login)
        {
            if (Status == AccessStatus.Approved)
            {
                throw new InvalidOperationException("Este acesso já foi aprovado.");
            }

            if (Status != AccessStatus.Requested)
            {
                throw new InvalidOperationException("Não é possível aprovar acessos com situação diferente de requerido.");
            }
            
            ApprovedBy = login;
            ApprovedAt = DateTime.Now;
            Status = AccessStatus.Approved;
        }

        /// <summary>
        /// Cancela o acesso do usuário ao um determinado aplicativo
        /// </summary>
        /// <remarks>
        /// Após efetuada esta operação é necessário persistir os dados no banco
        /// </remarks>
        /// <param name="login">Login do funcionário que está efetuando o cancelamento do acesso no formato uXXXX</param>
        /// <param name="cause">Motivo do cancelamento do acesso</param>
        /// <exception cref="System.InvalidOperationException">Disparada a exceção caso o acesso já tenha sido cancelado</exception>
        public void Deny(string login, string cause)
        {
            if (Status == AccessStatus.Denied)
            {
                throw new InvalidOperationException("Este acesso já foi negado.");
            }

            DeniedBy = login;
            DeniedAt = DateTime.Now;
            Status = AccessStatus.Denied;
            DeniedCause = cause;
        }

        /// <summary>
        /// Atualiza o status do acesso para requested ou performed conforme definido no campo RequiresApproval da aplicação
        /// </summary>
        public void RequestAccess()
        {
            Status = Application.RequiresApproval ? AccessStatus.Requested : AccessStatus.Performed;
        }

        /// <summary>
        /// Verifica se o usuário possui acesso a aplicação,
        /// </summary>
        /// <remarks>
        /// Carregar relacionamento a Application antes de executar o método
        /// </remarks>
        /// <param name="userDoc">CPF ou CNPJ do usuário</param>
        /// <param name="message">Mensagem de erro, caso não haja erro retorna nulo</param>
        /// <returns>True caso seja permitido o acesso do usuário</returns>
        public bool CheckAccess(string userDoc, out string message)
        {
            message = null;

            if (Status == AccessStatus.Denied)
            {
                message = "O seu acesso a aplicação foi revogado.";
                return false;
            }
            else if (Status == AccessStatus.PendingRequest)
            {
                message = "Seu acesso deve ser submetido a aprovação.";
                return false;
            }
            else if (AcceptedTerms == false)
            {
                message = "Você deve aceitar os termos de uso para ter acesso a aplicação.";
                return false;
            }
            else if (Status == AccessStatus.Requested)
            {
                message = "Solicitação de acesso efetuada em "
                    + CreatedAt.ToString("dd/MM/yyyy")
                    + ", você receberá um e-mail notificando sobre o resultado da solicitação.";
                return false;
            }

            return true;
        }
        
        #endregion
    }

    /// <summary>
    /// Status da solicitação de acesso feita pelo usuário
    /// </summary>
    public enum AccessStatus
    {
        /// <summary>
        /// O usuário efetuou acesso a uma aplicação que não possui nenhum tipo de restrição
        /// </summary>
        [Display(Name = "Efetuado")]
        Performed,

        /// <summary>
        /// O usuário ainda não efetuou a requisição de acesso a uma aplicação com restrição de acesso
        /// </summary>
        [Display(Name = "Pendente de requisição de acesso")]
        PendingRequest,

        /// <summary>
        /// O acesso foi requisitado pelo usuário, porém ainda não foi aprovado
        /// </summary>
        [Display(Name = "Acesso requisitado")]
        Requested,
        
        /// <summary>
        /// O acesso foi aprovado por um funcionário
        /// </summary>
        [Display(Name = "Aprovado")]
        Approved,

        /// <summary>
        /// O acesso foi negado ao usuário por um funcionário da prefeitura, 
        /// para este caso deve ser preenchido o campo de motivo da negação
        /// </summary>
        [Display(Name = "Negado")]
        Denied
    }
}
