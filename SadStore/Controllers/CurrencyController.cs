using Microsoft.AspNetCore.Mvc;

namespace SadStore.Controllers
{
    public class CurrencyController : Controller
    {
        [HttpPost]
        public IActionResult SetCurrency(string currency, string returnUrl)
        {
            Response.Cookies.Append("SadStore_Currency", currency, new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                HttpOnly = true,
                IsEssential = true
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}