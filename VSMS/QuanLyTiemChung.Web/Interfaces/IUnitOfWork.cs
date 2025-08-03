namespace QuanLyTiemChung.Web.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IAppointmentRepository Appointments { get; }
        IVaccineRepository Vaccines { get; }

        Task<int> CompleteAsync();
    }
}