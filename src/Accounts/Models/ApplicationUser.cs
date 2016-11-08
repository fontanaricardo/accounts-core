using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Accounts.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string FullUserName { get; set; }

        public EletronicSignatureStatus EletronicSignatureStatus { get; set; }
    }
}
