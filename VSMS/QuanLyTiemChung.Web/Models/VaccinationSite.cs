using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.Models;

public class VaccinationSite
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Address { get; set; } = string.Empty;

    [StringLength(100)]
    public string? OperatingHours { get; set; }

    // Navigation Property
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}