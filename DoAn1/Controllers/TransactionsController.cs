using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using DoAn1.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace DoAn1.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly CsdlDoAn1Context _context;

        public TransactionsController(CsdlDoAn1Context context)
        {
            _context = context;
        }

        public IActionResult OrderHistory()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Users");

            var orders = _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Seller)
                .Where(t => t.BuyerId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            return View(orders);
        }

        public IActionResult SellerOrderHistory()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            var orders = _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .Where(t => t.SellerId == userId) // Lấy các đơn bán ra
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            return View(orders);
        }


        // GET: /Transactions/Track/5
        // GET: /Transactions/Track/5 
        public async Task<IActionResult> Track(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            var transaction = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Seller)
                .Include(t => t.Buyer)
                .FirstOrDefaultAsync(t => t.TransactionId == id
                                          && (t.BuyerId == userId || t.SellerId == userId));

            if (transaction == null)
                return NotFound();

            // Đảm bảo TempData["Success"] vẫn tồn tại cho View
            TempData.Keep("Success");
            return View(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Product)
                .FirstOrDefaultAsync(t => t.TransactionId == id);

            if (transaction == null) return NotFound();

            var steps = new[] { "Pending", "Confirmed", "Shipping", "Delivered", "Received" };

            int currentIndex = Array.IndexOf(steps, transaction.ShippingStatus ?? "Pending");
            int newIndex = Array.IndexOf(steps, status);

            // ✅ Chỉ cho phép cập nhật đúng bước tiếp theo hoặc giữ nguyên
            if (newIndex == currentIndex + 1 || newIndex == currentIndex)
            {
                transaction.ShippingStatus = status;

                // Gửi thông báo cho người mua
                _context.Notifications.Add(new Notification
                {
                    UserId = transaction.BuyerId,
                    TransactionId = transaction.TransactionId,
                    Title = "Cập nhật trạng thái đơn hàng",
                    Content = $"Đơn hàng '{transaction.Product?.Title}' đã được cập nhật sang trạng thái: {status}.",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }
            else
            {
                TempData["Error"] = "Bạn phải cập nhật trạng thái theo đúng thứ tự!";
            }

            return RedirectToAction("Track", new { id });
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmReceived(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Seller)
                .FirstOrDefaultAsync(t => t.TransactionId == id);

            if (transaction == null)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || transaction.BuyerId != userId || transaction.ShippingStatus != "Delivered")
            {
                TempData["Error"] = "Bạn không thể xác nhận nhận hàng lúc này!";
                return RedirectToAction("Track", new { id });
            }

            // Cập nhật trạng thái
            transaction.ShippingStatus = "Received";

            // ✅ Cộng điểm uy tín cho người bán
            if (transaction.Seller != null)
            {
                transaction.Seller.ReputationScore = (transaction.Seller.ReputationScore ?? 0) + 1;
            }

            // ✅ Gửi thông báo cho người bán
            _context.Notifications.Add(new Notification
            {
                UserId = transaction.SellerId,
                TransactionId = transaction.TransactionId,
                Title = "Người mua đã nhận hàng",
                Content = $"Người mua đã xác nhận nhận hàng cho sản phẩm '{transaction.Product?.Title}'.",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["Message"] = "Xác nhận nhận hàng thành công!";
            return RedirectToAction("Track", new { id });
        }



        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var csdlDoAn1Context = _context.Transactions.Include(t => t.Buyer).Include(t => t.PaymentMethod).Include(t => t.Product).Include(t => t.Seller);
            return View(await csdlDoAn1Context.ToListAsync());
        }

        // GET: Transactions/Details/5
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

        // GET: Transactions/Create
        public IActionResult Create(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            var currentUser = _context.Users.FirstOrDefault(u => u.Username == User.Identity.Name);

            ViewBag.Product = product;
            ViewBag.CurrentUser = currentUser;

            ViewBag.PaymentMethodId = new SelectList(_context.PaymentMethods.Where(p => p.IsActive == true),
                                                     "PaymentMethodId", "MethodName");

            return View();
        }


        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Transactions/Create
        // POST: Transactions/Create
        [HttpPost]
        public async Task<IActionResult> Create(Transaction transaction, int quantity)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var product = await _context.Products.FindAsync(transaction.ProductId);
            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Index", "Products");
            }

            transaction.BuyerId = userId.Value;
            transaction.SellerId = product.UserId;
            transaction.TransactionDate = DateTime.Now;
            transaction.Amount = product.Price * quantity;
            transaction.PaymentStatus = "Pending";
            transaction.ShippingStatus = "Pending";
            transaction.ExchangeType = "Mua";

            _context.Add(transaction);
            await _context.SaveChangesAsync();

            // Tạo thông báo cho người bán
            if (product.UserId != null)
            {
                var newNotification = new Notification
                {
                    UserId = product.UserId.Value,
                    TransactionId = transaction.TransactionId,
                    Title = "Bạn có đơn hàng mới",
                    Content = $"Sản phẩm '{product.Title}' đã được đặt mua với số lượng {quantity}.",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(newNotification);
                await _context.SaveChangesAsync();
            }

            var selectedMethod = _context.PaymentMethods
                .FirstOrDefault(p => p.PaymentMethodId == transaction.PaymentMethodId)?.MethodName;

            if (selectedMethod == "VNPay")
            {
                return Redirect($"/Payment/VNPay?transactionId={transaction.TransactionId}");
            }
            else if (selectedMethod == "Momo")
            {
                return Redirect($"/Payment/Momo?transactionId={transaction.TransactionId}");
            }

            TempData["Success"] = "🎉 Đặt hàng thành công!";
            return RedirectToAction("Track", "Transactions", new { id = transaction.TransactionId });
        }

        // Tạo QR code bằng PngByteQRCode
        public IActionResult GenerateQR(string refCode, decimal amount)
        {
            var qrContent = $"Thanh toán {refCode}, số tiền {amount:N0} VND";

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20); // 20: pixel size

                return File(qrCodeBytes, "image/png");
            }
        }




        // GET: Transactions/Edit/5
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

        // POST: Transactions/Edit/5
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

        // GET: Transactions/Delete/5
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

        // POST: Transactions/Delete/5
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

        public async Task<IActionResult> BuyerOrders()
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

            var orders = await _context.Transactions
                .Include(t => t.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(t => t.Seller)
                .Where(t => t.BuyerId == currentUser.UserId)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            var order = _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Seller)
                .FirstOrDefault(t => t.TransactionId == id);

            if (order != null && order.ShippingStatus != "Shipping" &&
                order.ShippingStatus != "Delivered" && order.ShippingStatus != "Received" &&
                order.ShippingStatus != "Cancelled")
            {
                order.ShippingStatus = "Cancelled";

                // Gửi thông báo cho người bán
                _context.Notifications.Add(new Notification
                {
                    UserId = order.SellerId,
                    TransactionId = order.TransactionId,
                    Title = "Đơn hàng bị hủy",
                    Content = $"Người mua đã hủy đơn hàng '{order.Product?.Title}'.",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đơn hàng đã được hủy.";
            }
            return RedirectToAction("BuyerOrders");
        }


        [HttpPost]
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Transactions.FirstOrDefault(t => t.TransactionId == id);
            if (order != null && order.ShippingStatus == "Cancelled")
            {
                _context.Transactions.Remove(order);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đơn hàng đã được xóa.";
            }
            return RedirectToAction("BuyerOrders");
        }

        // GET: Danh sách các đơn hàng người bán nhận được
        public IActionResult SellerOrthers()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var seller = _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (seller == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var transactions = _context.Transactions
                .Include(t => t.Product)
                .ThenInclude(p => p.User) // để truy cập thông tin người bán
                .Include(t => t.Buyer)
                .Where(t => t.Product.UserId == seller.UserId) // lấy các đơn mà seller đăng sản phẩm
                .ToList();

            return View("SellerOrthers", transactions);
        }


        [HttpPost]
        public IActionResult XacNhanDon(int id)
        {
            var transaction = _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .FirstOrDefault(t => t.TransactionId == id);

            if (transaction == null)
            {
                return NotFound();
            }

            // Chỉ cập nhật nếu đang ở trạng thái Pending
            if (transaction.ShippingStatus == "Pending")
            {
                transaction.ShippingStatus = "Confirmed";

                // Gửi thông báo cho người mua
                _context.Notifications.Add(new Notification
                {
                    UserId = transaction.BuyerId,
                    TransactionId = transaction.TransactionId,
                    Title = "Cập nhật trạng thái đơn hàng",
                    Content = $"Đơn hàng '{transaction.Product?.Title}' đã được người bán xác nhận. Đơn hàng đang chuẩn bị để vận chuyển.",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

                _context.SaveChanges();
            }
            else
            {
                // Nếu seller cố gắng xác nhận lại đơn đã được xác nhận hoặc đã ở trạng thái khác
                TempData["Error"] = "Đơn hàng này đã được xử lý hoặc không còn ở trạng thái chờ xác nhận.";
            }

            return RedirectToAction("SellerOrthers");
        }

        // Người bán hủy đơn hàng
        [HttpPost]
        public IActionResult SellerCancelOrder(int id)
        {
            var order = _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .FirstOrDefault(t => t.TransactionId == id);

            if (order != null && order.ShippingStatus != "Shipping" &&
                order.ShippingStatus != "Delivered" && order.ShippingStatus != "Received" &&
                order.ShippingStatus != "Cancelled")
            {
                order.ShippingStatus = "Cancelled";

                // Gửi thông báo cho người mua
                _context.Notifications.Add(new Notification
                {
                    UserId = order.BuyerId,
                    TransactionId = order.TransactionId,
                    Title = "Đơn hàng bị hủy bởi người bán",
                    Content = $"Người bán đã hủy đơn hàng '{order.Product?.Title}'.",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đơn hàng đã được hủy.";
            }
            return RedirectToAction("SellerOrthers");
        }


        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionId == id);
        }
    }
}
