using QuanLyTiemChung.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyTiemChung.Web.Interfaces
{
    public interface IVaccineCategoryRepository
    {
        /// <summary>
        /// Lấy tất cả các danh mục vắc-xin.
        /// </summary>
        /// <returns>Danh sách các danh mục.</returns>
        Task<IEnumerable<VaccineCategory>> GetAllAsync();

        /// <summary>
        /// Lấy một danh mục theo ID.
        /// </summary>
        /// <param name="id">ID của danh mục.</param>
        /// <returns>Đối tượng VaccineCategory hoặc null nếu không tìm thấy.</returns>
        Task<VaccineCategory?> GetByIdAsync(int id);

        /// <summary>
        /// Thêm một danh mục mới vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="category">Đối tượng danh mục cần thêm.</param>
        Task AddAsync(VaccineCategory category);

        /// <summary>
        /// Cập nhật thông tin một danh mục.
        /// </summary>
        /// <param name="category">Đối tượng danh mục với thông tin đã cập nhật.</param>
        Task UpdateAsync(VaccineCategory category);

        /// <summary>
        /// Xóa một danh mục khỏi cơ sở dữ liệu.
        /// </summary>
        /// <param name="id">ID của danh mục cần xóa.</param>
        /// <returns>True nếu xóa thành công, False nếu thất bại (ví dụ: vẫn còn vắc-xin thuộc danh mục).</returns>
        Task<bool> DeleteAsync(int id);
    }
}
