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
    }

}
