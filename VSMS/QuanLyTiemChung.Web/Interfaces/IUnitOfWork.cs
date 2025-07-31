namespace QuanLyTiemChung.Web.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IAppointmentRepository Appointments { get; }
        IVaccineRepository Vaccines { get; }
        // ...Khai báo các repository khác ở đây

        // Phương thức để lưu tất cả các thay đổi vào CSDL
        Task<int> CompleteAsync();
    }
}