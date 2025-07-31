using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.Models;

public class Role : IdentityRole<int>
{
    // Navigation Property
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}