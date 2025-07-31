using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly IVaccineRepository _vaccineRepository;
        private readonly IVaccinationSiteRepository _siteRepository;
        private readonly UserManager<User> _userManager;
        private readonly IVaccinationRecordRepository _recordRepository;

        public AppointmentController(
            IAppointmentRepository appointmentRepository,
            IVaccineRepository vaccineRepository,
            IVaccinationSiteRepository siteRepository,
            UserManager<User> userManager,
            IVaccinationRecordRepository recordRepository)
        {
            _appointmentRepository = appointmentRepository;
            _vaccineRepository = vaccineRepository;
            _siteRepository = siteRepository;
            _userManager = userManager;
            _recordRepository = recordRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var appointments = await _appointmentRepository.GetViewModelsByUserIdAsync(user.Id);
            return View(appointments);
        }

        public async Task<IActionResult> Create()
        {
            var allSites = await _siteRepository.GetAllAsync();
            var availableProvinces = allSites
                .Select(s => s.Address.Split(',').LastOrDefault()?.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct()
                .OrderBy(p => p)
                .Select(p => new SelectListItem { Value = p, Text = p })
                .ToList();

            var viewModel = new CreateAppointmentViewModel
            {
                AvailableVaccines = await _vaccineRepository.GetAllAsync(),
                AvailableProvinces = availableProvinces,
                ScheduledDateTime = DateTime.Now.Date.AddDays(1).AddHours(8)
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();

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

                await _appointmentRepository.AddAsync(appointment);
                TempData["StatusMessage"] = "Đăng ký lịch tiêm thành công! Vui lòng chờ xác nhận từ cán bộ y tế.";
                return RedirectToAction(nameof(Index));
            }

            var allSites = await _siteRepository.GetAllAsync();
            var availableProvinces = allSites
                .Select(s => s.Address.Split(',').LastOrDefault()?.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct()
                .OrderBy(p => p)
                .Select(p => new SelectListItem { Value = p, Text = p })
                .ToList();

            model.AvailableVaccines = await _vaccineRepository.GetAllAsync();
            model.AvailableProvinces = availableProvinces;
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
            if (string.IsNullOrEmpty(provinceName) || string.IsNullOrEmpty(districtName) || string.IsNullOrEmpty(wardName)) return Json(new List<SelectListItem>());
            var sites = await _siteRepository.GetAllAsync();
            var filteredSites = sites
                .Where(s => s.Address.Contains($", {wardName}, {districtName}, {provinceName}"))
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"{s.Name} - {s.Address.Split(',')[0].Trim()}" })
                .OrderBy(s => s.Text).ToList();
            return Json(filteredSites);
        }

        // ======================= BẮT ĐẦU SỬA LỖI =======================
        public async Task<IActionResult> VaccinationCertificate(int appointmentId)
        {
            var record = await _recordRepository.GetByAppointmentIdAsync(appointmentId);
            
            if (record == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            if (record.Appointment.UserId != currentUser.Id)
            {
                return Forbid();
            }
            
            return View(record);
        }
    }
}