namespace Shopping_Tutorial.Models.Momo
{
    public class MomoOptionModel
    {
        public string MomoApiUrl { get; set; } //Địa chỉ API của MoMo
        public string AccessKey { get; set; } //Khóa truy cập
        public string SecretKey { get; set; } //Khóa bí mật
        public string ReturnUrl { get; set; } //Địa chỉ trả về
        public string NotifyUrl { get; set; } //Địa chỉ thông báo
        public string PartnerCode { get; set; } //Mã đối tác
        public string RequestType { get; set; } //Loại yêu cầu
    }
}
