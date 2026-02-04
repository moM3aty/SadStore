using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using System.Text.Json;

namespace SadStore.Controllers
{
    public class CartController : Controller
    {
        private readonly StoreContext _context;

        public CartController(StoreContext context)
        {
            _context = context;
        }

        // عرض السلة
        public async Task<IActionResult> Index()
        {
            var cart = GetCart();
            var productIds = cart.Keys.ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var cartViewModel = new CartViewModel
            {
                Items = products.Select(p => new CartItemViewModel
                {
                    Product = p,
                    Quantity = cart[p.Id],
                    Total = p.Price * cart[p.Id]
                }).ToList()
            };

            return View(cartViewModel);
        }

        [HttpPost]
        public IActionResult Add(int productId, int quantity = 1)
        {
            var cart = GetCart();
            if (cart.ContainsKey(productId))
            {
                cart[productId] += quantity;
            }
            else
            {
                cart[productId] = quantity;
            }
            SaveCart(cart);
            TempData["SuccessMessage"] = "Item added to cart";
            // العودة لنفس الصفحة
            string referer = Request.Headers["Referer"].ToString();
            return Redirect(string.IsNullOrEmpty(referer) ? "/" : referer);
        }

        [HttpPost]
        public IActionResult Update(int productId, int quantity)
        {
            var cart = GetCart();
            if (cart.ContainsKey(productId))
            {
                if (quantity > 0)
                    cart[productId] = quantity;
                else
                    cart.Remove(productId);
            }
            SaveCart(cart);
            TempData["SuccessMessage"] = "Cart updated";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();
            if (cart.ContainsKey(productId))
            {
                cart.Remove(productId);
            }
            SaveCart(cart);
            TempData["ErrorMessage"] = "Item removed from cart";
            return RedirectToAction("Index");
        }

        private Dictionary<int, int> GetCart()
        {
            var sessionCart = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(sessionCart))
            {
                return new Dictionary<int, int>();
            }
            return JsonSerializer.Deserialize<Dictionary<int, int>>(sessionCart);
        }

        private void SaveCart(Dictionary<int, int> cart)
        {
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
        }
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal SubTotal => Items.Sum(i => i.Total);
        public decimal Tax => SubTotal * 0.15m;
        public decimal GrandTotal => SubTotal + Tax;
    }

    public class CartItemViewModel
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }
}