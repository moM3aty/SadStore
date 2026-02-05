using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using System.Security.Claims;

namespace SadStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly StoreContext _context;

        public ProductsController(StoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string category, string search, string sort, decimal? minPrice, decimal? maxPrice, int? rating)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            // 1. فلترة القسم
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.NameEn == category || p.Category.NameAr == category);
            }

            // 2. البحث
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.NameAr.Contains(search) || p.NameEn.Contains(search) || p.DescriptionAr.Contains(search) || p.DescriptionEn.Contains(search));
            }

            // 3. فلترة السعر (الإصلاح المطلوب)
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // 4. الترتيب
            switch (sort)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            var products = await query.ToListAsync();

            // إعادة تمرير القيم للـ View للحفاظ على الحالة
            ViewBag.CurrentCategory = category;
            ViewBag.Search = search;

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images) // تضمين الصور الإضافية
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(4)
                .ToListAsync();

            var reviews = await _context.CustomerReviews
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.Id)
                .Take(5)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.Reviews = reviews;

            return View(product);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, string reviewText, int rating)
        {
            // ... (نفس كود التقييم السابق)
            if (string.IsNullOrEmpty(reviewText) || rating < 1 || rating > 5)
            {
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var review = new CustomerReview
            {
                CustomerName = User.Identity.Name ?? "عميل",
                ReviewText = reviewText,
                Rating = rating,
                IsApproved = false
            };

            _context.CustomerReviews.Add(review);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Review submitted";
            return RedirectToAction(nameof(Details), new { id = productId });
        }
    }
}