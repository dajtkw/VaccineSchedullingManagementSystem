using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.Data;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTiemChung.Web.Repositories
{
    public class VaccineRepository : IVaccineRepository
    {
        private readonly ApplicationDbContext _context;

        public VaccineRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Vaccine vaccine)
        {
            await _context.Vaccines.AddAsync(vaccine);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Vaccine>> GetAllAsync()
        {
            // CẬP NHẬT: Thêm .Include để tải dữ liệu liên quan
            return await _context.Vaccines
                .Include(v => v.VaccineCategory)
                .Include(v => v.Doses)
                .ToListAsync();
        }

        public async Task<Vaccine?> GetByIdAsync(int id)
        {
            // CẬP NHẬT: Dùng FirstOrDefaultAsync với Include để lấy dữ liệu liên quan
            return await _context.Vaccines
                .Include(v => v.VaccineCategory)
                .Include(v => v.Doses)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task UpdateAsync(Vaccine vaccine)
        {
            _context.Vaccines.Update(vaccine);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.VaccineId == id);
            if (hasAppointments)
            {
                return false;
            }

            var vaccine = await _context.Vaccines.FindAsync(id);
            if (vaccine != null)
            {
                _context.Vaccines.Remove(vaccine);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
