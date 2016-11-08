using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Accounts.Extensions
{
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