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

        public AppointmentReminderService(IServiceProvider services, 
        ILogger<AppointmentReminderService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Appointment Reminder Service is starting.");

            // Chạy lần đầu sau 1 phút, sau đó lặp lại mỗi 24 giờ.
            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(1), TimeSpan.FromHours(24));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            _logger.LogInformation("Appointment Reminder Service is working.");
            using (var scope = _services.CreateScope())
            {
                var appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
                var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

                // Lấy các lịch hẹn hợp lệ để nhắc
                var upcomingAppointments = appointmentRepository.GetUpcomingAppointments(24).Result;

                foreach (var appointment in upcomingAppointments)
                {
                    _logger.LogInformation($"Found upcoming appointment for user {appointment.UserId}.");

                    var notification = NotificationFactory.CreateAppointmentReminderNotification(appointment);

                    // Lưu thông báo vào DB
                    notificationRepository.AddAsync(notification).Wait();

                    // Đẩy thông báo real-time đến client
                    hubContext.Clients.User(appointment.UserId.ToString())
                        .SendAsync("ReceiveNotification", notification.Message, notification.CreatedAt.ToString("o")).Wait();
                    // Đánh dấu là đã gửi thông báo và cập nhật vào CSDL
                    appointment.IsReminderSent = true;
                    appointmentRepository.UpdateAsync(appointment).Wait();
                }
            }
          _logger.LogInformation("Appointment Reminder Service has finished its work.");

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