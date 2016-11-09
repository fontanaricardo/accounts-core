namespace Accounts.CustomAttributes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    /// Verifica se o atributo é um CNPJ válido
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class CNPJAttribute : ValidationAttribute
    {
        public CNPJAttribute()
            : base("CNPJ Inválido")
        {
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

            string newCNPJ = cnpj.Replace(".", string.Empty);
            newCNPJ = newCNPJ.Replace("/", string.Empty);
            newCNPJ = newCNPJ.Replace("-", string.Empty);

            if (newCNPJ.ToCharArray().Distinct().Count() == 1)
            {
                return false;
            }

            int[] digitos, soma, resultado;
            int nrDig;
            string ftmt;
            bool[] okCNPJ;
            ftmt = "6543298765432";
            digitos = new int[14];
            soma = new int[2];
            soma[0] = 0;
            soma[1] = 0;
            resultado = new int[2];
            resultado[0] = 0;
            resultado[1] = 0;
            okCNPJ = new bool[2];
            okCNPJ[0] = false;
            okCNPJ[1] = false;

            try
            {
                for (nrDig = 0; nrDig < 14; nrDig++)
                {
                    digitos[nrDig] = int.Parse(newCNPJ.Substring(nrDig, 1));
                    if (nrDig <= 11)
                    {
                        soma[0] += digitos[nrDig] * int.Parse(ftmt.Substring(nrDig + 1, 1));
                    }

                    if (nrDig <= 12)
                    {
                        soma[1] += digitos[nrDig] * int.Parse(ftmt.Substring(nrDig, 1));
                    }
                }

                for (nrDig = 0; nrDig < 2; nrDig++)
                {
                    resultado[nrDig] = soma[nrDig] % 11;
                    if ((resultado[nrDig] == 0) || (resultado[nrDig] == 1))
                    {
                        okCNPJ[nrDig] = digitos[12 + nrDig] == 0;
                    }
                    else
                    {
                        okCNPJ[nrDig] = digitos[12 + nrDig] == (11 - resultado[nrDig]);
                    }
                }

                return okCNPJ[0] && okCNPJ[1];
            }
            catch
            {
                return false;
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            value = value ?? string.Empty;

            var memberNames = new List<string>();
            if (context != null)
            {
                memberNames.Add(context.MemberName);
            }

            if (!ValidarCNPJ(value.ToString()))
            {
                return new ValidationResult(ErrorMessage, memberNames);
            }

            return ValidationResult.Success;
        }
    }
}
