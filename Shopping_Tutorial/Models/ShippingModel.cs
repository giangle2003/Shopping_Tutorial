namespace Shopping_Tutorial.Models
{
    public class ShippingModel
    {
        public int Id { get; set; }
        public decimal Price  { get; set; }
        public string Ward { get; set; }  //lưu trữ tên phường
        public string District { get; set; } //lưu trữ tên quận 
        public string City { get; set; } //lưu trữ tên tỉnh

    }
}
