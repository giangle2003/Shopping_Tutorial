using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public ProductModel ProductDetails { get; set; }
        public bool HasRated { get; set; } //check xem đã đánh giá chưa

        public List<RatingModel> Ratings { get; set; } = new List<RatingModel>(); //list đánh giá

        [Required(ErrorMessage = "Bạn cần nhập đánh giá")]
        public string Comment { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        // Thêm thuộc tính màu sắc để hiển thị trong view
        public List<ProductColor> ProductColors { get; set; } = new List<ProductColor>(); // Danh sách màu sắc của sản phẩm
        // Thêm danh sách ảnh phụ
        public List<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    }
}
