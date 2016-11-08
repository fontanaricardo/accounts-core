using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace System
{
    /// <summary>
    /// Cria métodos de extensão disponíveis em todos o projeto
    /// </summary>
    /// <remarks>
    /// Adaptado de: http://stackoverflow.com/a/17825129/5270073
    /// </remarks>
    public static partial class ExtendableType
    {
        private static HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Capitaliza nomes de pessoas (exemplo: JOSÉ DA SILVA para José da Silva)
        /// </summary>
        /// <remarks>
        /// Adaptado de: http://pt.stackoverflow.com/questions/247/capitalizando-nomes-em-c
        /// </remarks>
        /// <param name="input">String contendo o nome da pessoa</param>
        /// <returns>Nome no format capitalizado.</returns>
        public static string ToNameCase(this string input)
        {
            string[] exc = new string[] { "e", "de", "da", "das", "do", "dos" };
            var words = new Queue<string>();
            foreach (var word in input.Split(' '))
            {
                if (!string.IsNullOrEmpty(word))
                {
                    var down = word.ToLower();
                    var character = down.ToCharArray();
                    if (!exc.Contains(down)) character[0] = char.ToUpper(character[0]);
                    words.Enqueue(new string(character));
                }
            }
            return string.Join(" ", words);
        }

        public static byte[] Post(string url, List<KeyValuePair<string, string>> values)
        {
            var content = new FormUrlEncodedContent(values);
            var result = httpClient.PostAsync(url, content).Result;
            return result.Content.ReadAsByteArrayAsync().Result;
        }

        public static byte[] Post(string url, string fileName, MultipartFormDataContent data)
        {
            var resp = httpClient.PostAsync(url, data).Result;

            if (!resp.IsSuccessStatusCode)
            {
                throw new FileLoadException("Erro ao enviar o arquivo " + fileName);
            }

            return resp.Content.ReadAsByteArrayAsync().Result;
        }
    }
}
