namespace Accounts.CustomAttributes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    /// Verifica se o atributo é um CPF válido
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class CPFAttribute : ValidationAttribute
    {
        public CPFAttribute()
            : base("CPF Inválido")
        {
        }

        /// <summary>
        /// Valida o CPF
        /// </summary>
        /// <param name="cpf">CPF em formato numérico ou não</param>
        /// <returns>True - CPF válido</returns>
        public static bool ValidarCPF(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
            {
                return false;
            }

            int[] mt1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mt2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCPF;
            string digito;
            int soma;
            int resto;

            cpf = cpf.Trim();
            cpf = cpf.Replace(".", string.Empty).Replace("-", string.Empty);

            if (cpf.Length != 11)
            {
                return false;
            }

            if (cpf.ToCharArray().Distinct().Count() == 1)
            {
                return false;
            }

            tempCPF = cpf.Substring(0, 9);
            soma = 0;
            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(tempCPF[i].ToString()) * mt1[i];
            }

            resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito = resto.ToString();
            tempCPF = tempCPF + digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(tempCPF[i].ToString()) * mt2[i];
            }

            resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito = digito + resto.ToString();

            return cpf.EndsWith(digito);
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var memberNames = new List<string>();
            if (context != null)
            {
                memberNames.Add(context.MemberName);
            }

            value = value ?? string.Empty;

            if (!ValidarCPF(value.ToString()))
            {
                return new ValidationResult(ErrorMessage, memberNames);
            }

            return ValidationResult.Success;
        }
    }
}
