using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using SadStore.Services;
using System.Text;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly StoreContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly LanguageService _lang;

        public CategoriesController(StoreContext context, IWebHostEnvironment hostEnvironment, LanguageService lang)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _lang = lang;
        }

        // عرض الأقسام (بحث + ترتيب + تقسيم)
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Categories.Include(c => c.Products).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(c => c.NameAr.Contains(search) || c.NameEn.Contains(search));
            }

            // ترتيب حسب عدد المنتجات (الأكثر امتلاءً أولاً) ثم الاسم
            query = query.OrderByDescending(c => c.Products.Count).ThenBy(c => c.NameAr);

            int totalItems = await query.CountAsync();
            var categories = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.Search = search;

            return View(categories);
        }

        // تصدير إلى Excel
        public async Task<IActionResult> Export()
        {
            var categories = await _context.Categories.Include(c => c.Products).ToListAsync();
            var isRtl = _lang.IsRtl();
            var builder = new StringBuilder();

            builder.Append('\uFEFF'); // BOM

            if (isRtl)
                builder.AppendLine("المعرف,الاسم (عربي),الاسم (إنجليزي),عدد المنتجات");
            else
                builder.AppendLine("ID,Name (Ar),Name (En),Product Count");

            foreach (var c in categories)
            {
                var nameAr = c.NameAr?.Replace(",", " ");
                var nameEn = c.NameEn?.Replace(",", " ");
                var count = c.Products?.Count ?? 0;
                builder.AppendLine($"{c.Id},{nameAr},{nameEn},{count}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"categories_{DateTime.Now:yyyyMMdd}.csv");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // رفع الصورة
                if (imageFile != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string uploadDir = Path.Combine(wwwRootPath, "images", "categories");

                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    string path = Path.Combine(uploadDir, fileName);
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    category.ImageUrl = "/images/categories/" + fileName;
                }
                else
                {
                    category.ImageUrl = "/images/cat-default.jpg"; // صورة افتراضية
                }

                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Saved successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category, IFormFile? imageFile)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string uploadDir = Path.Combine(wwwRootPath, "images", "categories");

                        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                        string path = Path.Combine(uploadDir, fileName);
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                        category.ImageUrl = "/images/categories/" + fileName;
                    }
                    else
                    {
                        // الحفاظ على الصورة القديمة
                        var existing = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                        category.ImageUrl = existing?.ImageUrl ?? "/images/cat-default.jpg";
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Saved successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id == category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Deleted successfully";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}