using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyTiemChung.Web.Data;
using QuanLyTiemChung.Web.Hubs;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;
using QuanLyTiemChung.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTiemChung.Web.Controllers
{
    [Authorize(Roles = "Citizen")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IVaccinationSiteRepository _siteRepository;
        private readonly UserManager<User> _userManager;
        private readonly IVaccinationRecordRepository _recordRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentController> _logger;
        private readonly IVaccineCategoryRepository _categoryRepository;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationRepository _notificationRepository;
        public AppointmentController(
            IAppointmentRepository appointmentRepository,
            IVaccinationSiteRepository siteRepository,
            UserManager<User> userManager,
            IVaccinationRecordRepository recordRepository,
            ApplicationDbContext context,
            ILogger<AppointmentController> logger,
            IVaccineCategoryRepository categoryRepository,
            IHubContext<NotificationHub> hubContext,
            INotificationRepository notificationRepository)
        {
            _appointmentRepository = appointmentRepository;
            _siteRepository = siteRepository;
            _userManager = userManager;
            _recordRepository = recordRepository;
            _hubContext = hubContext;
            _context = context;
            _logger = logger;
            _categoryRepository = categoryRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var appointments = await _appointmentRepository.GetViewModelsByUserIdAsync(user.Id);
            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointmentsByStatus(string status = "all")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var appointments = await _appointmentRepository.GetViewModelsByUserIdAsync(user.Id);

            IEnumerable<AppointmentViewModel> filteredAppointments;

            switch (status.ToLower())
            {
                case "upcoming":
                    filteredAppointments = appointments.Where(a => a.Status == "Pending" || a.Status == "Confirmed");
                    break;
                case "history":
                    filteredAppointments = appointments.Where(a => a.Status == "Completed" || a.Status == "Cancelled");
                    break;
                default:
                    filteredAppointments = appointments;
                    break;
            }
            return PartialView("_AppointmentListPartial", filteredAppointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện." });

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

                if (appointment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch hẹn hoặc bạn không có quyền hủy lịch này." });
                }

                var oldStatus = appointment.Status;
                if (oldStatus != "Pending" && oldStatus != "Confirmed")
                {
                    return Json(new { success = false, message = "Bạn chỉ có thể hủy các lịch hẹn đang chờ hoặc đã xác nhận." });
                }

                appointment.Status = "Cancelled";
                _context.Update(appointment);

                var inventoryItem = await _context.SiteVaccineInventories
                    .FirstOrDefaultAsync(i => i.VaccinationSiteId == appointment.VaccinationSiteId && i.VaccineId == appointment.VaccineId);

                if (inventoryItem != null)
                {
                    inventoryItem.Quantity++;
                    _context.Update(inventoryItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Đã hủy lịch hẹn thành công." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi người dùng hủy lịch hẹn {AppointmentId}", id);
                return Json(new { success = false, message = "Đã xảy ra lỗi, vui lòng thử lại." });
            }
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateAppointmentViewModel
            {
                ScheduledDateTime = DateTime.Now.Date.AddDays(1).AddHours(8)
            };
            await PrepareCreateModelAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (model.DoseNumber > 1)
            {
                // 1. Tìm mũi tiêm hoàn thành gần nhất
                var lastAppointment = await _appointmentRepository.GetLastCompletedAppointmentAsync(user.Id, model.SelectedVaccineId);

                if (lastAppointment == null || lastAppointment.VaccinationRecord == null)
                {
                    // Nếu người dùng cố tình đăng ký mũi 2 mà chưa có mũi 1, báo lỗi.
                    ModelState.AddModelError("", $"Bạn cần hoàn thành mũi {model.DoseNumber - 1} trước khi đăng ký mũi này.");
                }
                else
                {
                    // 2. Tìm thông tin về khoảng cách yêu cầu giữa các liều
                    var vaccineInfo = await _context.Vaccines
                        .Include(v => v.Doses)
                        .FirstOrDefaultAsync(v => v.Id == model.SelectedVaccineId);

                    // Tìm khoảng cách yêu cầu sau mũi tiêm trước đó (lastAppointment.DoseNumber)
                    var requiredDoseInfo = vaccineInfo?.Doses.FirstOrDefault(d => d.DoseNumber == lastAppointment.DoseNumber);

                    if (requiredDoseInfo?.IntervalInMonths.HasValue == true)
                    {
                        // 3. Tính ngày đủ điều kiện
                        var lastVaccinationDate = lastAppointment.VaccinationRecord.ActualVaccinationTime;
                        var nextEligibleDate = lastVaccinationDate.AddMonths(requiredDoseInfo.IntervalInMonths.Value);

                        if (DateTime.Now < nextEligibleDate)
                        {
                            ModelState.AddModelError("", $"Bạn chưa đủ điều kiện đăng ký mũi này. Vui lòng quay lại sau ngày {nextEligibleDate:dd/MM/yyyy}.");
                        }
                    }
                }
            }
            if (!ModelState.IsValid)
            {
                await PrepareCreateModelAsync(model);
                return View(model);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var inventoryItem = await _context.SiteVaccineInventories
                    .FirstOrDefaultAsync(i => i.VaccinationSiteId == model.SelectedSiteId && i.VaccineId == model.SelectedVaccineId);

                if (inventoryItem == null || inventoryItem.Quantity < 1)
                {
                    ModelState.AddModelError("", "Vắc-xin bạn chọn đã hết tại điểm tiêm này. Vui lòng tải lại trang và chọn lại.");
                }

                if (ModelState.IsValid)
                {
                    inventoryItem.Quantity--;
                    _context.Update(inventoryItem);

                    var appointment = new Appointment
                    {
                        UserId = user.Id,
                        VaccineId = model.SelectedVaccineId,
                        VaccinationSiteId = model.SelectedSiteId,
                        ScheduledDateTime = model.ScheduledDateTime,
                        DoseNumber = model.DoseNumber,
                        Notes = model.Notes,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Appointments.Add(appointment);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    try
                    {
                        var createdAppointment = await _context.Appointments
                            .Include(a => a.User)
                            .Include(a => a.Vaccine)
                            .Include(a => a.VaccinationSite)
                            .FirstOrDefaultAsync(a => a.Id == appointment.Id);

                        if (createdAppointment != null)
                        {
                            var medicalStaffs = await _userManager.GetUsersInRoleAsync("HealthOfficial");
                            var tasks = new List<Task>();

                            // Chuẩn bị ViewModel để gửi đi cho việc cập nhật bảng
                            var newAppointmentViewModel = new AppointmentViewModel
                            {
                                Id = createdAppointment.Id,
                                UserFullName = createdAppointment.User.FullName,
                                VaccineName = createdAppointment.Vaccine.TradeName,
                                SiteName = createdAppointment.VaccinationSite.Name,
                                SiteAddress = createdAppointment.VaccinationSite.Address,
                                ScheduledDateTime = createdAppointment.ScheduledDateTime,
                                DoseNumber = createdAppointment.DoseNumber,
                                Status = createdAppointment.Status,
                                StatusBadgeClass = "badge bg-warning text-dark"
                            };

                            foreach (var staff in medicalStaffs)
                            {
                                // 1. Tạo và lưu thông báo vào CSDL (cho chuông và danh sách)
                                var notification = NotificationFactory.CreateNewAppointmentForStaffNotification(staff, createdAppointment.User, createdAppointment.Vaccine);
                                tasks.Add(_notificationRepository.AddAsync(notification));

                                // 2. Gửi tín hiệu ĐƠN GIẢN để cập nhật chuông và toast
                                tasks.Add(_hubContext.Clients.User(staff.Id.ToString())
                                    .SendAsync("ReceiveNotification", notification.Message, notification.CreatedAt.ToString("o")));

                                // 3. Gửi tín hiệu CHI TIẾT để cập nhật bảng real-time
                                tasks.Add(_hubContext.Clients.User(staff.Id.ToString())
                                    .SendAsync("ReceiveNewAppointment", newAppointmentViewModel));
                            }

                            await Task.WhenAll(tasks);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send SignalR notification for new appointment.");
                    }


                    TempData["StatusMessage"] = "Đăng ký lịch tiêm thành công! Vui lòng chờ xác nhận từ cán bộ y tế.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating appointment for User {UserId}", user.Id);
                ModelState.AddModelError("", "Đã xảy ra lỗi không mong muốn trong quá trình đặt lịch. Vui lòng thử lại.");
            }

            await PrepareCreateModelAsync(model);
            return View(model);
        }

        [HttpGet]
        public async Task<JsonResult> GetDistricts(string provinceName)
        {
            if (string.IsNullOrEmpty(provinceName)) return Json(new List<string>());
            var sites = await _siteRepository.GetAllAsync();
            var districts = sites
                .Where(s => s.Address.EndsWith(provinceName))
                .Select(s => s.Address.Split(','))
                .Where(parts => parts.Length >= 3)
                .Select(parts => parts[^2].Trim())
                .Distinct().OrderBy(d => d).ToList();
            return Json(districts);
        }

        [HttpGet]
        public async Task<JsonResult> GetWards(string provinceName, string districtName)
        {
            if (string.IsNullOrEmpty(provinceName) || string.IsNullOrEmpty(districtName)) return Json(new List<string>());
            var sites = await _siteRepository.GetAllAsync();
            var wards = sites
                .Where(s => s.Address.Contains($", {districtName}, {provinceName}"))
                .Select(s => s.Address.Split(','))
                .Where(parts => parts.Length >= 4)
                .Select(parts => parts[^3].Trim())
                .Distinct().OrderBy(w => w).ToList();
            return Json(wards);
        }

        [HttpGet]
        public async Task<JsonResult> GetSites(string provinceName, string districtName, string wardName)
        {
            IEnumerable<VaccinationSite> sites = await _siteRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(provinceName))
            {
                sites = sites.Where(s => s.Address.Contains(provinceName));
            }
            if (!string.IsNullOrEmpty(districtName))
            {
                sites = sites.Where(s => s.Address.Contains(districtName));
            }
            if (!string.IsNullOrEmpty(wardName))
            {
                sites = sites.Where(s => s.Address.Contains(wardName));
            }

            var filteredSites = sites
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.Name} - {s.Address}"
                })
                .OrderBy(s => s.Text).ToList();

            return Json(filteredSites);
        }

        [HttpGet]
        public async Task<JsonResult> GetProvinces()
        {
            var sites = await _siteRepository.GetAllAsync();
            var provinces = sites
                .Select(s => s.Address.Split(',').LastOrDefault()?.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct()
                .OrderBy(p => p)
                .Select(p => new SelectListItem { Value = p, Text = p })
                .ToList();
            return Json(provinces);
        }


        [HttpGet]
        public async Task<JsonResult> GetAvailableVaccines(int siteId, int? categoryId)
        {
            if (siteId <= 0) return Json(new List<object>());

            var query = _context.SiteVaccineInventories
                .AsNoTracking()
                .Include(i => i.Vaccine)
                .ThenInclude(v => v.Doses)
                .Where(i => i.VaccinationSiteId == siteId && i.Quantity > 0);

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(i => i.Vaccine.VaccineCategoryId == categoryId.Value);
            }

            var vaccinesInStock = await query
                .OrderBy(i => i.Vaccine.GenericName)
                .ToListAsync();

            // Phần code nhóm dữ liệu phía dưới giữ nguyên
            var groupedVaccines = vaccinesInStock
                .GroupBy(i => i.Vaccine.GenericName)
                .Select(group => new
                {
                    GroupName = group.Key,
                    Vaccines = group.Select(item => new
                    {
                        Value = item.VaccineId,
                        Text = $"{item.Vaccine.TradeName} (còn {item.Quantity} liều)",
                        IsScheduledDose = item.Vaccine.Doses.Count > 1 && item.Vaccine.Doses.Any(d => d.IntervalInMonths.HasValue)

                    })
                });

            return Json(groupedVaccines);
        }

        public async Task<IActionResult> VaccinationCertificate(int appointmentId)
        {
            var record = await _recordRepository.GetByAppointmentIdAsync(appointmentId);

            if (record == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            if (record.Appointment.UserId != currentUser.Id) return Forbid();

            return View(record);
        }

        private async Task PrepareCreateModelAsync(CreateAppointmentViewModel model)
        {
            // The province list is now loaded via AJAX by the new GetProvinces() endpoint,
            // so we don't need to load it here anymore.
            model.AvailableProvinces = new List<SelectListItem>();
            model.AvailableVaccines = new List<Vaccine>();
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoriesJson()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var result = categories.Select(c => new { id = c.Id, name = c.Name });
            return Json(result);
        }

    }
}