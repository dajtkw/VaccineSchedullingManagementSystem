using System.Collections.Generic;
using System.Linq;

namespace QuanLyTiemChung.Web.ViewModels
{
    public class UserAppointmentsViewModel
    {
        public IEnumerable<AppointmentViewModel> UpcomingAppointments { get; set; }
        public IEnumerable<AppointmentViewModel> AppointmentHistory { get; set; }

        public UserAppointmentsViewModel(IEnumerable<AppointmentViewModel> allAppointments)
        {
            UpcomingAppointments = allAppointments
                .Where(a => a.Status == "Pending" || a.Status == "Confirmed")
                .OrderBy(a => a.ScheduledDateTime);

            AppointmentHistory = allAppointments
                .Where(a => a.Status == "Completed" || a.Status == "Cancelled")
                .OrderByDescending(a => a.ScheduledDateTime);
        }
    }
}