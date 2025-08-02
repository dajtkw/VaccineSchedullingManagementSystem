using QuanLyTiemChung.Web.Models;
using QuanLyTiemChung.Web.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyTiemChung.Web.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetByIdAsync(int id);
        Task<IEnumerable<Appointment>> GetByUserIdAsync(int userId);
        Task<IEnumerable<AppointmentViewModel>> GetViewModelsByUserIdAsync(int userId);
        Task<IEnumerable<AppointmentViewModel>> GetViewModelsAsync();
        Task AddAsync(Appointment appointment);
        Task UpdateAsync(Appointment appointment);
        Task<IEnumerable<Appointment>> GetUpcomingAppointments(int hours);
        Task<bool> DeleteAsync(int id);

    }
}