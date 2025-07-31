using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.Models;

public class VaccinationRecord
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int HealthOfficialId { get; set; }
    public DateTime ActualVaccinationTime { get; set; }

    [StringLength(50)]
    public string? VaccineLotNumber { get; set; }
    public string? NotesAfterShot { get; set; }

    // Navigation Properties
    public virtual Appointment Appointment { get; set; } = null!;
    public virtual User HealthOfficial { get; set; } = null!;
}