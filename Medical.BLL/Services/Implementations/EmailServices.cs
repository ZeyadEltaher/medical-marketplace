using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Medical.BLL.Services
{
    public class EmailServices : IEmailServices
    {
        private readonly IConfiguration _configuration;
        public EmailServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var message = new MailMessage();

            message.From = new MailAddress(_configuration["Email:Username"]!, "Medical Company");
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var smtp = new SmtpClient(_configuration["Email:Host"]);
            smtp.Port = _configuration.GetValue<int>("Email:Port");
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(
                userName: _configuration["Email:Username"],
                password: _configuration["Email:Password"]
            );

            await smtp.SendMailAsync(message);
        }
    }
}