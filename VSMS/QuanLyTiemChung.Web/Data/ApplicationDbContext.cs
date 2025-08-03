using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.Models;


namespace QuanLyTiemChung.Web.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<VaccineCategory> VaccineCategories { get; set; }
    public DbSet<Vaccine> Vaccines { get; set; }
    public DbSet<VaccineDose> VaccineDoses { get; set; } // THÊM DbSet CHO BẢNG MỚI
    public DbSet<VaccinationSite> VaccinationSites { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<VaccinationRecord> VaccinationRecords { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<SiteVaccineInventory> SiteVaccineInventories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRole>(userRole =>
        {
            userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

            userRole.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            userRole.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });

        // Cấu hình mối quan hệ cho VaccineCategory và Vaccine
        modelBuilder.Entity<Vaccine>()
            .HasOne(v => v.VaccineCategory)
            .WithMany(vc => vc.Vaccines)
            .HasForeignKey(v => v.VaccineCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // THÊM CẤU HÌNH MỚI CHO VACCINEDOSE
        modelBuilder.Entity<VaccineDose>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RecommendedAge).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            entity.HasOne(d => d.Vaccine) // Một liều tiêm thuộc về một vắc-xin
                  .WithMany(v => v.Doses)   // Một vắc-xin có nhiều liều tiêm
                  .HasForeignKey(d => d.VaccineId) // Khóa ngoại là VaccineId
                  .OnDelete(DeleteBehavior.Cascade); // Nếu xóa vắc-xin, các liều tiêm liên quan cũng sẽ bị xóa
        });

        // Cấu hình mối quan hệ một-một giữa Appointment và VaccinationRecord
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.VaccinationRecord)
            .WithOne(vr => vr.Appointment)
            .HasForeignKey<VaccinationRecord>(vr => vr.AppointmentId);

        // Cấu hình các mối quan hệ với Appointment
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Vaccine)
            .WithMany(v => v.Appointments)
            .HasForeignKey(a => a.VaccineId)
            .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.VaccinationSite)
            .WithMany(vs => vs.Appointments)
            .HasForeignKey(a => a.VaccinationSiteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cấu hình quan hệ cho HealthOfficial trong VaccinationRecord
        modelBuilder.Entity<VaccinationRecord>()
            .HasOne(vr => vr.HealthOfficial)
            .WithMany()
            .HasForeignKey(vr => vr.HealthOfficialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}