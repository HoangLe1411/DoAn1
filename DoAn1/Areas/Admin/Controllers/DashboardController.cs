using Microsoft.AspNetCore.Mvc;

namespace DoAn1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("AdminUsername") == null
                || HttpContext.Session.GetString("AdminRole") != "Admin")
            {
                return RedirectToAction("Login", "Users");
            }

            return View();
        }
    }
}
