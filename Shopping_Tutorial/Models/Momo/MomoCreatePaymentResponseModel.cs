namespace Shopping_Tutorial.Models.Momo
{
    public class MomoCreatePaymentResponseModel
    {
        public string RequestId { get; set; } //ID yêu cầu
        public int ErrorCode { get; set; } //Mã lỗi
        public string OrderId { get; set; } //ID đơn hàng
        public string Message { get; set; } //Thông báo
        public string LocalMessage { get; set; } //Thông báo địa phương
        public string RequestType { get; set; } //Loại yêu cầu
        public string PayUrl { get; set; } //Địa chỉ thanh toán
        public string Signature { get; set; } //Chữ ký
        public string QrCodeUrl { get; set; } //Địa chỉ mã QR
        public string Deeplink { get; set; } //Địa chỉ liên kết sâu
        public string DeeplinkWebInApp { get; set; } //Địa chỉ liên kết sâu trong ứng dụng
    }
}
