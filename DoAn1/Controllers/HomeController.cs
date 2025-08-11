using System.Diagnostics;
using DoAn1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAn1.Areas.Admin.Models; // namespace chứa Product

namespace DoAn1.Controllers
{
    public class HomeController : Controller
    {
        private readonly CsdlDoAn1Context _context;

        public HomeController(CsdlDoAn1Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Sản phẩm mới nhất (đã có sẵn)
            var latestProducts = await _context.Products
                .Where(p => p.IsActive == true && p.Status != "Đã bán")
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.ProductImages)
                .Take(6)
                .ToListAsync();

            // 2. Sản phẩm nổi bật theo sao
            var featuredProducts = await _context.Products
                .Where(p => p.IsActive == true && p.RatingCount > 0)
                .OrderByDescending(p =>
                    (p.TotalRating ?? 0) == 0 || (p.RatingCount ?? 0) == 0
                        ? 0
                        : (double)(p.TotalRating ?? 0) / (p.RatingCount ?? 1)
                )

                .ThenByDescending(p => p.CreatedAt)
                .Include(p => p.ProductImages)
                .Take(6)
                .ToListAsync();

            // Gửi vào ViewBag
            ViewBag.FeaturedProducts = featuredProducts;

            return View(latestProducts); // Dùng model chính là sản phẩm mới
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
