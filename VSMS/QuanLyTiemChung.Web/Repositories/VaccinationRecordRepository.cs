using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.Data;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;

namespace QuanLyTiemChung.Web.Repositories
{
    public class VaccinationRecordRepository : IVaccinationRecordRepository
    {
        private readonly ApplicationDbContext _context;

        public VaccinationRecordRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(VaccinationRecord record)
        {
            await _context.VaccinationRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        // ======================= BẮT ĐẦU CẬP NHẬT =======================
        public async Task<VaccinationRecord?> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.VaccinationRecords
                // THÊM CÁC DÒNG NÀY ĐỂ TẢI DỮ LIỆU LIÊN QUAN
                .Include(r => r.HealthOfficial)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.User)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Vaccine)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.VaccinationSite)
                // KẾT THÚC PHẦN THÊM
                .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);
        }
        // ======================== KẾT THÚC CẬP NHẬT =======================

        public async Task<IEnumerable<VaccinationRecord>> GetByUserIdAsync(int userId)
        {
            return await _context.VaccinationRecords
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Vaccine)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.VaccinationSite)
                .Where(r => r.Appointment.UserId == userId)
                .OrderByDescending(r => r.ActualVaccinationTime)
                .ToListAsync();
        }
    }
}