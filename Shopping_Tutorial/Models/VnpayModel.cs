using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class VnpayModel
    {
        [Key]
        public int Id { get; set; } //Khóa chính
        public string OrderDescription { get; set; } //Mô tả đơn hàng
        public string TransactionId { get; set; } //Mã giao dịch
        public string OrderId { get; set; } //Mã đơn hàng
        public string PaymentMethod { get; set; }//Phương thức thanh toán
        public string PaymentId { get; set; }//Mã thanh toán

        public DateTime DateCreated { get; set; } //Ngày tạo giao dịch
    }
}
