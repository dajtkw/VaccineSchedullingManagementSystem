using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.ViewModels
{
    public class EditSiteViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên điểm tiêm là bắt buộc.")]
        [StringLength(200)]
        [Display(Name = "Tên điểm tiêm chủng")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Giờ hoạt động")]
        public string? OperatingHours { get; set; }

        [Display(Name = "Tỉnh/Thành phố")]
        public string? ProvinceName { get; set; }

        [Display(Name = "Quận/Huyện")]
        public string? DistrictName { get; set; }

        [Display(Name = "Phường/Xã")]
        public string? WardName { get; set; }

        [Display(Name = "Số nhà, tên đường")]
        public string? StreetAddress { get; set; }
    }
}