using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class CouponCategory
    {
        [Key]
        public int Id { get; set; }
        public int couponId { get; set; }
        public int categoryId { get; set; }

        public CouponModel Coupon { get; set; }
        public CategoryModel Category { get; set; }
    }
}
