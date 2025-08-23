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
    public class MessagesController : Controller
    {
        private readonly CsdlDoAn1Context _context;

        public async Task<IActionResult> Inbox()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return RedirectToAction("Login", "Users");

            var allMessages = await _context.Messages
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            // Lấy tin nhắn cuối cùng mỗi cuộc trò chuyện
            var lastMessages = allMessages
                .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Select(g => g.OrderByDescending(m => m.SentAt).First())
                .ToList();

            // Đếm số tin nhắn chưa đọc theo từng người gửi
            var unreadCounts = allMessages
                .Where(m => m.ReceiverId == currentUserId && m.IsRead == false)
                .GroupBy(m => m.SenderId)
                .ToDictionary(g => g.Key ?? 0, g => g.Count());

            ViewBag.UnreadCounts = unreadCounts;

            // Tổng số tin nhắn chưa đọc (cho icon tin nhắn trên navbar)
            ViewBag.TotalUnreadCount = unreadCounts.Values.Sum();

            return View(lastMessages);
        }



        public async Task<IActionResult> Chat(int userId)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return RedirectToAction("Login", "Users");

            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId)
                         || (m.SenderId == userId && m.ReceiverId == currentUserId))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            foreach (var msg in messages.Where(m => m.ReceiverId == currentUserId && m.IsRead != true))
            {
                msg.IsRead = true;
            }
            await _context.SaveChangesAsync();

            ViewBag.OtherUser = await _context.Users.FindAsync(userId);
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> Send(int receiverId, string content)
        {
            var senderId = HttpContext.Session.GetInt32("UserId");
            if (senderId == null) return RedirectToAction("Login", "Users");

            var message = new Message
            {
                SenderId = senderId.Value,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.Now,
                MessageType = "user"
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Chat", new { userId = receiverId });
        }

        public async Task<IActionResult> Recall(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var msg = await _context.Messages.FindAsync(id);

            if (msg == null || msg.SenderId != userId) return NotFound();

            msg.Content = "[Tin nhắn đã được thu hồi]";
            await _context.SaveChangesAsync();

            return RedirectToAction("Chat", new { userId = msg.ReceiverId });
        }

        public async Task<IActionResult> DeleteMessage(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var msg = await _context.Messages.FindAsync(id);

            if (msg == null) return NotFound();

            if (msg.SenderId == userId)
                msg.IsDeletedBySender = true;
            else if (msg.ReceiverId == userId)
                msg.IsDeletedByReceiver = true;

            await _context.SaveChangesAsync();
            return RedirectToAction("Chat", new { userId = (msg.SenderId == userId ? msg.ReceiverId : msg.SenderId) });
        }


        public MessagesController(CsdlDoAn1Context context)
        {
            _context = context;
        }

        // GET: Messages
        public async Task<IActionResult> Index()
        {
            var csdlDoAn1Context = _context.Messages.Include(m => m.Receiver).Include(m => m.Sender);
            return View(await csdlDoAn1Context.ToListAsync());
        }

        // GET: Messages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.Receiver)
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.MessageId == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // GET: Messages/Create
        public IActionResult Create()
        {
            ViewData["ReceiverId"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["SenderId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Messages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MessageId,SenderId,ReceiverId,Content,SentAt,MessageType,IsRead,IsDeletedBySender,IsDeletedByReceiver")] Message message)
        {
            if (ModelState.IsValid)
            {
                _context.Add(message);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReceiverId"] = new SelectList(_context.Users, "UserId", "UserId", message.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "UserId", "UserId", message.SenderId);
            return View(message);
        }

        // GET: Messages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            ViewData["ReceiverId"] = new SelectList(_context.Users, "UserId", "UserId", message.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "UserId", "UserId", message.SenderId);
            return View(message);
        }

        // POST: Messages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MessageId,SenderId,ReceiverId,Content,SentAt,MessageType,IsRead,IsDeletedBySender,IsDeletedByReceiver")] Message message)
        {
            if (id != message.MessageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(message);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MessageExists(message.MessageId))
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
            ViewData["ReceiverId"] = new SelectList(_context.Users, "UserId", "UserId", message.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "UserId", "UserId", message.SenderId);
            return View(message);
        }

        // GET: Messages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.Receiver)
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.MessageId == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                _context.Messages.Remove(message);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.MessageId == id);
        }
    }
}
