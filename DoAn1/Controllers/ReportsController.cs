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
    public class ReportsController : Controller
    {
        private readonly CsdlDoAn1Context _context;

        public ReportsController(CsdlDoAn1Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> MyReports()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var myReports = await _context.Reports
                .Where(r => r.ReportedUserId == currentUserId)
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync();

            return View(myReports);
        }


        // GET: Reports
        public async Task<IActionResult> Index()
        {
            var csdlDoAn1Context = _context.Reports.Include(r => r.ReportedUser).Include(r => r.Reporter);
            return View(await csdlDoAn1Context.ToListAsync());
        }

        // GET: Reports/Details/5
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

        // GET: Reports/Create
        public IActionResult Create(int reportedUserId)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var report = new Report
            {
                ReportedUserId = reportedUserId
            };
            return View(report);
        }

        // POST: Reports/Create
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

                TempData["Message"] = "Báo cáo đã được gửi thành công!";
                return RedirectToAction("Index", "Home");
            }

            return View(report);
        }

        // GET: Reports/Edit/5
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

        // POST: Reports/Edit/5
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

        // GET: Reports/Delete/5
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

        // POST: Reports/Delete/5
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
