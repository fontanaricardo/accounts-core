using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Accounts.Unit.Controllers
{
    public class CheckAccessControl
    {
        private const string nspace = "Accounts.Controllers";

        /// <summary>
        /// Todos os controllers devem estar marcados com AuthorizeAttribute
        /// </summary>
        [Fact]
        public void EveryControllerShouldHaveAuthorize()
        {
            Assembly.Load(new AssemblyName("Accounts")).GetTypes()
            .Where(t => typeof(Controller).IsAssignableFrom(t) && t.Namespace == nspace)
            .ToList()
            .ForEach(t =>
            {
                var authAttr  = t.GetTypeInfo().GetCustomAttribute<AuthorizeAttribute>();
                Assert.NotNull(authAttr);
            });
        }
    }
}
