using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyTiemChung.Web.Attributes
{
    /// <summary>
    /// Thuộc tính xác thực để đảm bảo một ngày không nằm trong quá khứ.
    /// </summary>
    public class NoPastDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Thuộc tính [Required] nên xử lý các giá trị null.
            if (value is not DateTime dateValue)
            {
                return ValidationResult.Success;
            }

            // So sánh phần ngày, bỏ qua phần giờ.
            if (dateValue.Date < DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? "Ngày không được ở trong quá khứ.");
            }

            return ValidationResult.Success;
        }
    }
}