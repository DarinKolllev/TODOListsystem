using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace TODOlistsystem.Controllers
{
    public class LanguageController : Controller
    {
        public IActionResult SetLanguage(string lang, string returnUrl)
        {
            if (!string.IsNullOrEmpty(lang))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(
                        new RequestCulture(lang)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1)
                    });
            }

            return LocalRedirect(returnUrl);
        }
    }
}