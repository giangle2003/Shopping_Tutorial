using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Bạn cần nhập Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Bạn cần nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")] 
        public string Email { get; set; }

        //Mã hóa mật khẩu
        [DataType(DataType.Password),Required(ErrorMessage = "Bạn cần nhập mật khẩu")]
        public string Password { get; set; }
    }
}
