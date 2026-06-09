using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SadStore.Controllers.Api
{
    // تحديد مسار الـ API ليكون /api/products
    [Route("api/products")]
    [ApiController]
    public class ProductsApiController : ControllerBase
    {
        private readonly StoreContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductsApiController(StoreContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // 1. جلب جميع المنتجات (GET: /api/products)
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            // استخدام Select لتجنب مشكلة Circular Reference الخاصة بـ Entity Framework
            var products = await _context.Products
                .Include(p => p.Category)
                .Select(p => new
                {
                    p.Id,
                    p.NameAr,
                    p.NameEn,
                    p.Price,
                    p.StockQuantity,
                    p.ImageUrl,
                    CategoryNameAr = p.Category != null ? p.Category.NameAr : null,
                    CategoryNameEn = p.Category != null ? p.Category.NameEn : null
                })
                .ToListAsync();

            return Ok(products);
        }

        // 2. جلب منتج واحد بالـ ID (GET: /api/products/{id})
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.NameAr,
                    p.NameEn,
                    p.DescriptionAr,
                    p.DescriptionEn,
                    p.Price,
                    p.StockQuantity,
                    p.ImageUrl,
                    CategoryId = p.CategoryId,
                    CategoryNameAr = p.Category != null ? p.Category.NameAr : null
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(product);
        }

        // 3. إضافة منتج جديد مع رفع الصورة (POST: /api/products)
        // نستخدم [FromForm] لاستقبال البيانات كـ form-data
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductFormDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = new Product
            {
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                IsFeatured = dto.IsFeatured,
                ModelNumber = dto.ModelNumber,
                CreatedAt = DateTime.Now
            };

            // معالجة وحفظ الصورة إن وجدت
            if (dto.MainImageFile != null && dto.MainImageFile.Length > 0)
            {
                string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.MainImageFile.FileName);
                string filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.MainImageFile.CopyToAsync(stream);
                }

                product.ImageUrl = "/images/products/" + fileName;
            }
            else
            {
                // صورة افتراضية في حال لم يتم رفع صورة
                product.ImageUrl = "/images/product.webp";
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new
            {
                message = "Product created successfully",
                product.Id,
                product.NameAr,
                product.NameEn,
                product.ImageUrl
            });
        }
    }

    // DTO: Data Transfer Object لتمثيل البيانات القادمة من الـ Form-Data
    public class ProductFormDto
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public bool IsFeatured { get; set; }
        public string? ModelNumber { get; set; }

        // خاصية استقبال ملف الصورة
        public IFormFile? MainImageFile { get; set; }
    }
}