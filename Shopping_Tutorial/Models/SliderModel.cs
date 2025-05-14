using Shopping_Tutorial.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class SliderModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Tên Slider")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Mô tả Thương hiệu")]
        public string Description { get; set; }
        public int? Status { get; set; }
        public string Image { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpload { get; set; }
    }
}
