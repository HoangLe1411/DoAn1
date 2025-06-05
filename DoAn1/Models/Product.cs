namespace DoAn1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; }      // Tên sản phẩm
        public decimal Price { get; set; }            // Giá tiền
        public string Location { get; set; }          // Địa điểm
        public string SellerName { get; set; }        // Tên người bán
        public string Description { get; set; }       // Mô tả chi tiết
        public string Condition { get; set; }         // Tình trạng (mới, cũ, ...)
        public string Category { get; set; }          // Loại sản phẩm
    }
}
