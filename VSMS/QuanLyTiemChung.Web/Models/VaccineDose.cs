using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Thêm using này
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyTiemChung.Web.Models
{
    public class VaccineDose
    {
        public int Id { get; set; }

        [Display(Name = "Thứ tự mũi tiêm")]
        public int DoseNumber { get; set; }

        [Display(Name = "Khoảng cách từ mũi trước (ngày)")]
        public int? IntervalFromPreviousDoseInDays { get; set; }

        [StringLength(100)]
        [Display(Name = "Độ tuổi khuyến nghị")]
        public string? RecommendedAge { get; set; }

        public int VaccineId { get; set; }

        [ValidateNever]
        public virtual Vaccine? Vaccine { get; set; }
    }
}
