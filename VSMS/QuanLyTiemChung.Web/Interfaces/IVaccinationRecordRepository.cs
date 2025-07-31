using QuanLyTiemChung.Web.Models;

namespace QuanLyTiemChung.Web.Interfaces;

public interface IVaccinationRecordRepository
{
    Task AddAsync(VaccinationRecord record);
    Task<IEnumerable<VaccinationRecord>> GetByUserIdAsync(int userId);
    Task<VaccinationRecord?> GetByAppointmentIdAsync(int appointmentId);
}