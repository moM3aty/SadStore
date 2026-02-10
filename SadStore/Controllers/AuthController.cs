using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SadStore.Services;

namespace SadStore.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LanguageService _lang;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, LanguageService lang)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _lang = lang;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Profile");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: true, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }
                    TempData["SuccessMessage"] = "Logged in successfully";
                    return RedirectToAction("Index", "Profile");
                }
                ModelState.AddModelError(string.Empty, _lang.Get("Invalid login attempt"));
            }
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Register(string name, string email, string phone, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, _lang.Get("Passwords do not match"));
                return View("Index");
            }

            if (ModelState.IsValid)
            {
               
                var user = new IdentityUser
                {
                    UserName = email, // نستخدم الايميل كاسم مستخدم لضمان التفرد
                    Email = email,
                    PhoneNumber = phone
                };

                // يمكنك تخزين الاسم الحقيقي (name) في Claim إذا أردت لاحقاً

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["SuccessMessage"] = "Logged in successfully";
                    return RedirectToAction("Index", "Profile");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View("Index");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}