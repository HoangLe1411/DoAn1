using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAn1.Areas.Admin.Models;

namespace DoAn1.Controllers
{
    public class UsersController : Controller
    {
        private readonly CsdlDoAn1Context _context;

        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(string Username, string Password)
        {
            var user = _context.Users.FirstOrDefault(u =>
                (u.Username == Username || u.Email == Username) && u.IsActive == true);

            if (user == null)
            {
                ViewBag.Error = "Tài khoản không tồn tại.";
                return View();
            }

            if (user.PasswordHash != Password)
            {
                ViewBag.Error = "Mật khẩu không đúng.";
                return View();
            }

            // ✅ LƯU ĐỦ THÔNG TIN VÀO SESSION
            HttpContext.Session.SetInt32("UserId", user.UserId); // <-- Cái này bạn bị thiếu
            HttpContext.Session.SetString("Username", user.Username ?? "");
            HttpContext.Session.SetString("FullName", user.FullName ?? user.Username ?? "");
            HttpContext.Session.SetString("Role", user.Role ?? "User");

            return RedirectToAction("Index", "Home");
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        public IActionResult Register(User model)
        {
            if (_context.Users.Any(u => u.Username == model.Username || u.Email == model.Email))
            {
                ViewBag.Error = "Tên đăng nhập hoặc email đã tồn tại.";
                return View();
            }

            // Gán mặc định các giá trị cần thiết
            model.Role = "User"; // Mặc định là người dùng
            model.IsActive = true;
            model.ReputationScore = 0;

            // Nếu bạn không dùng mã hóa, giữ nguyên PasswordHash
            // Nếu có dùng mã hóa như BCrypt, thì xử lý ở đây

            _context.Users.Add(model);
            _context.SaveChanges();

            ViewBag.Success = "Đăng ký thành công!";
            ModelState.Clear(); // Xoá form

            return View();
        }

        public UsersController(CsdlDoAn1Context context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Nếu không truyền id => lấy từ Session
            if (id == null)
            {
                id = HttpContext.Session.GetInt32("UserId");
            }

            // Nếu vẫn không có id => chưa đăng nhập
            if (id == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Username,FullName,Email,Phone,Address,PasswordHash,Role,ReputationScore,IsActive")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,FullName,Email,Phone,Address,PasswordHash")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các trường cho phép chỉnh sửa
                    existingUser.FullName = user.FullName;
                    existingUser.Email = user.Email;
                    existingUser.Phone = user.Phone;
                    existingUser.Address = user.Address;

                    // Giữ nguyên Role, ReputationScore và IsActive
                    // => KHÔNG ghi đè từ form

                    // Nếu PasswordHash được nhập mới thì cập nhật
                    if (!string.IsNullOrWhiteSpace(user.PasswordHash))
                    {
                        existingUser.PasswordHash = user.PasswordHash;
                    }

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();

                    // Hiển thị thông báo thành công
                    ViewData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return View(existingUser); // Ở lại trang Edit
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(user);
        }



        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
