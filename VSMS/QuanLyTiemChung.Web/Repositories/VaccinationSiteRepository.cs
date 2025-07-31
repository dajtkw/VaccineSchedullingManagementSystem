using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.Data;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;

namespace QuanLyTiemChung.Web.Repositories
{
    public class VaccinationSiteRepository : IVaccinationSiteRepository
    {
        private readonly ApplicationDbContext _context;

        public VaccinationSiteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(VaccinationSite site)
        {
            await _context.VaccinationSites.AddAsync(site);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<VaccinationSite>> GetAllAsync()
        {
            return await _context.VaccinationSites.ToListAsync();
        }

        public async Task<VaccinationSite?> GetByIdAsync(int id)
        {
            return await _context.VaccinationSites.FindAsync(id);
        }

        public async Task UpdateAsync(VaccinationSite site)
        {
            _context.VaccinationSites.Update(site);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Kiểm tra xem có lịch hẹn nào liên quan đến địa điểm này không
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.VaccinationSiteId == id);
            if (hasAppointments)
            {
                // Nếu có, không cho phép xóa và trả về false
                return false;
            }

            var site = await _context.VaccinationSites.FindAsync(id);
            if (site != null)
            {
                _context.VaccinationSites.Remove(site);
                await _context.SaveChangesAsync();
            }
            return true;
        }
    }
}