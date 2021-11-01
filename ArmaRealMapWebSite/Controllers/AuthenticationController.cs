using System;
using System.Linq;
using System.Threading.Tasks;
using ArmaRealMapWebSite.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ArmaRealMapWebSite.Controllers
{
    public class AuthenticationController : Controller
    {

        [HttpGet]
        public async Task<IActionResult> SignIn(string ReturnUrl)
        {
            if (ReturnUrl != null && !ReturnUrl.StartsWith("/"))
            {
                return BadRequest();
            }
            var vm = new SignInViewModel();
            vm.ReturnUrl = ReturnUrl ?? "/Home/Index";
            vm.Providers = await GetExternalProvidersAsync(HttpContext);
            return View("SignIn", vm);
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromForm] string provider, [FromForm] bool isPersistent, [FromForm] string ReturnUrl)
        {
            // Note: the "provider" parameter corresponds to the external
            // authentication provider choosen by the user agent.
            if (string.IsNullOrWhiteSpace(provider))
            {
                return BadRequest();
            }

            if (!await IsProviderSupportedAsync(HttpContext, provider))
            {
                return BadRequest();
            }

            if (!ReturnUrl.StartsWith("/"))
            {
                return BadRequest();
            }

            // Instruct the middleware corresponding to the requested external identity
            // provider to redirect the user agent to its own authorization endpoint.
            // Note: the authenticationScheme parameter must match the value configured in Startup.cs
            return Challenge(new AuthenticationProperties { RedirectUri = ReturnUrl, IsPersistent = isPersistent }, provider);
        }

        [HttpGet, HttpPost]
        public new IActionResult SignOut()
        {
            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            return SignOut(new AuthenticationProperties { RedirectUri = "/Home/Index" },
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
        public static async Task<AuthenticationScheme[]> GetExternalProvidersAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

            return (from scheme in await schemes.GetAllSchemesAsync()
                    where !string.IsNullOrEmpty(scheme.DisplayName)
                    select scheme).ToArray();
        }

        public static async Task<bool> IsProviderSupportedAsync(HttpContext context, string provider)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return (from scheme in await GetExternalProvidersAsync(context)
                    where string.Equals(scheme.Name, provider, StringComparison.OrdinalIgnoreCase)
                    select scheme).Any();
        }

        public IActionResult Denied() => View("Denied");

    }
}
