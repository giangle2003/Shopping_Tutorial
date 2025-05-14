using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class RatingModel
    {
        [Key]
        public int Id { get; set; }

        public long ProductId { get; set; }

        [Required(ErrorMessage ="Bạn cần nhập đánh giá")]
        public string Comment { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Star { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
