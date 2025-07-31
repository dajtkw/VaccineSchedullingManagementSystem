using QuanLyTiemChung.Web.Models;

namespace QuanLyTiemChung.Web.Interfaces;

public interface IVaccinationSiteRepository
{
    Task<VaccinationSite?> GetByIdAsync(int id);
    Task<IEnumerable<VaccinationSite>> GetAllAsync();
    Task AddAsync(VaccinationSite site);
    Task UpdateAsync(VaccinationSite site);
    Task<bool> DeleteAsync(int id);
}