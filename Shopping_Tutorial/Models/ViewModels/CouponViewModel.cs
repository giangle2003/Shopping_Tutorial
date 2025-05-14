using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models.ViewModels
{
    public class CouponViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Bạn phải nhập tên coupon.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Bạn phải nhập mô tả coupon.")]
        public string Description { get; set; }
        public DateTime DateStar { get; set; }
        public DateTime DateExpired { get; set; }

        public int? DiscountPercent { get; set; } //Giảm giá theo phần trăm
        public decimal? DiscountPrice { get; set; } //Giảm giá theo giá tiền
        public decimal? MaxDiscountAmount { get; set; } // Giảm tối đa


        [Required(ErrorMessage = "Bạn phải nhập số lượng coupon.")]
        public int Quantity { get; set; }
        public int Status { get; set; }


        [Required(ErrorMessage = "Vui lòng chọn ít nhất một danh mục.")]
        public List<int> SelectedCategoryIds { get; set; }

        public List<CategoryModel> Categories { get; set; }
    }
}
