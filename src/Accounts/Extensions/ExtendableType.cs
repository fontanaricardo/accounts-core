namespace System
{
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using Collections.Generic;
    using IO;
    using Linq;
    using Net.Http;

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
                    if (!exc.Contains(down))
                    {
                        character[0] = char.ToUpper(character[0]);
                    }

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

        /// <summary>
        /// Get de differences between two object properties.
        /// </summary>
        /// <remarks>
        /// Source: http://stackoverflow.com/a/4951271/5270073.
        /// </remarks>
        /// /// <param name="newObject">Object with new values.</param>
        /// <param name="oldObject">Object with old values.</param>
        /// <param name="ignoreKey">Ignore attributes marked with Key.</param>
        /// <returns>An tuple with Display Name atribute, or property name if Display attribute does not exists, the old value and new value.</returns>
        public static List<Tuple<string, object, object>> Diff<T>(this T newObject, T oldObject, bool ignoreKey = true)
        {
            var diff = new List<Tuple<string, object, object>>();

            var properties = newObject.GetType().GetProperties();
            foreach (var property in properties)
            {
                var name = property.Name;

                if (property != null)
                {
                    var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
                    if (displayAttr != null)
                    {
                        name = displayAttr != null ? displayAttr.Name : property.Name;
                    }

                    if (ignoreKey && (property.GetCustomAttribute<KeyAttribute>() != null))
                    {
                        continue;
                    }
                }

                var oldValue = property.GetValue(oldObject, null);
                var newValue = property.GetValue(newObject, null);

                if (!object.Equals(oldValue, newValue))
                {
                    diff.Add(new Tuple<string, object, object>(name, oldValue, newValue));
                }
            }

            return diff;
        }
    }
}
