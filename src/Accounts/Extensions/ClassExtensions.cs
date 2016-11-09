namespace Accounts.Extensions
{
    using System.Collections;
    using System.Reflection;
    using System.Text;

    public static class ClassExtensions
    {
        public static string PropertyList(this object obj)
        {
            var props = obj.GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var p in props)
            {
                // Caso seja uma collection ignora
                if (!typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                {
                    sb.AppendLine(p.Name + ": " + p.GetValue(obj, null));
                }
            }

            return sb.ToString();
        }
    }
}