using Microsoft.AspNetCore.Mvc;

namespace DoAn1.Controllers
{
    public class SellerController : Controller
    {
        public IActionResult SellerOrders()
        {
            // Logic lấy danh sách đơn bán
            return View();
        }
    }
}
