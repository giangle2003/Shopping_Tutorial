using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class OrderDetails
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string OrderCode { get; set; }
        public long ProductId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public DateTime OrderDate { get; set; } //Thêm ngày đặt hàng
        public string Name { get; set; } //Tên người nhận
        public decimal ImportPrice { get; set; }  // Giá nhập

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public OrderModel Order { get; set; }

    }
}
