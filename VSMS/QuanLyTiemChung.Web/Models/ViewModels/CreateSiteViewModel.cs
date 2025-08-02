using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.ViewModels
{
    public class CreateSiteViewModel
    {
        [Required(ErrorMessage = "Tên điểm tiêm là bắt buộc.")]
        [StringLength(200)]
        [Display(Name = "Tên điểm tiêm chủng")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Giờ hoạt động")]
        public string? OperatingHours { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố.")]
        [Display(Name = "Tỉnh/Thành phố")]
        public string ProvinceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn Quận/Huyện.")]
        [Display(Name = "Quận/Huyện")]
        public string DistrictName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn Phường/Xã.")]
        [Display(Name = "Phường/Xã")]
        public string WardName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ chi tiết.")]
        [StringLength(200)]
        [Display(Name = "Số nhà, tên đường")]
        public string StreetAddress { get; set; } = string.Empty;
    }
}