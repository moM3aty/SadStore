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

        // عرض المنتجات (بحث + فلترة + ترتيب + تقسيم صفحات)
        public async Task<IActionResult> Index(string search, int? categoryId, string sortOrder, int page = 1)
        {
            int pageSize = 10; // عدد المنتجات في الصفحة الواحدة

            var query = _context.Products.Include(p => p.Category).AsQueryable();

            // 1. الفلترة والبحث
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(p => p.NameAr.Contains(search) ||
                                         p.NameEn.Contains(search) ||
                                         p.ModelNumber.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // 2. الترتيب
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";
            ViewBag.StockSortParm = sortOrder == "Stock" ? "stock_desc" : "Stock";

            switch (sortOrder)
            {
                case "name_desc": query = query.OrderByDescending(p => p.NameAr); break;
                case "Price": query = query.OrderBy(p => p.Price); break;
                case "price_desc": query = query.OrderByDescending(p => p.Price); break;
                case "Stock": query = query.OrderBy(p => p.StockQuantity); break;
                case "stock_desc": query = query.OrderByDescending(p => p.StockQuantity); break;
                default: query = query.OrderByDescending(p => p.Id); break; // الافتراضي: الأحدث
            }

            // 3. تقسيم الصفحات
            int totalItems = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // تمرير البيانات للفيو
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.Search = search;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", _lang.IsRtl() ? "NameAr" : "NameEn", categoryId);
            ViewBag.CurrentCategoryId = categoryId;

            return View(products);
        }

        // تصدير إلى Excel
        public async Task<IActionResult> Export()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            var isRtl = _lang.IsRtl();
            var builder = new StringBuilder();

            // إضافة BOM لدعم العربية
            builder.Append('\uFEFF');

            // العناوين
            if (isRtl)
                builder.AppendLine("رقم المنتج,الاسم,القسم,السعر,المخزون,تاريخ الإضافة");
            else
                builder.AppendLine("ID,Name,Category,Price,Stock,Date Added");

            foreach (var p in products)
            {
                var name = (isRtl ? p.NameAr : p.NameEn)?.Replace(",", " ");
                var cat = (p.Category != null ? (isRtl ? p.Category.NameAr : p.Category.NameEn) : "-")?.Replace(",", " ");

                builder.AppendLine($"{p.Id},{name},{cat},{p.Price},{p.StockQuantity},{p.CreatedAt:yyyy-MM-dd}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"products_{DateTime.Now:yyyyMMdd}.csv");
        }

        // إنشاء منتج جديد
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", _lang.IsRtl() ? "NameAr" : "NameEn");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                // معالجة الصور
                await HandleImageUpload(product, imageFiles);

                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Saved successfully";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", _lang.IsRtl() ? "NameAr" : "NameEn", product.CategoryId);
            return View(product);
        }

        // تعديل منتج
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", _lang.IsRtl() ? "NameAr" : "NameEn", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? mainImageFile, List<IFormFile> additionalImages)
        {
            if (id != product.Id) return NotFound();
            ModelState.Remove("Category");
            ModelState.Remove("Images");
            if (ModelState.IsValid)
            {
                try
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string uploadDir = Path.Combine(wwwRootPath, "images/products");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    // 1. تحديث الصورة الرئيسية (فقط إذا تم رفع ملف جديد)
                    if (mainImageFile != null)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImageFile.FileName);
                        string path = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await mainImageFile.CopyToAsync(stream);
                        }
                        product.ImageUrl = "/images/products/" + fileName;
                    }
                    else
                    {
                        // استرجاع الرابط القديم إذا لم يتم رفعه
                        var existing = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                        product.ImageUrl = existing?.ImageUrl;
                    }

                    // 2. إضافة صور جديدة للمعرض
                    if (additionalImages != null && additionalImages.Count > 0)
                    {
                        foreach (var file in additionalImages)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string path = Path.Combine(uploadDir, fileName);
                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            // إضافة سجل جديد في جدول ProductImages
                            _context.ProductImages.Add(new ProductImage { Url = "/images/products/" + fileName, ProductId = id });
                        }
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", _lang.IsRtl() ? "NameAr" : "NameEn", product.CategoryId);
            return View(product);
        }

        // دالة مساعدة لرفع الصور
        private async Task HandleImageUpload(Product product, List<IFormFile> files, bool isEdit = false)
        {
            if (files != null && files.Count > 0)
            {
                string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                for (int i = 0; i < files.Count; i++)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(files[i].FileName);
                    string path = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await files[i].CopyToAsync(stream);
                    }

                    // إذا كان جديد، أول صورة هي الرئيسية. إذا تعديل والرئيسية فارغة، نحدثها.
                    if ((!isEdit && i == 0) || (isEdit && string.IsNullOrEmpty(product.ImageUrl)))
                    {
                        product.ImageUrl = "/images/products/" + fileName;
                    }

                    // إضافة لجدول الصور (فقط إذا لم يكن كائن المنتج جديداً بالكامل في الذاكرة، هنا نضيف للكائن نفسه)
                    if (isEdit)
                    {
                        _context.ProductImages.Add(new ProductImage { Url = "/images/products/" + fileName, ProductId = product.Id });
                    }
                    else
                    {
                        product.Images.Add(new ProductImage { Url = "/images/products/" + fileName });
                    }
                }
            }
            else if (!isEdit && string.IsNullOrEmpty(product.ImageUrl))
            {
                product.ImageUrl = "/images/product.webp"; // Default
            }
        }

        // حذف صورة فردية
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var img = await _context.ProductImages.FindAsync(id);
            if (img != null)
            {
                int pid = img.ProductId;
                _context.ProductImages.Remove(img);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Image Deleted";
                return RedirectToAction(nameof(Edit), new { id = pid });
            }
            return RedirectToAction(nameof(Index));
        }

        // حذف منتج
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