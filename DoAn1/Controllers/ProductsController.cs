using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAn1.Areas.Admin.Models;
using Newtonsoft.Json;
using PagedList.Core; // chứa IPagedList<> và ToPagedList()



namespace DoAn1.Controllers
{
    public class ProductsController : Controller
    {
        private readonly CsdlDoAn1Context _context;

        public ProductsController(CsdlDoAn1Context context)
        {
            _context = context;
        }

        public IActionResult Category(int id)
        {
            var products = _context.Products
                .Where(p => p.CategoryId == id)
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .ToList();

            return View("AllProducts", products);
        }

        public IActionResult SubCategory(int id)
        {
            var products = _context.Products
                .Where(p => p.SubCategoryId == id)
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .ToList();

            return View("AllProducts", products);
        }


        // POST: Thêm sản phẩm vào giỏ
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users"); // Chuyển về trang đăng nhập nếu chưa đăng nhập
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Lấy giỏ hàng từ Session
            var cart = HttpContext.Session.GetString("Cart");
            List<Product> cartItems = string.IsNullOrEmpty(cart)
                ? new List<Product>()
                : JsonConvert.DeserializeObject<List<Product>>(cart);

            // Thêm sản phẩm nếu chưa có
            if (!cartItems.Any(p => p.ProductId == productId))
            {
                cartItems.Add(product);
            }

