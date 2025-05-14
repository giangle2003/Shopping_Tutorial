namespace Shopping_Tutorial.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public decimal ShippingCost { get; set; } //Phí vận chuyển
        public string ShippingAddress { get; set; } //Địa chỉ giao hàng
        public string PhonenumberDelivery { get; set; } //số điện thoai người nhận
        public string UserName { get; set; } //Lấy đơn hàng theo tên người dùng
        public DateTime CreatedDate { get; set; } //Ngày tạo đơn hàng
        public int Status { get; set; } //Trạng thái đơn hàng
        public string CouponCode { get; set; } //Lấy coupon
        public decimal Discount { get; set; } //Giảm giá

        public string? PaymentMethod { get; set; } //Phương thức thanh toán
    }
}
