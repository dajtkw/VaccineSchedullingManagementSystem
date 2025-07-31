using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTiemChung.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly UserManager<User> _userManager;

        public NotificationController(INotificationRepository notificationRepository, UserManager<User> userManager)
        {
            _notificationRepository = notificationRepository;
            _userManager = userManager;
        }

        // Action cho trang chính
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Lấy tất cả thông báo
            var notifications = await _notificationRepository.GetByUserIdAsync(user.Id);

            // Đánh dấu tất cả là đã đọc khi người dùng vào xem trang
            await _notificationRepository.MarkAllAsReadAsync(user.Id);

            return View(notifications);
        }

        // Action này chỉ dùng để lấy số lượng thông báo chưa đọc cho biểu tượng chuông
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notifications = await _notificationRepository.GetByUserIdAsync(user.Id);
            int unreadCount = notifications.Count(n => !n.IsRead);
            
            return Json(new { count = unreadCount });
        }


        // Action này sẽ được gọi bằng JavaScript để lấy thông báo
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notifications = await _notificationRepository.GetByUserIdAsync(user.Id);
            
            var result = notifications.Select(n => new 
            {
                message = n.Message,
                isRead = n.IsRead,
                createdAt = n.CreatedAt.ToString("o") // Định dạng ISO 8601
            });

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _notificationRepository.MarkAllAsReadAsync(user.Id);
            return Ok();
        }
    }
}