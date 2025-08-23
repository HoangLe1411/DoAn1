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
    public class ShoppingCartsController : Controller
    {
        private readonly CsdlDoAn1Context _context;

        public ShoppingCartsController(CsdlDoAn1Context context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // GET: /ShoppingCarts
        public async Task<IActionResult> Index()
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var cartItems = await _context.ShoppingCarts
                .Where(c => c.UserId == userId.Value && c.IsCheckedOut == false)
                .Include(c => c.Product)
                .ToListAsync();

            return View(cartItems);
        }

        // POST: /ShoppingCarts/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var existing = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == userId.Value && c.ProductId == productId && c.IsCheckedOut == false);

            if (existing != null)
            {
                existing.Quantity = (existing.Quantity ?? 0) + quantity;
                _context.Update(existing);
            }
            else
            {
                var cartItem = new ShoppingCart
                {
                    UserId = userId.Value,
                    ProductId = productId,
                    Quantity = quantity,
                    AddedAt = DateTime.Now,
                    IsCheckedOut = false
                };
                _context.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /ShoppingCart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var cartItem = await _context.ShoppingCarts.FindAsync(cartItemId);
            if (cartItem != null && cartItem.UserId == userId && quantity > 0)
            {
                cartItem.Quantity = quantity;
                _context.Update(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /ShoppingCart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var cartItem = await _context.ShoppingCarts.FindAsync(cartItemId);
            if (cartItem != null && cartItem.UserId == userId)
            {
                _context.ShoppingCarts.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /ShoppingCart/Checkout
        public async Task<IActionResult> Checkout()
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }
            var cartItems = await _context.ShoppingCarts
                .Where(c => c.UserId == userId && c.IsCheckedOut == false)
                .Include(c => c.Product)
                .ToListAsync();

            var paymentMethods = await _context.PaymentMethods
                .Where(pm => pm.IsActive == true)
                .ToListAsync();

            ViewData["CartItems"] = cartItems;
            ViewData["PaymentMethods"] = paymentMethods;

            return View();
        }

        // POST: /ShoppingCart/ProcessCheckout
        [HttpPost]
        public async Task<IActionResult> ProcessCheckout(int paymentMethodId)
        {
            int? userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var cartItems = await _context.ShoppingCarts
                .Where(c => c.UserId == userId && c.IsCheckedOut == false)
                .Include(c => c.Product)
                .ToListAsync();

            if (!cartItems.Any())
                return RedirectToAction(nameof(Index));

            foreach (var item in cartItems)
            {
                var transaction = new Transaction
                {
                    BuyerId = userId.Value,
                    SellerId = item.Product?.UserId,
                    ProductId = item.ProductId,
                    PaymentMethodId = paymentMethodId,
                    PaymentStatus = "Pending",
                    Amount = (item.Quantity ?? 1) * (item.Product?.Price ?? 0),
                    ExchangeType = item.Product?.Type,
                    TransactionDate = DateTime.Now,
                    ShippingStatus = "Pending",
                };
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync(); // cần save để có TransactionId

                // 🔔 Tạo thông báo cho người bán
                if (item.Product?.UserId != null)
                {
                    var notification = new Notification
                    {
                        UserId = item.Product.UserId.Value,
                        TransactionId = transaction.TransactionId,
                        Title = "Đơn hàng mới chờ xác nhận",
                        Content = $"Bạn có đơn hàng mới cho sản phẩm '{item.Product.Title}' số lượng {item.Quantity}.",
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    _context.Notifications.Add(notification);
                }

                // Đánh dấu sản phẩm đã bán
                if (item.Product != null)
                {
                    item.Product.IsSold = true;
                    _context.Products.Update(item.Product);
                }

                // Đánh dấu sản phẩm trong giỏ đã thanh toán
                item.IsCheckedOut = true;
                _context.ShoppingCarts.Update(item);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(CheckoutSuccess));
        }

        // GET: /ShoppingCart/CheckoutSuccess
        public IActionResult CheckoutSuccess()
        {
            return View();
        }


        // GET: ShoppingCarts/Details/5
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

        // GET: ShoppingCarts/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: ShoppingCarts/Create
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

        // GET: ShoppingCarts/Edit/5
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

        // POST: ShoppingCarts/Edit/5
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

        // GET: ShoppingCarts/Delete/5
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

        // POST: ShoppingCarts/Delete/5
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
