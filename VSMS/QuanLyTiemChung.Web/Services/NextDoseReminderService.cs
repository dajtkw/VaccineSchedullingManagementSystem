using Microsoft.AspNetCore.SignalR;
using QuanLyTiemChung.Web.Data;
using QuanLyTiemChung.Web.Hubs;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyTiemChung.Web.Services
{
    public class NextDoseReminderService : IHostedService, IDisposable
    {
        private readonly ILogger<NextDoseReminderService> _logger;
        private readonly IServiceProvider _services;
        private Timer? _timer = null;

        public NextDoseReminderService(IServiceProvider services, ILogger<NextDoseReminderService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Next Dose Reminder Service is starting.");
            // Chạy mỗi 24 giờ
            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(2), TimeSpan.FromHours(24));
            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            _logger.LogInformation("Next Dose Reminder Service is working.");
            using (var scope = _services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

                // Lấy tất cả các vắc-xin có nhiều hơn 1 liều và có định nghĩa khoảng cách
                var multiDoseVaccines = context.Vaccines
                    .Include(v => v.Doses)
                    .Where(v => v.Doses.Count > 1 && v.Doses.Any(d => d.IntervalInMonths.HasValue))
                    .ToList();

                foreach (var vaccine in multiDoseVaccines)
                {
                    // Tìm tất cả các lịch hẹn đã hoàn thành cho vắc-xin này
                    var completedAppointments = context.Appointments
                        .Include(a => a.User)
                        .Include(a => a.VaccinationRecord)
                        .Where(a => a.VaccineId == vaccine.Id && a.Status == "Completed")
                        .ToList();

                    // Nhóm các lịch hẹn theo người dùng
                    var appointmentsByUser = completedAppointments.GroupBy(a => a.UserId);

                    foreach (var userGroup in appointmentsByUser)
                    {
                        var userId = userGroup.Key;
                        var user = userGroup.First().User;

                        // Tìm mũi tiêm hoàn thành gần nhất của người dùng này
                        var lastCompleted = userGroup.OrderByDescending(a => a.DoseNumber).First();
                        var nextDoseNumber = lastCompleted.DoseNumber + 1;

                        // Kiểm tra xem người dùng đã tiêm mũi tiếp theo chưa
                        bool hasNextDose = context.Appointments.Any(a => a.UserId == userId && a.VaccineId == vaccine.Id && a.DoseNumber == nextDoseNumber);
                        if (hasNextDose) continue; // Bỏ qua nếu đã có lịch hẹn cho mũi tiếp theo

                        var doseInfo = vaccine.Doses.FirstOrDefault(d => d.DoseNumber == lastCompleted.DoseNumber);
                        if (doseInfo?.IntervalInMonths.HasValue != true) continue;

                        var nextEligibleDate = lastCompleted.VaccinationRecord!.ActualVaccinationTime.AddMonths(doseInfo.IntervalInMonths.Value);

                        // Chỉ gửi thông báo nếu hôm nay là ngày đủ điều kiện
                        if (nextEligibleDate.Date == DateTime.Today)
                        {
                            _logger.LogInformation($"User {userId} is now eligible for dose {nextDoseNumber} of vaccine {vaccine.Id}. Sending notification.");

                            var notification = NotificationFactory.CreateNextDoseAvailableNotification(user, vaccine, nextDoseNumber);

                            notificationRepo.AddAsync(notification).Wait();
                            hubContext.Clients.User(user.Id.ToString())
                                .SendAsync("ReceiveNotification", notification.Message, notification.CreatedAt.ToString("o")).Wait();
                        }
                    }
                }
            }
            _logger.LogInformation("Next Dose Reminder Service has finished its work.");
        }

        // ... (StopAsync và Dispose giữ nguyên như trong AppointmentReminderService)
        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Next Dose Reminder Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}