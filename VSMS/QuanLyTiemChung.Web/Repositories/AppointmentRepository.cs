using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.Data;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;
using QuanLyTiemChung.Web.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTiemChung.Web.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Vaccine)
                .Include(a => a.VaccinationSite)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Appointment>> GetByUserIdAsync(int userId)
        {
            return await _context.Appointments
                .Where(a => a.UserId == userId)
                .Include(a => a.Vaccine)
                .Include(a => a.VaccinationSite)
                .ToListAsync();
        }

        public async Task UpdateAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointments(int hours)
        {
            var now = DateTime.Now;
            var reminderTime = now.AddHours(hours);

            return await _context.Appointments
                .Include(a => a.Vaccine)
                // Chỉ lấy các lịch hẹn đã được xác nhận (Confirmed)
                .Where(a => a.Status == "Confirmed")
                // Lịch hẹn nằm trong khoảng thời gian nhắc nhở
                .Where(a => a.ScheduledDateTime > now && a.ScheduledDateTime <= reminderTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<AppointmentViewModel>> GetViewModelsAsync()
        {
            return await _context.Appointments
                .Include(a => a.User) // Thêm Include để lấy thông tin người dùng
                .Include(a => a.Vaccine)
                .Include(a => a.VaccinationSite)
                .Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    UserFullName = a.User != null ? a.User.FullName : "Không có dữ liệu", // Lấy FullName
                    VaccineName = a.Vaccine != null ? a.Vaccine.TradeName : "Không có dữ liệu", // Sửa lại thành TradeName
                    SiteName = a.VaccinationSite != null ? a.VaccinationSite.Name : "Không có dữ liệu",
                    SiteAddress = a.VaccinationSite != null ? a.VaccinationSite.Address : "Không có dữ liệu",
                    ScheduledDateTime = a.ScheduledDateTime,
                    DoseNumber = a.DoseNumber,
                    Status = a.Status,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AppointmentViewModel>> GetViewModelsByUserIdAsync(int userId)
        {
            return await _context.Appointments
                .Where(a => a.UserId == userId)
                .Include(a => a.User) // Thêm Include để lấy thông tin người dùng
                .Include(a => a.Vaccine)
                .Include(a => a.VaccinationSite)
                .Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    UserFullName = a.User != null ? a.User.FullName : "Không có dữ liệu", // Lấy FullName
                    VaccineName = a.Vaccine != null ? a.Vaccine.TradeName : "Không có dữ liệu", // Sửa lại thành TradeName
                    SiteName = a.VaccinationSite != null ? a.VaccinationSite.Name : "Không có dữ liệu",
                    SiteAddress = a.VaccinationSite != null ? a.VaccinationSite.Address : "Không có dữ liệu",
                    ScheduledDateTime = a.ScheduledDateTime,
                    DoseNumber = a.DoseNumber,
                    Status = a.Status,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                })
                .OrderByDescending(a => a.ScheduledDateTime)
                .ToListAsync();
        }
    }
}
