namespace DoAn1.Areas.Admin.Models
{
    public class ProductDetailViewModel
    {
        public Product? Product { get; set; } // Cho phép null
        public List<Review> Reviews { get; set; } = new List<Review>();
        public double AverageRating { get; set; }
    }
}
