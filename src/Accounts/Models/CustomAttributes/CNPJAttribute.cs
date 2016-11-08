using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Accounts.CustomAttributes
{
    /// <summary>
    /// Verifica se o atributo é um CNPJ válido
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class CNPJAttribute : ValidationAttribute
    {
        public CNPJAttribute() : base("CNPJ Inválido") { }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            value = value ?? string.Empty;

            var memberNames = new List<string>();
            if (context != null) memberNames.Add(context.MemberName);

            if (!ValidarCNPJ(value.ToString()))
            {
                return new ValidationResult(ErrorMessage, memberNames);
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Verifica o CNPJ informado
        /// http://www.devmedia.com.br/validacao-de-cpf-e-cnpj/3950#ixzz3VaJhirWN
        /// </summary>
        /// <param name="cnpj"></param>
        /// <returns></returns>
        public static bool ValidarCNPJ(string cnpj)
        {
            cnpj = cnpj ?? string.Empty;

            string CNPJ = cnpj.Replace(".", "");
            CNPJ = CNPJ.Replace("/", "");
            CNPJ = CNPJ.Replace("-", "");

            if (CNPJ.ToCharArray().Distinct().Count() == 1) return false;

            int[] digitos, soma, resultado;
            int nrDig; string ftmt;
            bool[] CNPJOk; ftmt = "6543298765432";
            digitos = new int[14];
            soma = new int[2];
            soma[0] = 0;
            soma[1] = 0;
            resultado = new int[2];
            resultado[0] = 0;
            resultado[1] = 0;
            CNPJOk = new bool[2];
            CNPJOk[0] = false;
            CNPJOk[1] = false;

            try
            {
                for (nrDig = 0; nrDig < 14; nrDig++)
                {
                    digitos[nrDig] = int.Parse(CNPJ.Substring(nrDig, 1));
                    if (nrDig <= 11) soma[0] += (digitos[nrDig] * int.Parse(ftmt.Substring(nrDig + 1, 1)));
                    if (nrDig <= 12) soma[1] += (digitos[nrDig] * int.Parse(ftmt.Substring(nrDig, 1)));
                }
                for (nrDig = 0; nrDig < 2; nrDig++)
                {
                    resultado[nrDig] = (soma[nrDig] % 11);
                    if ((resultado[nrDig] == 0) || (resultado[nrDig] == 1))
                        CNPJOk[nrDig] = (digitos[12 + nrDig] == 0);
                    else
                        CNPJOk[nrDig] = (digitos[12 + nrDig] == (11 - resultado[nrDig]));
                }
                return (CNPJOk[0] && CNPJOk[1]);
            }
            catch
            {
                return false;
            }
        }

    }
}
