using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models.Vnpay
{
    public class PaymentResponseModel
    {
        public string OrderDescription { get; set; } //Mô tả đơn hàng
        public string TransactionId { get; set; } //Mã giao dịch
        public string OrderId { get; set; } //Mã đơn hàng
        public string PaymentMethod { get; set; }//Phương thức thanh toán
        public string PaymentId { get; set; }//Mã thanh toán
        public bool Success { get; set; }//Trạng thái thanh toán
        public string Token { get; set; }//Token
        public string VnPayResponseCode { get; set; } //Mã phản hồi từ VNPAY


    }
}
