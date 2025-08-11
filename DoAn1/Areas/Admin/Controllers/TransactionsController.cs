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
    public class TransactionsController : Controller
    {
        private readonly CsdlDoAn1Context _context;

        public TransactionsController(CsdlDoAn1Context context)
        {
            _context = context;
        }

        // GET: Admin/Transactions
        public async Task<IActionResult> Index()
        {
            var csdlDoAn1Context = _context.Transactions.Include(t => t.Buyer).Include(t => t.PaymentMethod).Include(t => t.Product).Include(t => t.Seller);
            return View(await csdlDoAn1Context.ToListAsync());
        }

        // GET: Admin/Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.PaymentMethod)
                .Include(t => t.Product)
                .Include(t => t.Seller)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Admin/Transactions/Create
        public IActionResult Create()
        {
            ViewData["BuyerId"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "PaymentMethodId", "PaymentMethodId");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId");
            ViewData["SellerId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Admin/Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,BuyerId,SellerId,ProductId,PaymentMethodId,PaymentStatus,Amount,ExchangeType,TransactionDate,ShippingStatus")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BuyerId"] = new SelectList(_context.Users, "UserId", "UserId", transaction.BuyerId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "PaymentMethodId", "PaymentMethodId", transaction.PaymentMethodId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", transaction.ProductId);
            ViewData["SellerId"] = new SelectList(_context.Users, "UserId", "UserId", transaction.SellerId);
            return View(transaction);
        }

        // GET: Admin/Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            ViewData["BuyerId"] = new SelectList(_context.Users, "UserId", "UserId", transaction.BuyerId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "PaymentMethodId", "PaymentMethodId", transaction.PaymentMethodId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", transaction.ProductId);
            ViewData["SellerId"] = new SelectList(_context.Users, "UserId", "UserId", transaction.SellerId);
            return View(transaction);
        }

        // POST: Admin/Transactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,BuyerId,SellerId,ProductId,PaymentMethodId,PaymentStatus,Amount,ExchangeType,TransactionDate,ShippingStatus")] Transaction transaction)
        {
            if (id != transaction.TransactionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.TransactionId))
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
            ViewData["BuyerId"] = new SelectList(_context.Users, "UserId", "UserId", transaction.BuyerId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "PaymentMethodId", "PaymentMethodId", transaction.PaymentMethodId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", transaction.ProductId);
            ViewData["SellerId"] = new SelectList(_context.Users, "UserId", "UserId", transaction.SellerId);
            return View(transaction);
        }

        // GET: Admin/Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.PaymentMethod)
                .Include(t => t.Product)
                .Include(t => t.Seller)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Admin/Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionId == id);
        }
    }
}
