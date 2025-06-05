using Microsoft.AspNetCore.Mvc;

namespace DoAn1.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Cart()
        {
            return View();  // Tìm Views/Cart/Cart.cshtml
        }
    }
}
