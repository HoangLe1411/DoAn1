using DoAn1.Models;
using Microsoft.AspNetCore.Mvc;

namespace DoAn1.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult All()
        {
            return View();
        }

        public IActionResult Fashion()
        {
            return View();
        }

        public IActionResult Household()
        {
            return View();
        }

        public IActionResult Electronics()
        {
            return View();
        }

        public IActionResult MomBaby()
        {
            return View();
        }

        public IActionResult Vehicles()
        {
            return View();
        }

        public IActionResult SportsEntertainment()
        {
            return View();
        }

        public IActionResult Office()
        {
            return View();
        }

        public IActionResult Book()
        {
            return View();
        }

        public IActionResult Others()
        {
            return View();
        }

        public IActionResult DetailProduct(int id)
        {
            // Tạo dữ liệu giả để test View
            var product = new Product
            {
                Id = id,
                ProductName = "Xe đạp cũ",
                Price = 1000000,
                Location = "Hà Nội",
                SellerName = "Nguyễn Văn A",
                Description = "Xe đạp cũ, còn dùng tốt, phù hợp đi lại trong thành phố.",
                Condition = "Đã sử dụng",
                Category = "Xe cộ"
            };
            return View(product);
        }

    }
}
