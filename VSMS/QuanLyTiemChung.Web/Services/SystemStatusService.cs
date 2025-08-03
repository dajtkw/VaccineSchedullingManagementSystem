// File: Services/SystemStatusService.cs
using Microsoft.AspNetCore.SignalR;
using QuanLyTiemChung.Web.Hubs;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuanLyTiemChung.Web.Services
{
    public class SystemStatusService : BackgroundService
    {
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly Random _random = new Random();
        private readonly ILogger<SystemStatusService> _logger;

        public SystemStatusService(IHubContext<DashboardHub> hubContext, ILogger<SystemStatusService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Giữ lại việc mô phỏng CPU và RAM
                var cpuLoad = _random.Next(10, 70);
                var memoryUsage = _random.Next(40, 85);

                // 🔥 THAY ĐỔI: Lấy số người dùng online THỰC TẾ từ Hub
                var onlineUsers = DashboardHub.GetOnlineUserCount();

                var systemStatus = new
                {
                    CpuLoad = cpuLoad,
                    MemoryUsage = memoryUsage,
                    OnlineUsers = onlineUsers
                };

                await _hubContext.Clients.All.SendAsync("ReceiveSystemStatus", systemStatus, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}
