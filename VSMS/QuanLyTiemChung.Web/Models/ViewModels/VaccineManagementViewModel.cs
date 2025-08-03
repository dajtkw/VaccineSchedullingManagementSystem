// ViewModels/VaccineManagementViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.ViewModels
{
    public class VaccineManagementViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Tên thương mại là bắt buộc")]
        public string TradeName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Tên chung là bắt buộc")]
        public string GenericName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int VaccineCategoryId { get; set; }
        
        public string? Manufacturer { get; set; }
        public string? CountryOfOrigin { get; set; }
        public decimal? Price { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? Description { get; set; }
        public string? Indications { get; set; }
        public string? Contraindications { get; set; }
        public bool IsActive { get; set; } = true;
        
        // 🔥 QUAN TRỌNG: Sử dụng List thay vì ICollection
        public List<VaccineDoseViewModel> Doses { get; set; } = new List<VaccineDoseViewModel>();
    }

    public class VaccineDoseViewModel
    {
        public int Id { get; set; }
        public int VaccineId { get; set; }
        
        [Required(ErrorMessage = "Số mũi là bắt buộc")]
        [Range(1, 20, ErrorMessage = "Số mũi phải từ 1 đến 20")]
        public int DoseNumber { get; set; }
        
        [Required(ErrorMessage = "Độ tuổi khuyến nghị là bắt buộc")]
        public string RecommendedAge { get; set; } = string.Empty;
        
        public int? IntervalInMonths { get; set; }
        public string? Notes { get; set; }
        public bool IsRequired { get; set; } = true;
    }
}
