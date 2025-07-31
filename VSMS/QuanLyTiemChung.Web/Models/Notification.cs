using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.Models;

public class Notification
{
    public long Id { get; set; }
    public int UserId { get; set; }

    [Required]
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(50)]
    public string? NotificationType { get; set; } // "AppointmentReminder", "RegistrationConfirmed"

    // Navigation Property
    public virtual User User { get; set; } = null!;
}