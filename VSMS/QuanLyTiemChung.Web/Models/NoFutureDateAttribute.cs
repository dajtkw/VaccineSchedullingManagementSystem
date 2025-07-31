using System.ComponentModel.DataAnnotations;

// Đặt namespace phù hợp với cấu trúc dự án của bạn
namespace QuanLyTiemChung.Web.Attributes 
{
    public class NoFutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            // Chuyển đổi giá trị sang kiểu DateTime
            var dateValue = (DateTime)value;

            // So sánh ngày sinh với ngày hiện tại (chỉ lấy phần ngày, bỏ qua giờ)
            if (dateValue > DateTime.Today)
            {
                // Nếu ngày sinh ở tương lai, trả về lỗi
                return new ValidationResult(ErrorMessage ?? "Ngày không được ở tương lai.");
            }

            // Nếu ngày hợp lệ, trả về thành công
            return ValidationResult.Success;
        }
    }
}