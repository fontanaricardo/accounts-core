namespace Accounts.Extensions
{
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;
    using Microsoft.AspNetCore.Identity;
    using Models;

    public static class IdentityExtensions
    {
        public static string GetFullName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("FullUserName");

            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        /// <summary>
        /// Delete all exiting claims with same type param and create a new one with param value.
        /// </summary>
        /// <remarks>
        /// After call this method use RefreshSignInAsync in SignInManager to refresh the cookies.!--
        ///
        /// _userManager.AddOrUpdateClaim(user, "FullUserName", user.FullUserName);
        /// await _signInManager.RefreshSignInAsync(user);
        ///
        /// </remarks>
        /// <param name="userManager">Current user manager.</param>
        /// <param name="user">Logged user.</param>
        /// <param name="type">Claim type as string value.</param>
        /// <param name="value">New claim values as string.</param>
        public static void AddOrUpdateClaim(this UserManager<ApplicationUser> userManager, ApplicationUser user, string type, string value)
        {
            var oldClaims = userManager.GetClaimsAsync(user).Result.Where(c => c.Type == type).ToList();

            if (oldClaims.Count > 0)
            {
                var removeResult = userManager.RemoveClaimsAsync(user, oldClaims).Result;
            }

            var addResult = userManager.AddClaimAsync(user, new Claim(type, value)).Result;
        }
    }
}