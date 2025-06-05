using Microsoft.AspNetCore.Mvc;

namespace DoAn1.Controllers
{
    public class BuyerController : Controller
    {
        public IActionResult BuyerOrders()
        {
            return View();
        }
    }
}
