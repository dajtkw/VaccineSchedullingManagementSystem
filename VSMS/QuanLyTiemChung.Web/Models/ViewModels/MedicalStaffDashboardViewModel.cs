using System.Collections.Generic;

namespace QuanLyTiemChung.Web.ViewModels
{
    public class MedicalStaffDashboardViewModel
    {
        public IEnumerable<AppointmentViewModel> AllAppointments { get; set; } = new List<AppointmentViewModel>();
        public int PendingCount { get; set; }
        public int AppointmentsTodayCount { get; set; }
    }
}