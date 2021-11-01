using Microsoft.AspNetCore.Authentication;

namespace ArmaRealMapWebSite.Models
{
    public class SignInViewModel
    {
        public string ReturnUrl { get; set; }
        public AuthenticationScheme[] Providers { get; set; }
    }
}
