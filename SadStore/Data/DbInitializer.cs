using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SadStore.Data; // Ensure correct namespace

namespace SadStore.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            try
            {
                using var context = serviceProvider.GetRequiredService<StoreContext>();
                using var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
                using var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                context.Database.EnsureCreated();

                // Roles
                string[] roles = { "Admin", "Customer" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Admin
                var adminEmail = "admin@store.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var adminUser = new IdentityUser
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

                // Categories
                if (!context.Categories.Any())
                {
                    var categories = new List<Category>
                    {
                        new Category { NameAr = "عبايات", NameEn = "Abayas", ImageUrl = "/images/cat-abayas.jpg" },
                        new Category { NameAr = "أقمشة", NameEn = "Fabrics", ImageUrl = "/images/cat-fabrics.jpg" },
                        new Category { NameAr = "ملابس", NameEn = "Clothes", ImageUrl = "/images/cat-clothes.jpg" },
                        new Category { NameAr = "حياكة وزخرفة", NameEn = "Leather", ImageUrl = "/images/cat-leather.jpg" },
                        new Category { NameAr = "تطريز", NameEn = "Embroidery", ImageUrl = "/images/cat-embroidery.jpg" },
                        new Category { NameAr = "شنط وأحزمة", NameEn = "Bags", ImageUrl = "/images/cat-bags.jpg" },
                         new Category { NameAr = "جلود خام", NameEn = "RawLeather", ImageUrl = "/images/cat-raw.jpg" },
                         new Category { NameAr = "توزيعات", NameEn = "Supplies", ImageUrl = "/images/cat-sup.jpg" }
                    };
                    context.Categories.AddRange(categories);
                    await context.SaveChangesAsync();

                    // Products (Only added if Categories were added to ensure linkage)
                    var products = new List<Product>
                    {
                        new Product
                        {
                            NameAr = "عباية سوداء مطرزة",
                            NameEn = "Black Embroidered Abaya",
                            DescriptionAr = "عباية سوداء فاخرة.",
                            DescriptionEn = "Luxury black abaya.",
                            Price = 350.00m,
                            StockQuantity = 10,
                            CategoryId = categories[0].Id,
                            ImageUrl = "/images/product.webp",
                            IsFeatured = true
                        },
                         new Product
                        {
                            NameAr = "فستان سهرة",
                            NameEn = "Evening Dress",
                            DescriptionAr = "فستان أنيق للحفلات.",
                            DescriptionEn = "Elegant party dress.",
                            Price = 500.00m,
                            StockQuantity = 5,
                            CategoryId = categories[2].Id,
                            ImageUrl = "/images/product.webp",
                            IsFeatured = true
                        }
                    };
                    context.Products.AddRange(products);
                    await context.SaveChangesAsync();
                }

                // Shipping
                if (!context.ShippingLocations.Any())
                {
                    context.ShippingLocations.AddRange(
                        new ShippingLocation { CityName = "الرياض", ShippingCost = 25.00m },
                        new ShippingLocation { CityName = "جدة", ShippingCost = 25.00m },
                        new ShippingLocation { CityName = "باقي المدن", ShippingCost = 35.00m }
                    );
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // Log error silently or rethrow
            }
        }
    }
}