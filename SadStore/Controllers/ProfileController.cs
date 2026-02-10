using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using SadStore.Services;
using System.Text.Json;

namespace SadStore.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly StoreContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LanguageService _lang;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IWebHostEnvironment _hostEnvironment; // لرفع الصور

        public ProfileController(StoreContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, LanguageService lang, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _lang = lang;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Auth");

            // 1. جلب تفاصيل المستخدم (المحفظة، النقاط، الصورة)
            var userDetail = await _context.UserDetails.FirstOrDefaultAsync(u => u.UserId == user.Id);
            if (userDetail == null)
            {
                userDetail = new UserDetail { UserId = user.Id, WalletBalance = 0, LoyaltyPoints = 0 };
                _context.UserDetails.Add(userDetail);
                await _context.SaveChangesAsync();
            }

            // 2. جلب الطلبات
            var orders = await _context.Orders
                .Where(o => o.CustomerName == user.UserName || o.PhoneNumber == user.PhoneNumber)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // 3. جلب الإشعارات
            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.Date)
                .ToListAsync();

            // 4. جلب المفضلة
            var wishlistProducts = new List<Product>();
            var wishlistCookie = Request.Cookies["SadStore_Wishlist"];
            if (!string.IsNullOrEmpty(wishlistCookie))
            {
                try
                {
                    var ids = JsonSerializer.Deserialize<List<int>>(wishlistCookie);
                    if (ids != null && ids.Any())
                        wishlistProducts = await _context.Products.Where(p => ids.Contains(p.Id)).ToListAsync();
                }
                catch { }
            }

            ViewBag.UserName = user.UserName;
            ViewBag.UserEmail = user.Email;
            ViewBag.UserPhone = user.PhoneNumber;

            ViewBag.WalletBalance = userDetail.WalletBalance;
            ViewBag.LoyaltyPoints = userDetail.LoyaltyPoints;
            ViewBag.ProfileImageUrl = userDetail.ProfileImageUrl; // رابط الصورة

            ViewBag.Notifications = notifications;
            ViewBag.Wishlist = wishlistProducts;
            ViewBag.PendingOrders = orders.Where(o => o.Status == "جديد" || o.Status == "Pending Payment").ToList();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInfo(string name, string phone)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.UserName = name;
                user.PhoneNumber = phone;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = "Profile updated successfully";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateImage(IFormFile imageFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Auth");

            if (imageFile != null && imageFile.Length > 0)
            {
                // مسار المجلد
                string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "images", "profiles");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                // اسم ملف فريد
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadDir, fileName);

                // حفظ الملف
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // تحديث قاعدة البيانات
                var userDetail = await _context.UserDetails.FirstOrDefaultAsync(u => u.UserId == user.Id);
                if (userDetail == null)
                {
                    userDetail = new UserDetail { UserId = user.Id };
                    _context.UserDetails.Add(userDetail);
                }

                // حذف الصورة القديمة إذا وجدت (اختياري لتنظيف السيرفر)
                // if (!string.IsNullOrEmpty(userDetail.ProfileImageUrl)) ...

                userDetail.ProfileImageUrl = "/images/profiles/" + fileName;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profile image updated";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Auth");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Password changed successfully";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Auth");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null || (order.CustomerName != user.UserName && order.PhoneNumber != user.PhoneNumber))
            {
                return NotFound();
            }

            return View(order);
        }
    }
}