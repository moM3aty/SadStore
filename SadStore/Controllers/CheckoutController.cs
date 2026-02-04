using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using SadStore.Services;
using System.Text;
using System.Text.Json;
using System.Web;

namespace SadStore.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly StoreContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LanguageService _lang;

        public CheckoutController(StoreContext context, UserManager<IdentityUser> userManager, LanguageService lang)
        {
            _context = context;
            _userManager = userManager;
            _lang = lang;
        }

        [HttpPost]
        public async Task<IActionResult> Process()
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index", "Cart");

            var productIds = cart.Keys.ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var order = new Order
            {
                CustomerName = User.Identity.IsAuthenticated ? User.Identity.Name : (_lang.IsRtl() ? "زائر" : "Guest"),
                OrderDate = DateTime.Now,
                Status = "جديد", // سيتم ترجمتها عند العرض
                OrderItems = new List<OrderItem>()
            };

            decimal totalAmount = 0;

            // بناء رسالة الواتساب
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(_lang.Get("Hello, I would like to complete the following order:")); // تأكد من إضافة هذا المفتاح للقاموس أو استخدامه مباشرة
            messageBuilder.AppendLine("------------------------");

            foreach (var product in products)
            {
                var quantity = cart[product.Id];
                var price = product.Price;
                var total = price * quantity;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    Price = price
                });

                totalAmount += total;
                var productName = _lang.IsRtl() ? product.NameAr : product.NameEn;
                messageBuilder.AppendLine($"- {productName} ({_lang.Get("Quantity")}: {quantity}) - {total} {_lang.Get("SAR")}");
            }

            decimal tax = totalAmount * 0.15m;
            decimal grandTotal = totalAmount + tax;

            order.TotalAmount = grandTotal;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // إفراغ السلة
            HttpContext.Session.Remove("Cart");

            messageBuilder.AppendLine("------------------------");
            messageBuilder.AppendLine($"{_lang.Get("Subtotal")}: {totalAmount:0.00} {_lang.Get("SAR")}");
            messageBuilder.AppendLine($"{_lang.Get("VAT (15%)")}: {tax:0.00} {_lang.Get("SAR")}");
            messageBuilder.AppendLine($"*{_lang.Get("Total")}: {grandTotal:0.00} {_lang.Get("SAR")}*");
            messageBuilder.AppendLine($"{_lang.Get("Order Number")}: #{order.Id}");
            messageBuilder.AppendLine("------------------------");
            messageBuilder.AppendLine(_lang.Get("Please provide payment method and confirm order.")); // تأكد من إضافتها للقاموس

            // توجيه للواتساب
            string adminPhone = "966565532971";
            string urlEncodedMessage = HttpUtility.UrlEncode(messageBuilder.ToString());
            string whatsappUrl = $"https://wa.me/{adminPhone}?text={urlEncodedMessage}";

            return Redirect(whatsappUrl);
        }

        private Dictionary<int, int> GetCart()
        {
            var sessionCart = HttpContext.Session.GetString("Cart");
            return string.IsNullOrEmpty(sessionCart)
                ? new Dictionary<int, int>()
                : JsonSerializer.Deserialize<Dictionary<int, int>>(sessionCart);
        }
    }
}