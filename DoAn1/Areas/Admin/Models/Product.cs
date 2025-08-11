using System;
using System.Collections.Generic;

namespace DoAn1.Areas.Admin.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int? UserId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Status { get; set; }

    public string? Condition { get; set; }

    public string? Location { get; set; }

    public int? CategoryId { get; set; }

    public int? SubCategoryId { get; set; }

    public bool? IsSold { get; set; }

    public string? Brand { get; set; }

    public string? WarrantyPeriod { get; set; }

    public string? UsedDuration { get; set; }

    public string? Type { get; set; }

    public int? TotalRating { get; set; }

    public int? RatingCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual SubCategory? SubCategory { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User? User { get; set; }

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
