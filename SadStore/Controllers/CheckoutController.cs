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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index", "Cart");

            var productIds = cart.Keys.ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var locations = await _context.ShippingLocations.ToListAsync();

            var viewModel = new CheckoutViewModel
            {
                CartItems = products.Select(p => new CartItemViewModel
                {
                    Product = p,
                    Quantity = cart[p.Id],
                    Total = p.Price * cart[p.Id]
                }).ToList(),
                ShippingLocations = locations
            };

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    viewModel.FullName = user.UserName;
                    viewModel.Email = user.Email;
                    viewModel.Phone = user.PhoneNumber;
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(CheckoutViewModel model)
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index", "Cart");

            var productIds = cart.Keys.ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

            var shippingLocation = await _context.ShippingLocations.FindAsync(model.ShippingLocationId);
            decimal shippingCost = shippingLocation?.ShippingCost ?? 0;
            // تصحيح: استخدام الاسم المناسب حسب اللغة
            string cityName = _lang.IsRtl() ? (shippingLocation?.CityNameAr ?? "غير محدد") : (shippingLocation?.CityNameEn ?? "Unknown");

            var order = new Order
            {
                CustomerName = model.FullName,
                PhoneNumber = model.Phone,
                Address = $"{cityName} - {model.Address}",
                OrderDate = DateTime.Now,
                Status = "جديد",
                OrderItems = new List<OrderItem>()
            };

            decimal totalAmount = 0;
            var messageBuilder = new StringBuilder();

            messageBuilder.AppendLine(_lang.Get("Hello, I would like to complete the following order:"));
            messageBuilder.AppendLine("------------------------");
            messageBuilder.AppendLine($"{_lang.Get("Name")}: {model.FullName}");
            messageBuilder.AppendLine($"{_lang.Get("Phone")}: {model.Phone}");
            messageBuilder.AppendLine($"{_lang.Get("Address")}: {cityName} - {model.Address}");
            if (!string.IsNullOrEmpty(model.Note)) messageBuilder.AppendLine($"{_lang.Get("Note")}: {model.Note}");
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
                var pName = _lang.IsRtl() ? product.NameAr : product.NameEn;
                messageBuilder.AppendLine($"- {pName} (x{quantity})");
            }

            decimal tax = totalAmount * 0.15m;
            decimal grandTotal = totalAmount + tax + shippingCost;

            order.TotalAmount = grandTotal;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

            messageBuilder.AppendLine("------------------------");
            messageBuilder.AppendLine($"{_lang.Get("Subtotal")}: {totalAmount:0.00} {_lang.Get("SAR")}");
            messageBuilder.AppendLine($"{_lang.Get("VAT (15%)")}: {tax:0.00} {_lang.Get("SAR")}");
            messageBuilder.AppendLine($"{_lang.Get("Shipping Cost")}: {shippingCost:0.00} {_lang.Get("SAR")}");
            messageBuilder.AppendLine($"*{_lang.Get("Total")}: {grandTotal:0.00} {_lang.Get("SAR")}*");
            messageBuilder.AppendLine($"{_lang.Get("Order Number")}: #{order.Id}");
            messageBuilder.AppendLine("------------------------");
            messageBuilder.AppendLine(_lang.Get("Please provide payment method and confirm order."));

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

    public class CheckoutViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int ShippingLocationId { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        public List<ShippingLocation> ShippingLocations { get; set; } = new List<ShippingLocation>();
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal SubTotal => CartItems.Sum(i => i.Total);
        public decimal Tax => SubTotal * 0.15m;
    }
}