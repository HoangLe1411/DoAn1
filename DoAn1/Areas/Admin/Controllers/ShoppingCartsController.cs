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
    public class ShoppingCartsController : Controller
    {
        private readonly CsdlDoAn1Context _context;

        public ShoppingCartsController(CsdlDoAn1Context context)
        {
            _context = context;
        }

        // GET: Admin/ShoppingCarts
        public async Task<IActionResult> Index()
        {
            var csdlDoAn1Context = _context.ShoppingCarts.Include(s => s.Product).Include(s => s.User);
            return View(await csdlDoAn1Context.ToListAsync());
        }

        // GET: Admin/ShoppingCarts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shoppingCart = await _context.ShoppingCarts
                .Include(s => s.Product)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.CartItemId == id);
            if (shoppingCart == null)
            {
                return NotFound();
            }

            return View(shoppingCart);
        }

        // GET: Admin/ShoppingCarts/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Admin/ShoppingCarts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CartItemId,UserId,ProductId,Quantity,AddedAt,IsCheckedOut")] ShoppingCart shoppingCart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(shoppingCart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", shoppingCart.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", shoppingCart.UserId);
            return View(shoppingCart);
        }

        // GET: Admin/ShoppingCarts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shoppingCart = await _context.ShoppingCarts.FindAsync(id);
            if (shoppingCart == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", shoppingCart.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", shoppingCart.UserId);
            return View(shoppingCart);
        }

        // POST: Admin/ShoppingCarts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CartItemId,UserId,ProductId,Quantity,AddedAt,IsCheckedOut")] ShoppingCart shoppingCart)
        {
            if (id != shoppingCart.CartItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shoppingCart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShoppingCartExists(shoppingCart.CartItemId))
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
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", shoppingCart.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", shoppingCart.UserId);
            return View(shoppingCart);
        }

        // GET: Admin/ShoppingCarts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shoppingCart = await _context.ShoppingCarts
                .Include(s => s.Product)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.CartItemId == id);
            if (shoppingCart == null)
            {
                return NotFound();
            }

            return View(shoppingCart);
        }

        // POST: Admin/ShoppingCarts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shoppingCart = await _context.ShoppingCarts.FindAsync(id);
            if (shoppingCart != null)
            {
                _context.ShoppingCarts.Remove(shoppingCart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShoppingCartExists(int id)
        {
            return _context.ShoppingCarts.Any(e => e.CartItemId == id);
        }
    }
}
