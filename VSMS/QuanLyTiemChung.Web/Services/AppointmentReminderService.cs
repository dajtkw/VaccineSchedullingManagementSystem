using Microsoft.AspNetCore.SignalR;
using QuanLyTiemChung.Web.Hubs;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuanLyTiemChung.Web.Services
{
    public class AppointmentReminderService : IHostedService, IDisposable
    {
        private readonly ILogger<AppointmentReminderService> _logger;
        private readonly IServiceProvider _services;
        private Timer? _timer = null;

        public AppointmentReminderService(IServiceProvider services, ILogger<AppointmentReminderService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Appointment Reminder Service is starting.");

            // Hẹn giờ để chạy công việc mỗi giờ một lần
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            _logger.LogInformation("Appointment Reminder Service is working.");

            // Vì Hosted Service là Singleton, chúng ta cần tạo một "scope" mới
            // để lấy các dịch vụ có vòng đời "scoped" như DbContext và repositories.
            using (var scope = _services.CreateScope())
            {
                var appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
                var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

                // Lấy các lịch hẹn sẽ diễn ra trong vòng 24 giờ tới và chưa được nhắc
                // Giả sử chúng ta chỉ nhắc 1 lần
                var upcomingAppointments = appointmentRepository.GetUpcomingAppointments(24).Result;

                foreach (var appointment in upcomingAppointments)
                {
                    _logger.LogInformation($"Found upcoming appointment for user {appointment.UserId}.");

                    var notification = new Notification
                    {
                        UserId = appointment.UserId,
                        Message = $"Nhắc lịch: Bạn có lịch hẹn tiêm vắc-xin '{appointment.Vaccine.TradeName}' vào lúc {appointment.ScheduledDateTime:HH:mm dd/MM/yyyy}.",
                        NotificationType = "AppointmentReminder"
                    };

                    // Lưu thông báo vào DB
                    notificationRepository.AddAsync(notification).Wait();
                    
                    // Đẩy thông báo real-time đến client
                    hubContext.Clients.User(appointment.UserId.ToString())
                        .SendAsync("ReceiveNotification", notification.Message, notification.CreatedAt.ToString("o")).Wait();
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Appointment Reminder Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}