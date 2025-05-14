using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class ProductColor
    {
        public long ProductId { get; set; }
        public ProductModel Product { get; set; }

        public int ColorId { get; set; }
        public ColorModel Color { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int Quantity { get; set; } // Số lượng cho từng màu
        public int Sold { get; set; } // Số lượng đã bán cho từng màu
    }
}
