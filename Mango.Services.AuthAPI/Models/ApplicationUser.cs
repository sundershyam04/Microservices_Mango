using Microsoft.AspNetCore.Identity;

namespace Mango.Fro.AuthAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
