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
    public class SystemNotificationsController : Controller
    {
        private readonly CsdlDoAn1Context _context;

        public SystemNotificationsController(CsdlDoAn1Context context)
        {
            _context = context;
        }

        // GET: SystemNotifications
        public async Task<IActionResult> Index()
        {
            return View(await _context.SystemNotifications.ToListAsync());
        }

        // GET: SystemNotifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemNotification = await _context.SystemNotifications
                .FirstOrDefaultAsync(m => m.NotificationId == id);
            if (systemNotification == null)
            {
                return NotFound();
            }

            return View(systemNotification);
        }

        // GET: SystemNotifications/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SystemNotifications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NotificationId,Title,Content,CreatedAt")] SystemNotification systemNotification)
        {
            if (ModelState.IsValid)
            {
                _context.Add(systemNotification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(systemNotification);
        }

        // GET: SystemNotifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemNotification = await _context.SystemNotifications.FindAsync(id);
            if (systemNotification == null)
            {
                return NotFound();
            }
            return View(systemNotification);
        }

        // POST: SystemNotifications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NotificationId,Title,Content,CreatedAt")] SystemNotification systemNotification)
        {
            if (id != systemNotification.NotificationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(systemNotification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SystemNotificationExists(systemNotification.NotificationId))
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
            return View(systemNotification);
        }

        // GET: SystemNotifications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemNotification = await _context.SystemNotifications
                .FirstOrDefaultAsync(m => m.NotificationId == id);
            if (systemNotification == null)
            {
                return NotFound();
            }

            return View(systemNotification);
        }

        // POST: SystemNotifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var systemNotification = await _context.SystemNotifications.FindAsync(id);
            if (systemNotification != null)
            {
                _context.SystemNotifications.Remove(systemNotification);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SystemNotificationExists(int id)
        {
            return _context.SystemNotifications.Any(e => e.NotificationId == id);
        }
    }
}
