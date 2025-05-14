using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class ColorModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập Tên Màu sắc")]
        public string Name { get; set; }

        public ICollection<ProductColor> ProductColors { get; set; }
    }
}
