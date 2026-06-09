using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SadStore.Data;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SadStore.Controllers.Api
{
    [Route("api/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly StoreContext _context;
        private readonly string _jwtSecret = "SadStore_Super_Secret_Key_For_JWT_Auth_2026";

        public AuthApiController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, StoreContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));

                var token = new JwtSecurityToken(
                    expires: DateTime.Now.AddDays(30),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    user = new { user.Id, user.Email, user.UserName, user.PhoneNumber }
                });
            }
            return Unauthorized(new { message = "البريد الإلكتروني أو كلمة المرور غير صحيحة" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "المستخدم موجود بالفعل!" });

            var user = new IdentityUser
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Name,
                PhoneNumber = model.Phone
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = errors });
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            // إنشاء ملف مالي (محفظة ونقاط) مبدئي للعميل
            _context.UserDetails.Add(new UserDetail { UserId = user.Id, WalletBalance = 0, LoyaltyPoints = 0 });
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إنشاء الحساب بنجاح" });
        }

        [Authorize(AuthenticationSchemes = "Bearer")] // يطلب توكن
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { message = "المستخدم غير موجود" });

            var userDetails = await _context.UserDetails.FirstOrDefaultAsync(u => u.UserId == user.Id);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                WalletBalance = userDetails?.WalletBalance ?? 0,
                LoyaltyPoints = userDetails?.LoyaltyPoints ?? 0,
                ProfileImageUrl = userDetails?.ProfileImageUrl
            });
        }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}