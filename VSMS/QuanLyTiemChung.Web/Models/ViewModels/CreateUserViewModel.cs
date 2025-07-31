using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyTiemChung.Web.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        [NoFutureDate(ErrorMessage = "Ngày sinh không được là một ngày trong tương lai.")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Dùng để hiển thị danh sách các vai trò
        public IEnumerable<SelectListItem> AllRoles { get; set; } = new List<SelectListItem>();

        // Dùng để lấy vai trò được chọn từ form
        [Required(ErrorMessage = "Vui lòng chọn một vai trò.")]
        [Display(Name = "Vai trò")]
        public string SelectedRole { get; set; } = string.Empty;
    }
}