using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.Models;

public class User : IdentityUser<int>
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    // Navigation Properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}