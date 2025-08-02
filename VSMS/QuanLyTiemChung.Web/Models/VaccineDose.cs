using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTiemChung.Web.Models
{
    public class VaccineDose
    {
        public int Id { get; set; }
        
        [Required]
        public int VaccineId { get; set; }
        
        [Required(ErrorMessage = "Số mũi là bắt buộc")]
        [Range(1, 20, ErrorMessage = "Số mũi phải từ 1 đến 20")]
        [Display(Name = "Thứ tự mũi tiêm")]
        public int DoseNumber { get; set; }
        
        [Required(ErrorMessage = "Độ tuổi khuyến nghị là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Độ tuổi khuyến nghị")]
        public string RecommendedAge { get; set; } = string.Empty;
        
        [Display(Name = "Khoảng cách (tháng)")]
        public int? IntervalInMonths { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }
        
        [Display(Name = "Bắt buộc")]
        public bool IsRequired { get; set; } = true;
        
        // Navigation property
        public virtual Vaccine? Vaccine { get; set; }
    }
}