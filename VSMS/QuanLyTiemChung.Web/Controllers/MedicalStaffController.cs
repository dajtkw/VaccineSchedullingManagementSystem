using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;
using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using QuanLyTiemChung.Web.Hubs;

namespace QuanLyTiemChung.Web.Controllers
{
    [Authorize(Roles = "HealthOfficial")]
    public class MedicalStaffController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IVaccinationRecordRepository _recordRepository;
        private readonly UserManager<User> _userManager;
        private readonly INotificationRepository _notificationRepository; 
        private readonly IHubContext<NotificationHub> _hubContext;

        public MedicalStaffController(
            IAppointmentRepository appointmentRepository,
            IVaccinationRecordRepository recordRepository,
            UserManager<User> userManager,
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> hubContext
            )
        {
            _appointmentRepository = appointmentRepository;
            _recordRepository = recordRepository;
            _userManager = userManager;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        // GET: /MedicalStaff
        // Dashboard cho cán bộ y tế, hiển thị các lịch hẹn cần xử lý.
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả các lịch hẹn để hiển thị trên dashboard
            var appointments = await _appointmentRepository.GetViewModelsAsync();
            return View(appointments);
        }

        // POST: /MedicalStaff/ConfirmAppointment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Confirmed";
            await _appointmentRepository.UpdateAsync(appointment);


            var notification = new Notification
            {
                UserId = appointment.UserId,
                Message = $"Lịch hẹn tiêm vắc-xin '{appointment.Vaccine.TradeName}' của bạn vào lúc {appointment.ScheduledDateTime:HH:mm dd/MM/yyyy} đã được xác nhận.",
                NotificationType = "AppointmentConfirmed"
            };
            await _notificationRepository.AddAsync(notification);
            await _hubContext.Clients.User(appointment.UserId.ToString())
            .SendAsync("ReceiveNotification", notification.Message, notification.CreatedAt.ToString("o"));

            TempData["StatusMessage"] = "Đã xác nhận lịch hẹn thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /MedicalStaff/CancelAppointment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var notification = new Notification
            {
                UserId = appointment.UserId,
                Message = $"Rất tiếc, lịch hẹn tiêm vắc-xin '{appointment.Vaccine.TradeName}' của bạn vào lúc {appointment.ScheduledDateTime:HH:mm dd/MM/yyyy} đã bị hủy.",
                NotificationType = "AppointmentCancelled"
            };
            await _notificationRepository.AddAsync(notification);
            appointment.Status = "Cancelled";
            await _appointmentRepository.UpdateAsync(appointment);

            await _hubContext.Clients.User(appointment.UserId.ToString())
                .SendAsync("ReceiveNotification", notification.Message, notification.CreatedAt.ToString("o"));

            TempData["StatusMessage"] = "Đã hủy lịch hẹn.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /MedicalStaff/CompleteVaccination/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteVaccination(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var medicalStaff = await _userManager.GetUserAsync(User);
            if (medicalStaff == null)
            {
                return Challenge();
            }

            appointment.Status = "Completed";
            await _appointmentRepository.UpdateAsync(appointment);

            var record = new VaccinationRecord
            {
                AppointmentId = appointment.Id,
                HealthOfficialId = medicalStaff.Id,
                ActualVaccinationTime = DateTime.Now, 
                NotesAfterShot = $"Hoàn thành qua lịch hẹn #{appointment.Id}"
            };
            await _recordRepository.AddAsync(record);

            var notification = new Notification
            {
                UserId = appointment.UserId,
                Message = $"Bạn đã hoàn thành tiêm mũi {appointment.DoseNumber} vắc-xin '{appointment.Vaccine.TradeName}' vào lúc {record.ActualVaccinationTime:HH:mm dd/MM/yyyy}.",
                NotificationType = "VaccinationCompleted"
            };
            await _notificationRepository.AddAsync(notification);

            await _hubContext.Clients.User(appointment.UserId.ToString())
                    .SendAsync("ReceiveNotification", notification.Message, notification.CreatedAt.ToString("o"));

            TempData["StatusMessage"] = "Đã ghi nhận tiêm chủng thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}