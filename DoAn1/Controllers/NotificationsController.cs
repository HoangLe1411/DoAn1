using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DoAn1.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAn1.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly CsdlDoAn1Context _context;

        public NotificationsController(CsdlDoAn1Context context)
        {
            _context = context;
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            var csdlDoAn1Context = _context.Notifications.Include(n => n.User);
            return View(await csdlDoAn1Context.ToListAsync());
        }

        public async Task<IActionResult> NewNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (currentUser == null)
            {
                return NotFound("User not found.");
            }

            var newNotifications = await _context.Notifications
                .Where(n => n.UserId == currentUser.UserId && n.IsRead == false)
                .ToListAsync();

            return View(newNotifications);
        }

        public IActionResult OrderDetails(int id)
        {
            var transaction = _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .FirstOrDefault(t => t.TransactionId == id);

            if (transaction == null)
                return NotFound();

            return View(transaction);
        }


        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var notify = _context.Notifications.FirstOrDefault(n => n.NotificationId == id);
            if (notify != null && !notify.IsRead.GetValueOrDefault())
            {
                notify.IsRead = true;
                _context.SaveChanges();
            }

            if (notify?.TransactionId != null)
            {
                // Nếu tiêu đề là "Cập nhật trạng thái đơn hàng" => chuyển đến Track
                if (!string.IsNullOrEmpty(notify.Title) && notify.Title.Contains("Cập nhật trạng thái"))
                {
                    return RedirectToAction("Track", "Transactions", new { id = notify.TransactionId });
                }

                // Các thông báo khác liên quan đến giao dịch => SellerOrthers
                return RedirectToAction("SellerOrthers", "Transactions", new { id = notify.TransactionId });
            }
            else if (notify != null && !string.IsNullOrEmpty(notify.Title) && notify.Title.StartsWith("Cảnh báo"))
            {
                // Nếu là thông báo cảnh báo từ báo cáo
                return RedirectToAction("MyReports", "Reports");
            }

            return RedirectToAction("NewNotifications");
        }


        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.NotificationId == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // GET: Notifications/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Notifications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReportedUserId,Reason")] Report report)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (ModelState.IsValid)
            {
                report.ReporterId = currentUserId.Value;
                report.ReportedAt = DateTime.Now;
                report.IsResolved = false;

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                // ✅ Gửi thông báo cho người bị báo cáo
                var notification = new Notification
                {
                    UserId = report.ReportedUserId,
                    Title = "Cảnh báo: Bạn đã bị báo cáo",
                    Content = $"Tài khoản của bạn đã bị báo cáo với lý do: {report.Reason}",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Báo cáo đã được gửi thành công!";
                return RedirectToAction("Index", "Home");
            }

            return View(report);
        }


        // GET: Notifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", notification.UserId);
            return View(notification);
        }

        // POST: Notifications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NotificationId,UserId,TransactionId,Title,Content,IsRead,CreatedAt")] Notification notification)
        {
            if (id != notification.NotificationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.NotificationId))
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
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", notification.UserId);
            return View(notification);
        }

        // GET: Notifications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.NotificationId == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.NotificationId == id);
        }
    }
}
