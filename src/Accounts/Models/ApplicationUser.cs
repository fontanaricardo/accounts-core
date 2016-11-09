namespace Accounts.Models
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string FullUserName { get; set; }

        public EletronicSignatureStatus EletronicSignatureStatus { get; set; }
    }
}
