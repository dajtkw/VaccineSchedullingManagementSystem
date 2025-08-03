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
                .Where(a => a.Status == "Confirmed")
                .Where(a => a.IsReminderSent == false)
                .Where(a => a.ScheduledDateTime > now)
                .Where(a => a.ScheduledDateTime <= reminderTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<AppointmentViewModel>> GetViewModelsAsync()
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Vaccine)
                .Include(a => a.VaccinationSite)
                .Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    UserFullName = a.User != null ? a.User.FullName : "Không có dữ liệu",
                    VaccineName = a.Vaccine != null ? a.Vaccine.TradeName : "Không có dữ liệu",
                    SiteName = a.VaccinationSite != null ? a.VaccinationSite.Name : "Không có dữ liệu",
                    SiteAddress = a.VaccinationSite != null ? a.VaccinationSite.Address : "Không có dữ liệu",
                    ScheduledDateTime = a.ScheduledDateTime,
                    DoseNumber = a.DoseNumber,
                    Status = a.Status,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt,

                    // Gán dữ liệu cho các thuộc tính mới
                    VaccinationSiteName = a.VaccinationSite != null ? a.VaccinationSite.Name : "Không có dữ liệu",
                    StatusBadgeClass =
        a.Status == "Confirmed" ? "badge bg-success" :
        a.Status == "Completed" ? "badge bg-primary" :
        a.Status == "Pending" ? "badge bg-warning text-dark" :
        a.Status == "Cancelled" ? "badge bg-danger" :
        "badge bg-secondary"
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AppointmentViewModel>> GetViewModelsByUserIdAsync(int userId)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Include(a => a.User)
                .Include(a => a.Vaccine)
                .Include(a => a.VaccinationSite)
                .Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    UserFullName = a.User != null ? a.User.FullName : "Không có dữ liệu",
                    VaccineName = a.Vaccine != null ? a.Vaccine.TradeName : "Không có dữ liệu",
                    SiteName = a.VaccinationSite != null ? a.VaccinationSite.Name : "Không có dữ liệu",
                    SiteAddress = a.VaccinationSite != null ? a.VaccinationSite.Address : "Không có dữ liệu",
                    ScheduledDateTime = a.ScheduledDateTime,
                    DoseNumber = a.DoseNumber,
                    Status = a.Status,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt,

                    // Gán dữ liệu cho các thuộc tính mới
                    VaccinationSiteName = a.VaccinationSite != null ? a.VaccinationSite.Name : "Không có dữ liệu",
                    StatusBadgeClass =
        a.Status == "Confirmed" ? "badge bg-success" :
        a.Status == "Completed" ? "badge bg-primary" :
        a.Status == "Pending" ? "badge bg-warning text-dark" :
        a.Status == "Cancelled" ? "badge bg-danger" :
        "badge bg-secondary"
                })
                .OrderByDescending(a => a.ScheduledDateTime)
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return false;
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Appointment?> GetLastCompletedAppointmentAsync(int userId, int vaccineId)
        {
            return await _context.Appointments
                .Where(a => a.UserId == userId && a.VaccineId == vaccineId && a.Status == "Completed")
                .Include(a => a.VaccinationRecord) // Tải kèm hồ sơ tiêm chủng để lấy ngày tiêm thực tế
                .OrderByDescending(a => a.ScheduledDateTime)
                .FirstOrDefaultAsync();
        }
    }
}