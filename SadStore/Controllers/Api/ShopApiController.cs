using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SadStore.Controllers.Api
{
    [Route("api/shop")]
    [ApiController]
    public class ShopApiController : ControllerBase
    {
        private readonly StoreContext _context;

        public ShopApiController(StoreContext context)
        {
            _context = context;
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new { c.Id, c.NameAr, c.NameEn, c.ImageUrl })
                .ToListAsync();
            return Ok(categories);
        }

        [HttpGet("shipping-locations")]
        public async Task<IActionResult> GetShippingLocations()
        {
            var locations = await _context.ShippingLocations.ToListAsync();
            return Ok(locations);
        }

        [HttpGet("blogs")]
        public async Task<IActionResult> GetBlogs()
        {
            var blogs = await _context.BlogPosts
                .OrderByDescending(b => b.PublishedDate)
                .Select(b => new { b.Id, b.TitleAr, b.TitleEn, b.ImageUrl, b.PublishedDate })
                .ToListAsync();
            return Ok(blogs);
        }

        [Authorize(AuthenticationSchemes = "Bearer")] // يطلب تسجيل دخول
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutApiDto model)
        {
            if (model.Items == null || !model.Items.Any())
                return BadRequest(new { message = "السلة فارغة" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var shippingLocation = await _context.ShippingLocations.FindAsync(model.ShippingLocationId);
            if (shippingLocation == null) return BadRequest(new { message = "منطقة الشحن غير صالحة" });

            decimal shippingCost = shippingLocation.ShippingCost;
            decimal productsTotal = 0;

            var order = new Order
            {
                CustomerName = model.FullName ?? userName,
                PhoneNumber = model.Phone,
                Address = $"{shippingLocation.CityNameAr} - {model.Address}",
                OrderDate = DateTime.Now,
                Status = "جديد",
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in model.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    decimal itemTotal = product.Price * item.Quantity;
                    productsTotal += itemTotal;

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        Price = product.Price
                    });
                }
            }

            decimal tax = productsTotal * 0.15m;
            order.TotalAmount = productsTotal + tax + shippingCost;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم إنشاء الطلب بنجاح",
                orderId = order.Id,
                totalAmount = order.TotalAmount
            });
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var phone = User.FindFirstValue(ClaimTypes.MobilePhone); // قد يكون فارغاً إذا لم يسجل به

            var orders = await _context.Orders
                .Where(o => o.CustomerName == userName || (phone != null && o.PhoneNumber == phone))
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new { o.Id, o.OrderDate, o.TotalAmount, o.Status })
                .ToListAsync();

            return Ok(orders);
        }
    }

    public class CheckoutApiDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int ShippingLocationId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? Note { get; set; }
        public List<CartItemApiDto> Items { get; set; } = new List<CartItemApiDto>();
    }

    public class CartItemApiDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}