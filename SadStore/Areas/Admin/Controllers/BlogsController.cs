using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogsController : Controller
    {
        private readonly StoreContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public BlogsController(StoreContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.BlogPosts.ToListAsync());
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

                    // Preserve original date or allow edit based on requirement
                    var currentBlog = await _context.BlogPosts.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                    if (currentBlog != null) blogPost.PublishedDate = currentBlog.PublishedDate;

                    _context.Update(blogPost);
                    await _context.SaveChangesAsync();
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
            }
            return RedirectToAction(nameof(Index));
        }
    }
}