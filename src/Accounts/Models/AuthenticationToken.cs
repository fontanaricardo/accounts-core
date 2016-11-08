using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;

namespace Accounts.Models
{
    public class AuthenticationToken
    {
        public AuthenticationToken() { }

        public AuthenticationToken(IPrincipal user, Uri uri)
        {
            if (user.Identity != null) UserName = user.Identity.Name;
            if (uri.Host != null) Domain = uri.Host;
            CreatedAt = DateTime.Now;
            Expiration = DateTime.Now.AddDays(7);
            GenerateToken();
        }
        
        [Key]
        public int TokenID { get; set; }

        [Required]
        public string Token { get; private set; }

        [Required]
        public string Domain { get; private set; }

        [Required]
        public DateTime CreatedAt { get; private set; }

        [Required]
        public DateTime Expiration { get; private set; }

        [Required]
        public string UserName { get; private set; }

        public DateTime? UsedAt { get; private set; }

        public void GenerateToken()
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            byte[] userNameBytes = new byte[UserName.Length * sizeof(char)];
            System.Buffer.BlockCopy(UserName.ToCharArray(), 0, userNameBytes, 0, userNameBytes.Length);
            Token = Convert.ToBase64String(time.Concat(key).Concat(userNameBytes).ToArray());
        }

        public void UseToken()
        {
            UsedAt = DateTime.Now;
        }
    }
}