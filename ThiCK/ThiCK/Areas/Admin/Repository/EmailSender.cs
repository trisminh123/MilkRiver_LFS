using System.Net.Mail;
using System.Net;

namespace ThiCK.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("vuongminhtri51@gmail.com", "ntru junf daqd lctx")
            };

            return client.SendMailAsync(
                new MailMessage(from: "vuongminhtri51@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
