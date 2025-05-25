using Shopping_Tutorial.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class ProductModel
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Tên Sản phẩm")]
        [MinLength(4, ErrorMessage = "Tên sản phẩm phải có ít nhất 4 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Mô tả Sản phẩm")]
        [MinLength(4, ErrorMessage = "Mô tả sản phẩm phải có ít nhất 4 ký tự")]
        public string Description { get; set; }

        public string Slug { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Giá Sản phẩm")]
        [Range(0.01,double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required,Range(1,int.MaxValue,ErrorMessage = "Chọn một thương hiệu")]
        public int BrandId { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Chọn một danh mục")]
        public int CategoryId { get; set; }

        public CategoryModel Category { get; set; }

        public BrandModel Brand { get; set; }

        public string Image { get; set; } = "noimage.jpg";

        public int Quantity { get; set; }
        public int Sold { get; set; }
        public decimal ImportPrice { get; set; }  // Giá nhập
        public decimal OriginalPrice{ get; set; }  // Giá gốc


        public RatingModel Ratings { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpload { get; set; }

        public ICollection<ProductColor> ProductColors { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; }
        

    }
}
