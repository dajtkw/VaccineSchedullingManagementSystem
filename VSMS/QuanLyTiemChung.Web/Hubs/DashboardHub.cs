// File: Hubs/DashboardHub.cs
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Tasks;

namespace QuanLyTiemChung.Web.Hubs
{
    public class DashboardHub : Hub
    {
        // Sử dụng một biến static để đếm số kết nối trên toàn server
        private static int _onlineUsers = 0;

        // Phương thức này được gọi mỗi khi có một client mới kết nối
        public override Task OnConnectedAsync()
        {
            // Tăng biến đếm lên một cách an toàn (thread-safe)
            Interlocked.Increment(ref _onlineUsers);
            return base.OnConnectedAsync();
        }

        // Phương thức này được gọi mỗi khi có một client ngắt kết nối
        public override Task OnDisconnectedAsync(System.Exception? exception)
        {
            // Giảm biến đếm xuống một cách an toàn
            Interlocked.Decrement(ref _onlineUsers);
            return base.OnDisconnectedAsync(exception);
        }

        // Một phương thức để các dịch vụ khác có thể lấy số lượng người dùng
        public static int GetOnlineUserCount()
        {
            return _onlineUsers;
        }
    }
}
