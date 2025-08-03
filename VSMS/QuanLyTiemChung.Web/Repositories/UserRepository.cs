using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.Data;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;

namespace QuanLyTiemChung.Web.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            // Thêm Include để lấy thông tin UserRoles và Role
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetUsersWithRolesAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<User?> GetUserDetailsWithAppointmentsAndRolesAsync(int id)
        {
            return await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Appointments).ThenInclude(a => a.Vaccine)
                .Include(u => u.Appointments).ThenInclude(a => a.VaccinationSite)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<User>> GetCitizensOrderedByNameAsync()
        {
            return await _context.Users
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Citizen"))
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<bool> HasAppointmentsAsync(int userId)
        {
            return await _context.Appointments.AnyAsync(a => a.UserId == userId);
        }
    }
}