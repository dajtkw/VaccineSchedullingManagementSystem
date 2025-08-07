using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.Models;

public class Appointment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int VaccineId { get; set; }
    public int VaccinationSiteId { get; set; }
    public DateTime ScheduledDateTime { get; set; }
    public int DoseNumber { get; set; }
    
    public bool IsReminderSent { get; set; } = false;


    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // "PendingConfirmation", "Confirmed", "Completed", "Cancelled"

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Vaccine Vaccine { get; set; } = null!;
    public virtual VaccinationSite VaccinationSite { get; set; } = null!;
    public virtual VaccinationRecord? VaccinationRecord { get; set; }
}