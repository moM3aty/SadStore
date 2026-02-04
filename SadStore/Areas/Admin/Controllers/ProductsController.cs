using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using SadStore.Services;
using System.Text;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly StoreContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly LanguageService _lang;

        public ProductsController(StoreContext context, IWebHostEnvironment hostEnvironment, LanguageService lang)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _lang = lang;
        }

        // ... (Index, Create, Edit, Delete ... Keep as they were in previous steps)

        // Updated Export to Excel (CSV)
        public async Task<IActionResult> Export()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            var isRtl = _lang.IsRtl();
            var builder = new StringBuilder();

            // إضافة BOM لدعم اللغة العربية في الإكسل
            var preamble = Encoding.UTF8.GetPreamble();

            // العناوين بناءً على اللغة
            if (isRtl)
            {
                builder.AppendLine("رقم المنتج,الاسم,القسم,السعر الحالي,السعر القديم,المخزون,المقاسات,التصميم,الخامة,تاريخ الإضافة");
            }
            else
            {
                builder.AppendLine("Product ID,Name,Category,Current Price,Old Price,Stock,Sizes,Design,Material,Created At");
            }

            foreach (var p in products)
            {
                // اختيار البيانات بناءً على اللغة
                var name = isRtl ? p.NameAr : p.NameEn;
                var catName = p.Category != null ? (isRtl ? p.Category.NameAr : p.Category.NameEn) : "-";
                var design = isRtl ? p.DesignTypeAr : p.DesignTypeEn;
                var material = isRtl ? p.MaterialAr : p.MaterialEn;

                // تنظيف البيانات من الفواصل لتجنب كسر ملف CSV
                name = name?.Replace(",", " ") ?? "";
                catName = catName?.Replace(",", " ") ?? "";
                design = design?.Replace(",", " ") ?? "";
                material = material?.Replace(",", " ") ?? "";
                var sizes = p.AvailableSizes?.Replace(",", " | ") ?? "";

                builder.AppendLine($"{p.Id},{name},{catName},{p.Price},{p.OldPrice},{p.StockQuantity},{sizes},{design},{material},{p.CreatedAt.ToString("yyyy-MM-dd")}");
            }

            // دمج BOM مع المحتوى
            var contentBytes = Encoding.UTF8.GetBytes(builder.ToString());
            var resultBytes = new byte[preamble.Length + contentBytes.Length];
            Array.Copy(preamble, 0, resultBytes, 0, preamble.Length);
            Array.Copy(contentBytes, 0, resultBytes, preamble.Length, contentBytes.Length);

            string fileName = isRtl ? "products_report_ar.csv" : "products_report_en.csv";
            return File(resultBytes, "text/csv", fileName);
        }

        // ... (Keep other methods: Index, Create, Edit, DeleteImage, DeleteConfirmed) ...
        // For brevity, assuming other methods are unchanged from the previous batch (32)
        // If you need the FULL controller again, I can provide it, but only Export was requested to be updated.

        // Re-adding Index for context
        public async Task<IActionResult> Index(string search, int? categoryId, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.NameAr.Contains(search) || p.NameEn.Contains(search) || p.ModelNumber.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            int totalItems = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.Search = search;
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "NameAr", categoryId);
            ViewBag.CurrentCategoryId = categoryId;

            return View(products);
        }

        // Re-adding basic Create/Edit structure for compilation safety
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "NameAr");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string uploadDir = Path.Combine(wwwRootPath, "images/products");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                if (imageFiles != null && imageFiles.Count > 0)
                {
                    for (int i = 0; i < imageFiles.Count; i++)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFiles[i].FileName);
                        string path = Path.Combine(uploadDir, fileName);
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await imageFiles[i].CopyToAsync(fileStream);
                        }

                        if (i == 0) product.ImageUrl = "/images/products/" + fileName;
                        product.Images.Add(new ProductImage { Url = "/images/products/" + fileName });
                    }
                }
                else
                {
                    product.ImageUrl = "/images/product.webp";
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Saved successfully";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "NameAr", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "NameAr", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, List<IFormFile> imageFiles)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string uploadDir = Path.Combine(wwwRootPath, "images/products");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    if (imageFiles != null && imageFiles.Count > 0)
                    {
                        foreach (var file in imageFiles)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string path = Path.Combine(uploadDir, fileName);
                            using (var fileStream = new FileStream(path, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            var newImage = new ProductImage { Url = "/images/products/" + fileName, ProductId = id };
                            _context.ProductImages.Add(newImage);

                            if (string.IsNullOrEmpty(product.ImageUrl)) product.ImageUrl = "/images/products/" + fileName;
                        }
                    }

                    if (string.IsNullOrEmpty(product.ImageUrl))
                    {
                        var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                        product.ImageUrl = existingProduct?.ImageUrl;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Saved successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "NameAr", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image != null)
            {
                int productId = image.ProductId;
                _context.ProductImages.Remove(image);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Image Deleted";
                return RedirectToAction(nameof(Edit), new { id = productId });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Deleted successfully";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}