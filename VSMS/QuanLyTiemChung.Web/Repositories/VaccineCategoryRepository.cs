using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.Data;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace QuanLyTiemChung.Web.Repositories
{
    public class VaccineCategoryRepository : IVaccineCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public VaccineCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VaccineCategory>> GetAllAsync()
        {
            // Sắp xếp theo tên để hiển thị danh sách có thứ tự
            return await _context.VaccineCategories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<VaccineCategory?> GetByIdAsync(int id)
        {
            return await _context.VaccineCategories.FindAsync(id);
        }

        public async Task AddAsync(VaccineCategory category)
        {
            await _context.VaccineCategories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(VaccineCategory category)
        {
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.VaccineCategories
                                         .Include(c => c.Vaccines) // Kiểm tra các vắc-xin liên quan
                                         .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return false; // Không tìm thấy danh mục
            }

            // Chỉ cho phép xóa nếu không còn vắc-xin nào thuộc danh mục này
            if (category.Vaccines.Any())
            {
                return false; // Xóa thất bại
            }

            _context.VaccineCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true; // Xóa thành công
        }
    }
}
