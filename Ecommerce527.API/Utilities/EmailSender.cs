using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace ECommerce527.API.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("bahaa.test99@gmail.com", "rtdr diyk xbqy xeml\r\n")
            };
            var mail = new MailMessage(from: "bahaa.test99@gmail.com", to: email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };
            return client.SendMailAsync(mail);
        }
    }
}   
