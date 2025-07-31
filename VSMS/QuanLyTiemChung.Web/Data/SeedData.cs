using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTiemChung.Web.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();


                // Tạo các vai trò nếu chúng chưa tồn tại
                string[] roleNames = { "Admin", "HealthOfficial", "Citizen" };
                foreach (var roleName in roleNames)
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        await roleManager.CreateAsync(new Role { Name = roleName });
                    }
                }

                // Tạo tài khoản Admin mẫu nếu chưa có
                var adminEmail = "admin@example.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    var newAdminUser = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "Quản Trị Viên",
                        EmailConfirmed = true, // Bỏ qua bước xác thực email cho tài khoản mẫu
                        DateOfBirth = new DateTime(1990, 1, 1)
                    };

                    var result = await userManager.CreateAsync(newAdminUser, "Admin@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newAdminUser, "Admin");
                    }
                }

                
                // Kiểm tra xem đã có danh mục nào chưa
                if (!context.VaccineCategories.Any())
                {
                    // Nếu chưa có, thêm các danh mục mẫu
                    await context.VaccineCategories.AddRangeAsync(
                        new VaccineCategory { Name = "Vắc-xin cho trẻ em" },
                        new VaccineCategory { Name = "Vắc-xin cho người lớn" },
                        new VaccineCategory { Name = "Vắc-xin cho phụ nữ mang thai" },
                        new VaccineCategory { Name = "Vắc-xin phòng COVID-19" },
                        new VaccineCategory { Name = "Vắc-xin Cúm mùa" },
                        new VaccineCategory { Name = "Vắc-xin du lịch & đặc thù" }
                    );
                    
                    // Lưu các thay đổi vào cơ sở dữ liệu
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
