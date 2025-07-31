using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.Models
{
    public class VaccineCategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        [StringLength(100)]
        [Display(Name = "Tên Danh mục")]
        public string Name { get; set; } = string.Empty;

        // Navigation Property: Một danh mục có nhiều vắc-xin
        public virtual ICollection<Vaccine> Vaccines { get; set; } = new List<Vaccine>();
    }
}