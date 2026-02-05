using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using SadStore.Services;
using System.Text;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogsController : Controller
    {
        private readonly StoreContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly LanguageService _lang;

        public BlogsController(StoreContext context, IWebHostEnvironment hostEnvironment, LanguageService lang)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _lang = lang;
        }

        public async Task<IActionResult> Index(string search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.BlogPosts.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(b => b.TitleAr.Contains(search) || b.TitleEn.Contains(search));
            }

            query = query.OrderByDescending(b => b.PublishedDate);

            int totalItems = await query.CountAsync();
            var posts = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.Search = search;
            ViewBag.TotalCount = totalItems;

            return View(posts);
        }

        public async Task<IActionResult> Export()
        {
            var posts = await _context.BlogPosts.OrderByDescending(b => b.PublishedDate).ToListAsync();
            var isRtl = _lang.IsRtl();
            var builder = new StringBuilder();

            builder.Append('\uFEFF');

            if (isRtl)
                builder.AppendLine("المعرف,العنوان (عربي),العنوان (إنجليزي),تاريخ النشر");
            else
                builder.AppendLine("ID,Title (Ar),Title (En),Published Date");

            foreach (var item in posts)
            {
                var titleAr = item.TitleAr?.Replace(",", " ") ?? "";
                var titleEn = item.TitleEn?.Replace(",", " ") ?? "";
                builder.AppendLine($"{item.Id},{titleAr},{titleEn},{item.PublishedDate:yyyy-MM-dd}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"blogs_{DateTime.Now:yyyyMMdd}.csv");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogPost blogPost, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string path = Path.Combine(wwwRootPath + "/images/blogs/", fileName);

                    if (!Directory.Exists(Path.Combine(wwwRootPath, "images/blogs")))
                        Directory.CreateDirectory(Path.Combine(wwwRootPath, "images/blogs"));

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    blogPost.ImageUrl = "/images/blogs/" + fileName;
                }
                else
                {
                    blogPost.ImageUrl = "/images/blog-default.jpg";
                }

                blogPost.PublishedDate = DateTime.Now;
                _context.Add(blogPost);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Saved successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(blogPost);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null) return NotFound();
            return View(blogPost);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogPost blogPost, IFormFile? imageFile)
        {
            if (id != blogPost.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string path = Path.Combine(wwwRootPath + "/images/blogs/", fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                        blogPost.ImageUrl = "/images/blogs/" + fileName;
                    }
                    else
                    {
                        var existingBlog = await _context.BlogPosts.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                        blogPost.ImageUrl = existingBlog?.ImageUrl;
                    }

                    var current = await _context.BlogPosts.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                    if (current != null) blogPost.PublishedDate = current.PublishedDate;

                    _context.Update(blogPost);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Saved successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.BlogPosts.Any(e => e.Id == blogPost.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(blogPost);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost != null)
            {
                _context.BlogPosts.Remove(blogPost);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Deleted successfully";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}