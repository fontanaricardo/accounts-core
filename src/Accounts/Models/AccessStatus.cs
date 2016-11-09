namespace Accounts.Models
{
    using System.ComponentModel.DataAnnotations;

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
