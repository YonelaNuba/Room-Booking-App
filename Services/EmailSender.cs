
using System.Net;
using System.Net.Mail;

namespace RoomBookingSystem.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public SmtpEmailSender(IConfiguration config) => _config = config;
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var s = _config.GetSection("Smtp");
            var host = s.GetValue<string>("Host");
            var port = s.GetValue<int>("Port");
            var user = s.GetValue<string>("UserName");
            var pass = s.GetValue<string>("Password");
            var enableSsl = s.GetValue<bool>("EnableSsl");

            using var client = new SmtpClient(host, port) { EnableSsl = enableSsl, Credentials = new NetworkCredential(user, pass) };
            var msg = new MailMessage(user, to, subject, body) { IsBodyHtml = true };
            await client.SendMailAsync(msg);
        }
    }
}
