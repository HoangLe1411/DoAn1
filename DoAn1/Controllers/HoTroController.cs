using Microsoft.AspNetCore.Mvc;

namespace DoAn1.Controllers
{
    public class HoTroController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult HuongDanMuaBan() => View();
        public IActionResult LienHeHoTro() => View();
        public IActionResult CauHoiThuongGap() => View();
        public IActionResult QuyDinhCongDong() => View();
        public IActionResult HuongDanBaoMat() => View();
        public IActionResult DieuKhoanSuDung() => View();
    }
}
