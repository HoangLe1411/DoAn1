using Microsoft.AspNetCore.Mvc;

namespace DoAn1.Controllers
{
    public class MessagesController : Controller
    {
        public IActionResult Inbox()
        {
            return View();
        }
    }
}
