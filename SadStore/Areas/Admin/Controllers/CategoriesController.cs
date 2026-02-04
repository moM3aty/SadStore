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
        private readonly LanguageService _lang;

        public CategoriesController(StoreContext context, LanguageService lang)
        {
            _context = context;
            _lang = lang;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.Include(c => c.Products).ToListAsync());
        }

        // تصدير الأقسام إلى Excel (CSV)
        public async Task<IActionResult> Export()
        {
            var categories = await _context.Categories.Include(c => c.Products).ToListAsync();
            var isRtl = _lang.IsRtl();
            var builder = new StringBuilder();

            var preamble = Encoding.UTF8.GetPreamble();

            if (isRtl)
            {
                builder.AppendLine("المعرف,الاسم (عربي),الاسم (إنجليزي),عدد المنتجات");
            }
            else
            {
                builder.AppendLine("ID,Name (Ar),Name (En),Product Count");
            }

            foreach (var c in categories)
            {
                var nameAr = c.NameAr?.Replace(",", " ") ?? "";
                var nameEn = c.NameEn?.Replace(",", " ") ?? "";
                var count = c.Products?.Count ?? 0;

                builder.AppendLine($"{c.Id},{nameAr},{nameEn},{count}");
            }

            var contentBytes = Encoding.UTF8.GetBytes(builder.ToString());
            var resultBytes = new byte[preamble.Length + contentBytes.Length];
            Array.Copy(preamble, 0, resultBytes, 0, preamble.Length);
            Array.Copy(contentBytes, 0, resultBytes, preamble.Length, contentBytes.Length);

            string fileName = isRtl ? "categories_report_ar.csv" : "categories_report_en.csv";
            return File(resultBytes, "text/csv", fileName);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                category.ImageUrl = "/images/cat-default.jpg";
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
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    category.ImageUrl = existing?.ImageUrl ?? "/images/cat-default.jpg";

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