            // Lưu lại vào session
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cartItems));

            return RedirectToAction("Cart");
        }



        // GET: Hiển thị giỏ hàng
        public IActionResult Cart()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users"); // Chuyển về trang đăng nhập nếu chưa đăng nhập
            }
            var cart = HttpContext.Session.GetString("Cart");
            List<Product> cartItems = string.IsNullOrEmpty(cart)
                ? new List<Product>()
                : JsonConvert.DeserializeObject<List<Product>>(cart);

            return View(cartItems);
        }


        // POST: Đặt hàng
        [HttpPost]
        public IActionResult OrderSelected(List<int> selectedProductIds)
        {

            if (selectedProductIds == null || selectedProductIds.Count == 0)
            {
                TempData["Message"] = "Vui lòng chọn ít nhất một sản phẩm để đặt hàng.";
                return RedirectToAction("Cart");
            }

            // Lấy danh sách đã chọn từ session
            var cart = HttpContext.Session.GetString("Cart");
            var cartItems = JsonConvert.DeserializeObject<List<Product>>(cart);

            var selectedItems = cartItems.Where(p => selectedProductIds.Contains(p.ProductId)).ToList();

            // TODO: xử lý tiếp – ví dụ: hiển thị đơn hàng, xác nhận, lưu vào DB (tuỳ bạn)

            return View("OrderConfirm", selectedItems);
        }


        public IActionResult MyProducts()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var myProducts = _context.Products
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.ProductId)
                .ToList();

            return View(myProducts);
        }

        [HttpPost]
        public IActionResult ToggleAvailability(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
                return NotFound();

            if (product.Status == "Đã bán")
            {
                product.Status = "Bán"; // Hoặc "Trao đổi" nếu cần
                product.IsSold = false;
            }
            else
            {
                product.Status = "Đã bán";
                product.IsSold = true;
            }

            product.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction("MyProducts"); // danh sách sản phẩm của người bán
        }


        public IActionResult ByCategory(int categoryId, int? subCategoryId, string? location, string? price, string? condition, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            // Lấy thông tin category và SubCategories
            var category = _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefault(c => c.CategoryId == categoryId);

            if (category == null)
                return NotFound();

            var productsQuery = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.SubCategory)
                .Where(p => p.CategoryId == categoryId && p.IsActive == true && (p.Status == "Bán" || p.Status == "Trao đổi") && p.IsSold == false)
                .AsQueryable();

            // Lọc SubCategory
            if (subCategoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.SubCategoryId == subCategoryId.Value);

            // Lọc Location
            if (!string.IsNullOrEmpty(location))
                productsQuery = productsQuery.Where(p => p.Location == location);

            // Lọc Condition
            if (!string.IsNullOrEmpty(condition))
            {
                string cond = condition.ToLower();
                productsQuery = productsQuery.Where(p =>
                    (cond == "new" && p.Condition != null && p.Condition.ToLower() == "mới") ||
                    (cond == "used" && p.Condition != null && p.Condition.ToLower() == "đã sử dụng")
                );
            }


            // Lọc Price
            if (!string.IsNullOrEmpty(price))
            {
                var prices = price.Split('-').Select(s => decimal.TryParse(s, out decimal val) ? val : 0).ToArray();
                if (prices.Length == 2)
                {
                    decimal minPrice = prices[0];
                    decimal maxPrice = prices[1];
                    productsQuery = productsQuery.Where(p => (p.Price ?? 0) >= minPrice && (p.Price ?? 0) <= maxPrice);
                }
            }

            // Sắp xếp
            productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);

            // Phân trang
            var pagedProducts = new PagedList<Product>(productsQuery.OrderBy(p => p.ProductId),pageNumber,pageSize);


            // ViewBag để giữ trạng thái filter
            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = category.CategoryName;
            ViewBag.SubCategories = category.SubCategories.ToList();
            ViewBag.SubCategoryId = subCategoryId;
            ViewBag.Location = location;
            ViewBag.Price = price;
            ViewBag.Condition = condition;

            return View(pagedProducts);
        }



        public IActionResult AllProducts(string? location, string? category, string? price, string? condition, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            var productsQuery = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.User)
                .Where(p => p.IsActive == true && p.IsSold == false && (p.Status == "Bán" || p.Status == "Trao đổi"))
                .AsQueryable();

            // Filter Location
            if (!string.IsNullOrEmpty(location))
            {
                productsQuery = productsQuery.Where(p => p.Location == location);
            }

            // Filter Category
            if (!string.IsNullOrEmpty(category))
            {
                productsQuery = productsQuery.Where(p => p.Category != null && p.Category.CategoryName == category);
            }

            // Filter Condition
            if (!string.IsNullOrEmpty(condition))
            {
                productsQuery = productsQuery.Where(p => p.Condition != null &&
                    ((condition == "new" && p.Condition.ToLower() == "mới") ||
                     (condition == "used" && p.Condition.ToLower() == "cũ")));
            }

            // Filter Price
            if (!string.IsNullOrEmpty(price))
            {
                var parts = price.Split('-');
                if (parts.Length == 2 && decimal.TryParse(parts[0], out var minPrice) && decimal.TryParse(parts[1], out var maxPrice))
                {
                    productsQuery = productsQuery.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
                }
            }

            productsQuery = productsQuery.OrderBy(p => p.ProductId);

            var pagedProducts = new PagedList<Product>(productsQuery, pageNumber, pageSize);

            // Truyền filter sang View để giữ giá trị
            ViewBag.Location = location;
            ViewBag.Category = category;
            ViewBag.Condition = condition;
            ViewBag.Price = price;

            return View(pagedProducts);
        }


        public IActionResult DetailProduct(int id)
        {
            var product = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.User)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.Reviewer)
                .Include(p => p.Transactions)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // ✅ Lấy số lượt bán từ Product.SoldCount
            ViewBag.SoldCount = _context.Transactions
                .Count(t => t.ProductId == product.ProductId && t.ShippingStatus == "Received");


            // Nếu điểm uy tín hoặc trạng thái hoạt động bị null thì gán mặc định
            if (product.User != null)
            {
                product.User.ReputationScore ??= 0;
                product.User.IsActive ??= true;
            }

            return View(product);
        }

        [HttpPost]
        public IActionResult AddComment(int id, string Comment, int Rating)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để gửi đánh giá.";
                return RedirectToAction("Login", "Users");
            }

            var product = _context.Products.Include(p => p.User).FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();

            var review = new Review
            {
                ProductId = id,
                ReviewerId = userId,
                ReviewedUserId = product.UserId,
                Rating = Rating,
                Comment = Comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);

            // Cập nhật điểm đánh giá sản phẩm
            product.TotalRating = (product.TotalRating ?? 0) + Rating;
            product.RatingCount = (product.RatingCount ?? 0) + 1;

            // Gửi thông báo cho người bán
            var notification = new Notification
            {
                UserId = product.UserId,
                Title = "Đánh giá mới cho sản phẩm",
                Content = $"Sản phẩm '{product.Title}' vừa nhận được đánh giá {Rating}★ từ người mua.",
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);

            _context.SaveChanges();

            return RedirectToAction("DetailProduct", new { id });
        }


        [HttpPost]
        public IActionResult ReplyReview(int reviewId, string reply)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Users");

            var review = _context.Reviews.Include(r => r.Product).FirstOrDefault(r => r.ReviewId == reviewId);
            if (review == null) return NotFound();

            var product = review.Product;
            if (product == null || product.UserId != userId) return Forbid();

            review.SellerReply = reply;
            review.SellerReplyAt = DateTime.Now;

            // Gửi thông báo cho người mua
            var notification = new Notification
            {
                UserId = review.ReviewerId,
                Title = "Phản hồi từ người bán",
                Content = $"Người bán đã phản hồi đánh giá của bạn về sản phẩm '{product.Title}'.",
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);

            _context.SaveChanges();

            return RedirectToAction("DetailProduct", new { id = review.ProductId });
        }



        // GET: Products
        public async Task<IActionResult> Index()
        {
            var csdlDoAn1Context = _context.Products.Include(p => p.Category).Include(p => p.SubCategory).Include(p => p.User);
            return View(await csdlDoAn1Context.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId"); // ID người đang đăng nhập

            if (userId == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để đăng tin.";
                return RedirectToAction("Login", "Users");
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategories, "SubCategoryId", "SubCategoryName");

            // Không cần gán ViewData["UserId"] vì UserId sẽ lấy từ session
            return View();
        }


        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<IFormFile> Images)
        {
            if (ModelState.IsValid)
            {
                // Gán các giá trị mặc định
                product.CreatedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;
                product.Status = "Bán"; // hoặc "Chờ duyệt" nếu cần duyệt
                product.IsActive = true;
                product.IsSold = false;
                product.TotalRating = 0;
                product.RatingCount = 0;

                // Lấy UserId từ session
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Login", "Users");
                }
                product.UserId = userId.Value;

                // Lưu sản phẩm
                _context.Add(product);
                await _context.SaveChangesAsync();

                // Lưu ảnh sản phẩm
                foreach (var image in Images)
                {
                    if (image != null && image.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var productImage = new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImageUrl = "/images/" + fileName
                        };
                        _context.ProductImages.Add(productImage);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng sản phẩm thành công.";
                return RedirectToAction(nameof(Create));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategories, "SubCategoryId", "SubCategoryName", product.SubCategoryId);
            return View(product);
        }


        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["SubCategoryId"] = new SelectList(
                _context.SubCategories.Where(s => s.CategoryId == product.CategoryId),
                "SubCategoryId", "SubCategoryName", product.SubCategoryId
            );

            return View(product);
        }


        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile[]? NewImages, int[]? ImagesToDelete)
        {
            if (id != product.ProductId)
                return NotFound();

            var existing = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (existing == null)
                return NotFound();

            // Cập nhật các trường cho phép chỉnh sửa
            existing.Title = product.Title;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Condition = product.Condition;
            existing.Location = product.Location;
            existing.CategoryId = product.CategoryId;
            existing.SubCategoryId = product.SubCategoryId;
            existing.Brand = product.Brand;
            existing.WarrantyPeriod = product.WarrantyPeriod;
            existing.UsedDuration = product.UsedDuration;
            existing.IsSold = product.IsSold;
            existing.UpdatedAt = DateTime.Now;

            // Xóa ảnh được chọn
            if (ImagesToDelete != null && ImagesToDelete.Length > 0)
            {
                var imagesToRemove = existing.ProductImages
                    .Where(img => img != null && ImagesToDelete.Contains(img.ImageId))
                    .ToList();

                foreach (var img in imagesToRemove)
                {
                    if (!string.IsNullOrWhiteSpace(img.ImageUrl))
                    {
                        var filePath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            img.ImageUrl.TrimStart('/')
                        );

                        if (System.IO.File.Exists(filePath))
                            System.IO.File.Delete(filePath);
                    }

                    if (img != null)
                        _context.ProductImages.Remove(img);
                }
            }

            // Thêm ảnh mới
            if (NewImages != null && NewImages.Length > 0)
            {
                foreach (var file in NewImages)
                {
                    if (file?.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        existing.ProductImages.Add(new ProductImage
                        {
                            ProductId = existing.ProductId,
                            ImageUrl = "/images/" + fileName
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("MyProducts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var img = await _context.ProductImages.FirstOrDefaultAsync(p => p.ImageId == id);
            if (img == null)
                return Json(new { success = false, message = "Không tìm thấy ảnh." });

            if (!string.IsNullOrWhiteSpace(img.ImageUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.ProductImages.Remove(img);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        // Thêm Action hỗ trợ Ajax: GetSubCategoriesByCategory
        [HttpGet]
        public JsonResult GetSubCategoriesByCategory(int id)
        {
            var subCategories = _context.SubCategories
                .Where(s => s.CategoryId == id)
                .Select(s => new {
                    value = s.SubCategoryId,
                    text = s.SubCategoryName
                })
                .ToList();

            return Json(subCategories);
        }




        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.User)
                .Include(p => p.ProductImages) // Nếu có bảng ảnh
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages) // Nếu có ảnh
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product != null)
            {
                // Xóa ảnh sản phẩm (nếu có bảng ProductImages)
                if (product.ProductImages?.Any() == true)
                {
                    _context.ProductImages.RemoveRange(product.ProductImages);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
            }

            return RedirectToAction(nameof(MyProducts)); // hoặc "MyProducts" nếu bạn có trang quản lý riêng
        }

        [HttpGet]
        public JsonResult SearchSuggestions(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<string>());

            var suggestions = _context.Products
                .Where(p => (p.IsActive ?? false)
                    && !string.IsNullOrEmpty(p.Title)
                    && EF.Functions.Like(
                        EF.Functions.Collate(p.Title, "SQL_Latin1_General_CP1_CI_AI"),
                        $"%{term}%"))
                .Select(p => p.Title)
                .Distinct()
                .Take(5)
                .ToList();

            return Json(suggestions);
        }


        [HttpGet]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View("SearchResults", new List<Product>());

            string normalizedQuery = RemoveDiacritics(query).ToLower();

            // 1️⃣ Danh sách từ đồng nghĩa
            var synonyms = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "laptop", new[] { "máy tính", "máy tính xách tay", "notebook" } },
                { "lap top", new[] { "máy tính", "máy tính xách tay", "notebook" } },
                { "điện thoại", new[] { "smartphone", "mobile", "phone" } },
                { "tủ lạnh", new[] { "refrigerator", "fridge" } }
            };

            // 2️⃣ Tạo danh sách từ khóa tìm kiếm
            var searchTerms = new List<string> { normalizedQuery };
            if (synonyms.ContainsKey(normalizedQuery))
            {
                // Thêm các từ đồng nghĩa đã bỏ dấu và chuyển thường
                searchTerms.AddRange(synonyms[normalizedQuery]
                    .Select(s => RemoveDiacritics(s).ToLower()));
            }

            // 3️⃣ Lấy dữ liệu từ DB
            var products = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Where(p => p.IsActive ?? false)
                .ToList(); // Chạy truy vấn DB trước

            // 4️⃣ Lọc trong bộ nhớ với danh sách từ khóa
            var results = products.Where(p =>
                searchTerms.Any(term =>
                    (!string.IsNullOrEmpty(p.Title) &&
                     RemoveDiacritics(p.Title).ToLower().Contains(term))
                    || (p.Category != null &&
                        !string.IsNullOrEmpty(p.Category.CategoryName) &&
                        RemoveDiacritics(p.Category.CategoryName).ToLower().Contains(term))
                    || (p.SubCategory != null &&
                        !string.IsNullOrEmpty(p.SubCategory.SubCategoryName) &&
                        RemoveDiacritics(p.SubCategory.SubCategoryName).ToLower().Contains(term))
                )
            ).ToList();

            return View("SearchResults", results);
        }


        // Hàm bỏ dấu tiếng Việt
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var builder = new System.Text.StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }



        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
