using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using System.Text.Json;

namespace SadStore.Controllers
{
    public class WishlistController : Controller
    {
        private readonly StoreContext _context;
        private const string CookieName = "SadStore_Wishlist";

        public WishlistController(StoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var wishlistIds = GetWishlistIds();
            var products = await _context.Products
                .Where(p => wishlistIds.Contains(p.Id))
                .ToListAsync();

            return View(products);
        }

        [HttpPost]
        public IActionResult Toggle(int productId)
        {
            var wishlistIds = GetWishlistIds();

            if (wishlistIds.Contains(productId))
            {
                wishlistIds.Remove(productId);
            }
            else
            {
                wishlistIds.Add(productId);
            }

            SaveWishlistIds(wishlistIds);

            // العودة إلى نفس الصفحة التي تم الضغط منها
            string referer = Request.Headers["Referer"].ToString();
            return Redirect(string.IsNullOrEmpty(referer) ? "/" : referer);
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var wishlistIds = GetWishlistIds();
            if (wishlistIds.Contains(productId))
            {
                wishlistIds.Remove(productId);
                SaveWishlistIds(wishlistIds);
            }
            return RedirectToAction(nameof(Index));
        }

        private List<int> GetWishlistIds()
        {
            var cookie = Request.Cookies[CookieName];
            if (string.IsNullOrEmpty(cookie))
            {
                return new List<int>();
            }
            return JsonSerializer.Deserialize<List<int>>(cookie);
        }

        private void SaveWishlistIds(List<int> ids)
        {
            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30),
                HttpOnly = true,
                IsEssential = true
            };
            Response.Cookies.Append(CookieName, JsonSerializer.Serialize(ids), options);
        }
    }
}