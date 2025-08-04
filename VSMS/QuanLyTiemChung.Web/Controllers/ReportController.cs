using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.Data;
using QuanLyTiemChung.Web.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTiemChung.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action này chỉ trả về view container, dữ liệu sẽ được tải bằng AJAX
        public IActionResult Index()
        {
            return PartialView("_ReportContent");
        }

        // API endpoint để cung cấp dữ liệu thống kê cho frontend
        [HttpGet]
        public async Task<IActionResult> GetReportData()
        {
            var appointments = await _context.Appointments
                                        .Include(a => a.Vaccine)
                                        .AsNoTracking()
                                        .ToListAsync();

            var last30Days = DateTime.Today.AddDays(-30);

            // Thống kê số lượng lịch hẹn được tạo mỗi ngày trong 30 ngày qua
            var appointmentsByDay = appointments
                .Where(a => a.CreatedAt.Date >= last30Days)
                .GroupBy(a => a.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToList();

            // Thống kê 5 loại vắc-xin được tiêm (hoàn thành) nhiều nhất
            var topVaccines = appointments
                .Where(a => a.Status == "Completed" && a.Vaccine != null)
                .GroupBy(a => a.Vaccine.TradeName)
                .Select(g => new { VaccineName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            var viewModel = new ReportViewModel
            {
                TotalPending = appointments.Count(a => a.Status == "Pending"),
                TotalConfirmed = appointments.Count(a => a.Status == "Confirmed"),
                TotalCompleted = appointments.Count(a => a.Status == "Completed"),
                TotalCancelled = appointments.Count(a => a.Status == "Cancelled"),
                AppointmentsByDay = new ChartDataViewModel
                {
                    Labels = appointmentsByDay.Select(x => x.Date.ToString("dd/MM")).ToList(),
                    Data = appointmentsByDay.Select(x => x.Count).ToList()
                },
                TopVaccines = new ChartDataViewModel
                {
                    Labels = topVaccines.Select(x => x.VaccineName).ToList(),
                    Data = topVaccines.Select(x => x.Count).ToList()
                }
            };

            return Json(viewModel);
        }
    }
}
