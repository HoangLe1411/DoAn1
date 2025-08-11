using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAn1.Areas.Admin.Models;

namespace DoAn1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportsController : Controller
    {
        private readonly CsdlDoAn1Context _context;

        public ReportsController(CsdlDoAn1Context context)
        {
            _context = context;
        }

        // Danh sách báo cáo
        public async Task<IActionResult> Index()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .ToListAsync();

            return View(reports);
        }

        // Duyệt báo cáo
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var report = await _context.Reports
                .FirstOrDefaultAsync(r => r.ReportId == id);
            if (report == null) return NotFound();

            report.IsResolved = true;
            await _context.SaveChangesAsync();

            // Đếm số lần vi phạm của user bị báo cáo
            var violationCount = await _context.Reports
                .CountAsync(r => r.ReportedUserId == report.ReportedUserId && r.IsResolved == true);

            if (violationCount >= 3)
            {
                // Xóa wishlist trước khi xóa sản phẩm
                var products = await _context.Products
                    .Where(p => p.UserId == report.ReportedUserId)
                    .ToListAsync();

                foreach (var product in products)
                {
                    var wishlists = _context.Wishlists.Where(w => w.ProductId == product.ProductId);
                    _context.Wishlists.RemoveRange(wishlists);
                }

                // Xóa sản phẩm
                _context.Products.RemoveRange(products);

                // Khóa tài khoản người bị báo cáo
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == report.ReportedUserId);
                if (user != null)
                {
                    user.IsActive = false;
                }

                await _context.SaveChangesAsync();
            }

            // Gửi thông báo
            var notification = new Notification
            {
                UserId = report.ReportedUserId,
                Title = "Cảnh báo: Báo cáo của bạn đã được duyệt",
                Content = "Admin đã duyệt báo cáo liên quan đến tài khoản của bạn. Hãy kiểm tra lại hoạt động của bạn.",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }



        // Từ chối báo cáo
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var report = await _context.Reports.FirstOrDefaultAsync(r => r.ReportId == id);
            if (report == null) return NotFound();

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UndoApproval(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null || report.IsResolved == false)
                return NotFound();

            var reportedUser = await _context.Users.FindAsync(report.ReportedUserId);
            if (reportedUser != null)
            {
                reportedUser.ReputationScore += 10;

                // Mở lại tài khoản nếu bị khóa mà giờ điểm uy tín >= -50
                if (reportedUser.ReputationScore >= -50)
                    reportedUser.IsActive = true;

                await _context.SaveChangesAsync();
            }

            report.IsResolved = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // GET: Admin/Reports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .Include(r => r.ReportedUser)
                .Include(r => r.Reporter)
                .FirstOrDefaultAsync(m => m.ReportId == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // GET: Admin/Reports/Create
        public IActionResult Create()
        {
            ViewData["ReportedUserId"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["ReporterId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Admin/Reports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReportId,ReporterId,ReportedUserId,Reason,IsResolved,ReportedAt")] Report report)
        {
            if (ModelState.IsValid)
            {
                _context.Add(report);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReportedUserId"] = new SelectList(_context.Users, "UserId", "UserId", report.ReportedUserId);
            ViewData["ReporterId"] = new SelectList(_context.Users, "UserId", "UserId", report.ReporterId);
            return View(report);
        }

        // GET: Admin/Reports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            ViewData["ReportedUserId"] = new SelectList(_context.Users, "UserId", "UserId", report.ReportedUserId);
            ViewData["ReporterId"] = new SelectList(_context.Users, "UserId", "UserId", report.ReporterId);
            return View(report);
        }

        // POST: Admin/Reports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReportId,ReporterId,ReportedUserId,Reason,IsResolved,ReportedAt")] Report report)
        {
            if (id != report.ReportId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(report);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportExists(report.ReportId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReportedUserId"] = new SelectList(_context.Users, "UserId", "UserId", report.ReportedUserId);
            ViewData["ReporterId"] = new SelectList(_context.Users, "UserId", "UserId", report.ReporterId);
            return View(report);
        }

        // GET: Admin/Reports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .Include(r => r.ReportedUser)
                .Include(r => r.Reporter)
                .FirstOrDefaultAsync(m => m.ReportId == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Admin/Reports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReportExists(int id)
        {
            return _context.Reports.Any(e => e.ReportId == id);
        }
    }
}
