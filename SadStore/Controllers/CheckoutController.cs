using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using System.Text;
using System.Text.Json;
using System.Web;

namespace SadStore.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly StoreContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CheckoutController(StoreContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                CustomerName = User.Identity.IsAuthenticated ? User.Identity.Name : "زائر",
                OrderDate = DateTime.Now,
                Status = "جديد",
                OrderItems = new List<OrderItem>()
            };

            decimal totalAmount = 0;
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("مرحباً، أرغب بإتمام الطلب التالي من موقعكم:");
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
                messageBuilder.AppendLine($"- {product.NameAr} (العدد: {quantity}) - {total} ر.س");
            }

            decimal tax = totalAmount * 0.15m;
            decimal grandTotal = totalAmount + tax;

            order.TotalAmount = grandTotal;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Clear Cart
            HttpContext.Session.Remove("Cart");

            messageBuilder.AppendLine("------------------------");
            messageBuilder.AppendLine($"المجموع: {totalAmount:0.00} ر.س");
            messageBuilder.AppendLine($"الضريبة (15%): {tax:0.00} ر.س");
            messageBuilder.AppendLine($"*الإجمالي النهائي: {grandTotal:0.00} ر.س*");
            messageBuilder.AppendLine($"رقم الطلب المرجعي: #{order.Id}");
            messageBuilder.AppendLine("------------------------");
            messageBuilder.AppendLine("الرجاء تزويدي بطريقة الدفع وتأكيد الطلب.");

            // WhatsApp Redirect
            string adminPhone = "966565532971"; // رقم الأدمن كما في الفوتر
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