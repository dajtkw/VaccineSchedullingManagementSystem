using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.ViewModels
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }

        public int UserId { get; set; } 

        [Display(Name = "Họ và tên")]
        public string UserFullName { get; set; } = string.Empty;

        [Display(Name = "Tên vắc-xin")]
        public string VaccineName { get; set; } = string.Empty;

        [Display(Name = "Cơ sở tiêm chủng")]
        public string SiteName { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ")]
        public string SiteAddress { get; set; } = string.Empty;

        [Display(Name = "Ngày giờ hẹn")]
        public DateTime ScheduledDateTime { get; set; }

        [Display(Name = "Mũi thứ")]
        public int DoseNumber { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Ngày đăng ký")]
        public DateTime CreatedAt { get; set; }
    }
}