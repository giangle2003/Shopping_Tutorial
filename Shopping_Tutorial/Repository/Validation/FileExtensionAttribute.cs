using System;
using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Repository.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                string[] extensions = { "jpg", "jpeg", "png", "webp"};
                bool result = extensions.Any(x => extension.EndsWith(x));
                if (!result)
                {
                    return new ValidationResult("Cho phép hình ảnh có đuôi jpg, png, jpeg và webp");
                }
            }
            return ValidationResult.Success;
        }
    }
}
