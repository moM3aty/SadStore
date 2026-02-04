using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SadStore.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = serviceProvider.GetRequiredService<StoreContext>();
            using var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            using var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // التأكد من إنشاء قاعدة البيانات
            context.Database.EnsureCreated();

            // 1. إنشاء الأدوار (Roles)
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                await roleManager.CreateAsync(new IdentityRole("Customer"));
            }

            // 2. إنشاء مستخدم Admin افتراضي
            var adminEmail = "admin@store.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "0500000000"
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. إضافة بيانات تجريبية إذا كانت قاعدة البيانات فارغة
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { NameAr = "عبايات", NameEn = "Abayas", ImageUrl = "/images/cat-abayas.jpg" },
                    new Category { NameAr = "أقمشة", NameEn = "Fabrics", ImageUrl = "/images/cat-fabrics.jpg" },
                    new Category { NameAr = "ملابس", NameEn = "Clothes", ImageUrl = "/images/cat-clothes.jpg" },
                    new Category { NameAr = "عطور", NameEn = "Perfumes", ImageUrl = "/images/cat-perfumes.jpg" }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();

                // إضافة منتجات تجريبية
                var products = new List<Product>
                {
                    new Product
                    {
                        NameAr = "عباية سوداء مطرزة",
                        NameEn = "Black Embroidered Abaya",
                        DescriptionAr = "عباية سوداء فاخرة مع تطريز يدوي على الأكمام.",
                        DescriptionEn = "Luxury black abaya with hand embroidery on sleeves.",
                        Price = 350.00m,
                        StockQuantity = 10,
                        CategoryId = categories[0].Id,
                        ImageUrl = "/images/product.webp",
                        IsFeatured = true
                    },
                    new Product
                    {
                        NameAr = "قماش حرير ياباني",
                        NameEn = "Japanese Silk Fabric",
                        DescriptionAr = "قماش حرير ياباني ناعم ومريح.",
                        DescriptionEn = "Soft and comfortable Japanese silk fabric.",
                        Price = 120.00m,
                        StockQuantity = 50,
                        CategoryId = categories[1].Id,
                        ImageUrl = "/images/product.webp",
                        IsFeatured = true
                    }
                };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }

            if (!context.ShippingLocations.Any())
            {
                context.ShippingLocations.AddRange(
                    new ShippingLocation { CityName = "الرياض", ShippingCost = 25.00m },
                    new ShippingLocation { CityName = "جدة", ShippingCost = 25.00m },
                    new ShippingLocation { CityName = "الدمام", ShippingCost = 30.00m },
                    new ShippingLocation { CityName = "باقي المدن", ShippingCost = 35.00m }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}