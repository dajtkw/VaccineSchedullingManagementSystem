using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using QuanLyTiemChung.Web.Hubs;
using Microsoft.Extensions.Logging;
using QuanLyTiemChung.Web.Data;
using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.ViewModels;

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
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicalStaffController> _logger;


        public MedicalStaffController(
            IAppointmentRepository appointmentRepository,
            IVaccinationRecordRepository recordRepository,
            UserManager<User> userManager,
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> hubContext,
            ApplicationDbContext context,
            ILogger<MedicalStaffController> logger)
        {
            _appointmentRepository = appointmentRepository;
            _recordRepository = recordRepository;
            _userManager = userManager;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var appointments = await _appointmentRepository.GetViewModelsAsync();
            var today = DateTime.Today;

            // Tải dữ liệu cho các bộ lọc
            ViewBag.VaccinationSites = await _context.VaccinationSites.OrderBy(s => s.Name).ToListAsync();
            ViewBag.Vaccines = await _context.Vaccines.OrderBy(v => v.TradeName).ToListAsync();

            var viewModel = new MedicalStaffDashboardViewModel
            {
                AllAppointments = appointments.OrderByDescending(a => a.ScheduledDateTime),
                PendingCount = appointments.Count(a => a.Status == "Pending"),
                AppointmentsTodayCount = appointments.Count(a => a.ScheduledDateTime.Date == today && (a.Status == "Pending" || a.Status == "Confirmed"))
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = "Confirmed";
            await _appointmentRepository.UpdateAsync(appointment);

            var notification = NotificationFactory.CreateAppointmentConfirmedNotification(appointment);
            await _notificationRepository.AddAsync(notification);

            // Gửi thông báo toast
            await _hubContext.Clients.User(appointment.UserId.ToString())
                .SendAsync("ReceiveNotification", notification.Message, notification.CreatedAt.ToString("o"));

            // Gửi tín hiệu cập nhật UI
            await _hubContext.Clients.User(appointment.UserId.ToString())
                .SendAsync("UpdateAppointmentStatus", appointment.Id, "Confirmed", "badge bg-success");

            TempData["StatusMessage"] = "Đã xác nhận lịch hẹn thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Vaccine) // Include Vaccine for notification message
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                {
                    return NotFound();
                }

                var oldStatus = appointment.Status;
                appointment.Status = "Cancelled";
                _context.Update(appointment);

                // --- START OF INVENTORY REFUND LOGIC ---
                // Only refund stock if the appointment was previously confirmed or pending
                if (oldStatus == "Confirmed" || oldStatus == "Pending")
                {
                    var inventoryItem = await _context.SiteVaccineInventories
                        .FirstOrDefaultAsync(i => i.VaccinationSiteId == appointment.VaccinationSiteId && i.VaccineId == appointment.VaccineId);

                    if (inventoryItem != null)
                    {
                        // Increment the quantity back by 1
                        inventoryItem.Quantity++;
                        _context.Update(inventoryItem);
                    }
                    else
                    {
                        // Optional: Log a warning if the inventory item was not found
                        _logger.LogWarning("Could not find inventory item to refund for cancelled Appointment ID {AppointmentId}", id);
                    }
                }
                // --- END OF INVENTORY REFUND LOGIC ---

                // Save changes for both appointment and inventory
                await _context.SaveChangesAsync();

                // Send notification to user
                var notification = NotificationFactory.CreateAppointmentCancelledNotification(appointment);
                await _notificationRepository.AddAsync(notification);

                // Send real-time signals
                await _hubContext.Clients.User(appointment.UserId.ToString())
                    .SendAsync("ReceiveNotification", notification.Message, notification.CreatedAt.ToString("o"));
                await _hubContext.Clients.User(appointment.UserId.ToString())
                    .SendAsync("UpdateAppointmentStatus", appointment.Id, "Cancelled", "badge bg-danger");

                await transaction.CommitAsync();

                TempData["StatusMessage"] = "Đã hủy lịch hẹn.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling appointment for ID {AppointmentId}", id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi trong quá trình hủy lịch hẹn.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteVaccination(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null) return NotFound();

            var medicalStaff = await _userManager.GetUserAsync(User);
            if (medicalStaff == null) return Challenge();

            appointment.Status = "Completed";
            await _appointmentRepository.UpdateAsync(appointment);

            var record = new VaccinationRecord
            {
                AppointmentId = appointment.Id,
                HealthOfficialId = medicalStaff.Id,
                ActualVaccinationTime = System.DateTime.Now,
                NotesAfterShot = $"Hoàn thành qua lịch hẹn #{appointment.Id}"
            };
            await _recordRepository.AddAsync(record);

            var notification = NotificationFactory.CreateVaccinationCompletedNotification(appointment, record);

            await _notificationRepository.AddAsync(notification);

            // Gửi thông báo toast
            await _hubContext.Clients.User(appointment.UserId.ToString())
                .SendAsync("ReceiveNotification", notification.Message, notification.CreatedAt.ToString("o"));

            // CẬP NHẬT: Thêm tín hiệu cập nhật UI cho hành động Hoàn thành
            await _hubContext.Clients.User(appointment.UserId.ToString())
                .SendAsync("UpdateAppointmentStatus", appointment.Id, "Completed", "badge bg-primary");

            TempData["StatusMessage"] = "Đã ghi nhận tiêm chủng thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointmentDetails(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Tái sử dụng partial view chi tiết của trang Admin để đảm bảo giao diện nhất quán
            return PartialView("~/Views/Admin/ManageAppointments/_AppointmentDetailsModal.cshtml", appointment);
        }
    }
}