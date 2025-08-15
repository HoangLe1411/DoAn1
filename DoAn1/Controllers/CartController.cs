using Microsoft.AspNetCore.Mvc;

namespace DoAn1.Controllers
{
    public class CartController : Controller
    {
        public IActionResult GioiThieu()
        {
            ViewBag.Active = "gioithieu";
            return View();
        }

        public IActionResult QuyCheHoatDong()
        {
            ViewBag.Active = "quychehoatdong";
            return View();
        }

        public IActionResult QuyCheHoatDongUngDung()
        {
            ViewBag.Active = "quychehoatdongungdung";
            return View();
        }

        public IActionResult BaoMat()
        {
            ViewBag.Active = "baomat";
            return View();
        }

        public IActionResult GiaiQuyetTranhChap()
        {
            ViewBag.Active = "GiaiQuyetTranhChap";
            return View();
        }

        public IActionResult QuyCheTaiKhoan()
        {
            ViewBag.Active = "quychetaikhoan";
            return View();
        }

        public IActionResult ThoaThuanCungCap()
        {
            ViewBag.Active = "thoathuancungcap";
            return View();
        }
        public IActionResult ChinhSachHoanTien()
        {
            return View();
        }
        public IActionResult ChinhSachDoiTra()
        {
            ViewBag.Active = "ChinhSachDoiTra";
            return View();
        }

        public IActionResult ChinhSachBaoHanh()
        {
            ViewBag.Active = "ChinhSachBaoHanh";
            return View();
        }
        public IActionResult ChinhSachVanChuyen()
        {
            ViewBag.Active = "ChinhSachVanChuyen";
            return View();
        }
    }
}
