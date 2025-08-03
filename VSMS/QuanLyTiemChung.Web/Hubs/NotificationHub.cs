using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using QuanLyTiemChung.Web.Models;
using System.Threading.Tasks;

namespace QuanLyTiemChung.Web.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly UserManager<User> _userManager;

        // Sử dụng Dependency Injection để lấy UserManager
        public NotificationHub(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // Phương thức này sẽ tự động được gọi khi một người dùng kết nối đến hub
        public override async Task OnConnectedAsync()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (Context.User?.Identity?.IsAuthenticated ?? false)
            {
                // Lấy thông tin người dùng đang kết nối
                var user = await _userManager.GetUserAsync(Context.User);
                if (user != null)
                {
                    // Nếu người dùng có vai trò là "HealthOfficial"
                    if (await _userManager.IsInRoleAsync(user, "HealthOfficial"))
                    {
                        // Thêm họ vào một nhóm tên là "MedicalStaffs"
                        await Groups.AddToGroupAsync(Context.ConnectionId, "MedicalStaffs");
                    }
                }
            }
            await base.OnConnectedAsync();
        }
    }
}