using Microsoft.AspNetCore.Identity;

namespace MVC.Intro.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}


