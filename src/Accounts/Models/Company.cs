using Accounts.CustomAttributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Accounts.Extensions;

namespace Accounts.Models
{
    public class Company : IValidatableObject
    {
        private string _name;
        private string _companyName;

        [Key]
        public int CompanyID { get; set; }

        [Display(Name = "CNPJ")]
        [Required, MaxLength(14), MinLength(14), CNPJ]
        public string CNPJ { get; set; }

        [Required]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
        
        [Required, MaxLength(2000)]
        [Display(Name = "Nome fantasia")]
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _name = value.ToNameCase();
                }
            }
        }
        
        [Required, MaxLength(2000)]
        [Display(Name = "Razão social")]
        public string CompanyName
        {
            get
            {
                return _companyName;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _companyName = value.ToNameCase();
                }
            }
        }

        [Display(Name = "Inscrição municipal")]
        public string MunicipalRegistration { get; set; }

        [Required]
        [ForeignKey("Address")]
        public int AddressID { get; set; }
        
        public Address Address { get; set; }
        
        public ICollection<Access> Access { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = new List<ValidationResult>();

            RegexUtilities rUtils = new RegexUtilities();

            if (!rUtils.IsValidEmail(Email))
            {
                results.Add(new ValidationResult("E-mail inválido", new String[] { "Email" }));
            }

            return results;
        }
    }
}
