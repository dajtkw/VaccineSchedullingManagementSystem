using QuanLyTiemChung.Web.Models;

namespace QuanLyTiemChung.Web.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<IEnumerable<User>> GetUsersWithRolesAsync();
    Task<User?> GetUserDetailsWithAppointmentsAndRolesAsync(int id);
    Task<IEnumerable<User>> GetCitizensOrderedByNameAsync();
    Task<bool> HasAppointmentsAsync(int userId);
}