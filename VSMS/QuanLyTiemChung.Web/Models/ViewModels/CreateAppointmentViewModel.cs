using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyTiemChung.Web.Attributes;
using QuanLyTiemChung.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.ViewModels
{
    public class CreateAppointmentViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn một loại vắc-xin.")]
        [Display(Name = "Vắc-xin đăng ký")]
        public int SelectedVaccineId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn một địa điểm tiêm.")]
        [Display(Name = "Cơ sở tiêm chủng")]
        public int SelectedSiteId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày và giờ hẹn.")]
        [Display(Name = "Ngày giờ hẹn")]
        [NoPastDate(ErrorMessage = "Ngày hẹn không được ở trong quá khứ.")]
        public DateTime ScheduledDateTime { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Mũi tiêm phải là một số từ 1 đến 10.")]
        [Display(Name = "Mũi tiêm thứ")]
        public int DoseNumber { get; set; } = 1;

        [Display(Name = "Ghi chú (nếu có)")]
        public string? Notes { get; set; }

        // Dữ liệu để hiển thị trên form
        public IEnumerable<Vaccine> AvailableVaccines { get; set; } = new List<Vaccine>();

        public IEnumerable<SelectListItem> AvailableProvinces { get; set; } = new List<SelectListItem>();
    }
}