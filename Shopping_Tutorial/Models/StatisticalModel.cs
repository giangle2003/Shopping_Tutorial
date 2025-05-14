namespace Shopping_Tutorial.Models
{
    public class StatisticalModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; } // Số lượng sản phẩm bán ra
        public int Sold { get; set; } // Số lượng đơn hàng
        public decimal Revenue { get; set; } //Doanh thu
        public decimal Profit { get; set; } // Lợi nhuận
        public DateTime DateCreated { get; set; } //Ngày đặt hàng
    }
}
