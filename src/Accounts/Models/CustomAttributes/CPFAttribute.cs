using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Accounts.CustomAttributes
{
    /// <summary>
    /// Verifica se o atributo é um CPF válido
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class CPFAttribute : ValidationAttribute
    {
        public CPFAttribute() : base("CPF Inválido") { }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var memberNames = new List<string>();
            if (context != null) memberNames.Add(context.MemberName);

            value = value ?? string.Empty;

            if (!ValidarCPF(value.ToString()))
            {
                return new ValidationResult(ErrorMessage, memberNames);
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Valida o CPF
        /// </summary>
        /// <param name="cpf">CPF em formato numérico ou não</param>
        /// <returns>True - CPF válido</returns>
        public static bool ValidarCPF(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            int[] mt1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mt2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string TempCPF;
            string Digito;
            int soma;
            int resto;

            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11)
                return false;

            if (cpf.ToCharArray().Distinct().Count() == 1) return false;

            TempCPF = cpf.Substring(0, 9);
            soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(TempCPF[i].ToString()) * mt1[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            Digito = resto.ToString();
            TempCPF = TempCPF + Digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(TempCPF[i].ToString()) * mt2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            Digito = Digito + resto.ToString();

            return cpf.EndsWith(Digito);
        }

    }
}
