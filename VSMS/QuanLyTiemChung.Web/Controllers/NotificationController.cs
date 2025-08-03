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

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Lấy tất cả thông báo
            var notifications = await _notificationRepository.GetByUserIdAsync(user.Id);

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
                id = n.Id, // <-- Trả về Id
                message = n.Message,
                isRead = n.IsRead,
                createdAt = n.CreatedAt.ToString("o"), // Định dạng ISO 8601
                url = n.Url // <-- Trả về Url
            });

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // TODO: Bạn có thể thêm bước kiểm tra xem thông báo này có thực sự thuộc về người dùng đang đăng nhập không
            await _notificationRepository.MarkAsReadAsync(id);
            return Ok();
        }
    }
}