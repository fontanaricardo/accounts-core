namespace Accounts.Extensions
{
    using System;
    using System.Security.Claims;
    using System.Security.Principal;
    using Models;

    public static class IdentityExtensions
    {
        public static string GetFullName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("FullUserName");

            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static EletronicSignatureStatus GetEletronicSignatureStatus(this IIdentity identity)
        {
            EletronicSignatureStatus es = EletronicSignatureStatus.Unsolicited;

            var claim = ((ClaimsIdentity)identity).FindFirst("EletronicSignatureStatus");

            if (claim != null)
            {
                // TODO: Fazer tratativa
                Enum.TryParse(claim.Value, out es);
            }

            return es;
        }

        public static void AddUpdateClaim(this IPrincipal currentPrincipal, string key, string value)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return;
            }

            // check for existing claim and remove it
            var existingClaim = identity.FindFirst(key);
            if (existingClaim != null)
            {
                identity.RemoveClaim(existingClaim);
            }

            // add new claim
            identity.AddClaim(new Claim(key, value));

            // var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            // authenticationManager.AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(identity), new AuthenticationProperties() { IsPersistent = true });
        }
    }
}