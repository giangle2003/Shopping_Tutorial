
using System.Net.Mail;
using System.Net;

namespace Shopping_Tutorial.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false, // không sử dụng thông tin đăng nhập mặc định
                Credentials = new NetworkCredential("legiangcute1122@gmail.com", "ryip fstbymskvylu") // thông tin đăng nhập
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("legiangcute1122@gmail.com", "ANTT TECH"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true // ✅ Chỉ định nội dung là HTML
            };

            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }
    }
}
