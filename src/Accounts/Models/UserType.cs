namespace Accounts.Models
{
    using System.ComponentModel.DataAnnotations;

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
