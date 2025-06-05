using Microsoft.AspNetCore.Mvc;

namespace DoAn1.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult OrderForm()
        {
            return View();
        }
    }
}
