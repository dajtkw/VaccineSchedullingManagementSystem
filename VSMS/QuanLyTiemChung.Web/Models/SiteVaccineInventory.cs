using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.Models
{
    public class SiteVaccineInventory
    {
        public int Id { get; set; }

        [Required]
        public int VaccinationSiteId { get; set; }
        public virtual VaccinationSite? Site { get; set; }

        [Required]
        public int VaccineId { get; set; }
        public virtual Vaccine? Vaccine { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải là số không âm.")]
        [Display(Name = "Số lượng tồn kho")]
        public int Quantity { get; set; }
    }
}