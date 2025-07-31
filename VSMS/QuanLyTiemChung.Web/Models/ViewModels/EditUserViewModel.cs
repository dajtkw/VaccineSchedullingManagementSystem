using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.ViewModels
{
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        // Dùng để hiển thị danh sách các vai trò
        public IEnumerable<SelectListItem> AllRoles { get; set; } = new List<SelectListItem>();

        // Dùng để lấy vai trò được chọn từ form
        [Required(ErrorMessage = "Vui lòng chọn một vai trò.")]
        [Display(Name = "Vai trò")]
        public string SelectedRole { get; set; } = string.Empty;
    }
}