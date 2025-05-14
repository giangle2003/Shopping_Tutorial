namespace Shopping_Tutorial.Models
{
    public class OrderInfor
    {
        public string FullName { get; set; }  //Người thanh toán
        public string OrderId { get; set; } //Mã đơn hàng
        public string OrderInformation { get; set; } //Thông tin đơn hàng
        public string Amount { get; set; } //Số tiền thanh toán
    }
}
