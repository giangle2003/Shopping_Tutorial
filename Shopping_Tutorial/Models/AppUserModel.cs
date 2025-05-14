using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class AppUserModel : IdentityUser
    {
        public string Occupation { get; set; }  //Nghề nghiệp
        public string RoleId { get; set; } 
        public string Token { get; set; } //Token - chuỗi để xác thực
        [NotMapped]
        public string ConfirmPassword { get; set; } // Xác nhận mật khẩu
    }
}
