using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models.ViewModels
{
    public class LoginViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Bạn cần nhập Username")]
        public string Username { get; set; }

        //Mã hóa mật khẩu
        [DataType(DataType.Password), Required(ErrorMessage = "Bạn cần nhập Email")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
