using QuanLyTiemChung.Web.Models;

namespace QuanLyTiemChung.Web.Interfaces;

public interface IVaccineRepository
{
    Task<Vaccine?> GetByIdAsync(int id);
    Task<IEnumerable<Vaccine>> GetAllAsync();
    Task AddAsync(Vaccine vaccine);
    Task UpdateAsync(Vaccine vaccine);
    Task<bool> DeleteAsync(int id);
}