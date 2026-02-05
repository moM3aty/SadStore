using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SadStore.Data
{
    public class StoreContext : IdentityDbContext
    {
        public StoreContext(DbContextOptions<StoreContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<CustomerReview> CustomerReviews { get; set; }
        public DbSet<ShippingLocation> ShippingLocations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<UserDetail> UserDetails { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }
    public class UserDetail
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal WalletBalance { get; set; } = 0;
        public int LoyaltyPoints { get; set; } = 0;
        public string? ProfileImageUrl { get; set; }
    }
    public class Category
    {
        public int Id { get; set; }
        [Required]
        public string NameAr { get; set; }
        [Required]
        public string NameEn { get; set; }
        public string? ImageUrl { get; set; } 
        public List<Product>? Products { get; set; } 
    }

    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string NameAr { get; set; }
        [Required]
        public string NameEn { get; set; }
        public string? DescriptionAr { get; set; } 
        public string? DescriptionEn { get; set; } 

        public string? ModelNumber { get; set; }
        public string? AvailableSizes { get; set; }

        public string? DesignTypeAr { get; set; }
        public string? DesignTypeEn { get; set; }
        public string? CutTypeAr { get; set; }
        public string? CutTypeEn { get; set; }
        public string? OccasionAr { get; set; }
        public string? OccasionEn { get; set; }
        public string? FitTypeAr { get; set; }
        public string? FitTypeEn { get; set; }
        public string? LiningAr { get; set; }
        public string? LiningEn { get; set; }
        public string? SleevesAr { get; set; }
        public string? SleevesEn { get; set; }
        public string? StretchAr { get; set; }
        public string? StretchEn { get; set; }
        public string? MaterialAr { get; set; }
        public string? MaterialEn { get; set; }
        public string? CareInstructionsAr { get; set; }
        public string? CareInstructionsEn { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; }

        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; } 

        public List<ProductImage>? Images { get; set; } = new List<ProductImage>();

        public int CategoryId { get; set; }
        public Category? Category { get; set; } 
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class ProductImage
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; } 
    }

    public class CustomerReview
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
    }

    public class ShippingLocation
    {
        public int Id { get; set; }
        [Required]
        public string CityNameAr { get; set; } 
        [Required]
        public string CityNameEn { get; set; } 
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<OrderItem>? OrderItems { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }

    public class BlogPost
    {
        public int Id { get; set; }
        [Required]
        public string TitleAr { get; set; } 
        [Required]
        public string TitleEn { get; set; } 
        [Required]
        public string ContentAr { get; set; } 
        [Required]
        public string ContentEn { get; set; } 
        public string? ImageUrl { get; set; }
        public DateTime PublishedDate { get; set; } = DateTime.Now;
    }
}