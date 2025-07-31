using QuanLyTiemChung.Web.Models;

namespace QuanLyTiemChung.Web.ViewModels
{
    public class AdminDashboardViewModel
    {
        public IEnumerable<User> Users { get; set; } = new List<User>();
        public IEnumerable<AppointmentViewModel> AllAppointments { get; set; } = new List<AppointmentViewModel>();
        public IEnumerable<Vaccine> AllVaccines { get; set; } = new List<Vaccine>();
        public IEnumerable<VaccinationSite> AllSites { get; set; } = new List<VaccinationSite>();

        // Các thuộc tính này có thể được tính toán
        public int TotalUsers => Users.Count();
        public int TotalAppointments => AllAppointments.Count();
        public int TotalVaccines => AllVaccines.Count();
        public int TotalVaccinationSites => AllSites.Count(); 

    }
}
