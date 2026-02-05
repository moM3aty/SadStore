using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace SadStore.Controllers
{
    public class WishlistController : Controller
    {
        private const string CookieName = "SadStore_Wishlist";

        [HttpPost]
        public IActionResult Toggle(int productId)
        {
            List<int> wishlist;

            // قراءة الكوكيز الحالية
            var cookie = Request.Cookies[CookieName];
            if (string.IsNullOrEmpty(cookie))
            {
                wishlist = new List<int>();
            }
            else
            {
                try
                {
                    wishlist = JsonSerializer.Deserialize<List<int>>(cookie) ?? new List<int>();
                }
                catch
                {
                    wishlist = new List<int>();
                }
            }

            // إضافة أو حذف المنتج
            if (wishlist.Contains(productId))
            {
                wishlist.Remove(productId);
            }
            else
            {
                wishlist.Add(productId);
            }

            // حفظ الكوكيز الجديدة (لمدة 30 يوم)
            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30),
                HttpOnly = true,
                IsEssential = true
            };

            Response.Cookies.Append(CookieName, JsonSerializer.Serialize(wishlist), options);

            return Ok(); // إرجاع نجاح لـ AJAX
        }

        // عرض صفحة المفضلة (اختياري، يمكن دمجها في البروفايل)
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Profile", new { fragment = "ads" });
        }
    }
}