using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTiemChung.Web.Models
{
    public class Vaccine
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên thương mại là bắt buộc.")]
        [Display(Name = "Tên thương mại")]
        public string TradeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên chung là bắt buộc.")]
        [Display(Name = "Tên chung / Bệnh phòng ngừa")]
        public string GenericName { get; set; } = string.Empty;

        [Display(Name = "Nhà sản xuất")]
        public string? Manufacturer { get; set; }

        [Display(Name = "Mô tả chi tiết")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        [Display(Name = "Danh mục vắc-xin")]
        public int VaccineCategoryId { get; set; }
        [ForeignKey("VaccineCategoryId")]
        public virtual VaccineCategory? VaccineCategory { get; set; }

        // Navigation Properties
        public virtual ICollection<VaccineDose> Doses { get; set; } = new List<VaccineDose>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        [Display(Name = "Nước sản xuất")]
        public string? CountryOfOrigin { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải là một số không âm.")]
        [Display(Name = "Giá (VND)")]
        public decimal? Price { get; set; }

        [Display(Name = "Độ tuổi tối thiểu (năm)")]
        public int? MinAge { get; set; }

        [Display(Name = "Độ tuổi tối đa (năm)")]
        public int? MaxAge { get; set; }

        [Display(Name = "Chỉ định")]
        public string? Indications { get; set; }

        [Display(Name = "Chống chỉ định")]
        public string? Contraindications { get; set; }

        [Display(Name = "Còn sử dụng")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}