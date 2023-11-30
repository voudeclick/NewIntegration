using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using Samurai.Integration.Domain.Infrastructure.Email;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Samurai.Integration.Email
{
    public class EmailClientSmtp : IEmailClientSmtp
    {
        private readonly EmailSettings _emailSettings;
        private readonly IHostEnvironment _env;
        public EmailClientSmtp(IOptions<EmailSettings> emailSettings,
            IHostEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _env = env;
        }

        public async Task SendAsync(EmailDto emailDto)
        {
            var builder = new BodyBuilder
            {
                HtmlBody = await GetEmailTemplateHtml(emailDto.Body),                
            };

            var mineMessage = new MimeMessage
            {               
                Subject = emailDto.Subject,
                Sender = MailboxAddress.Parse(_emailSettings.Mail),
                Body = builder.ToMessageBody(),                
            };

            foreach (var email in emailDto.ToEmails)
                mineMessage.To.Add(MailboxAddress.Parse(email));                                 
            
            using var smtp = new SmtpClient();
            smtp.Connect(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_emailSettings.Mail, _emailSettings.Password);
            await smtp.SendAsync(mineMessage);
            smtp.Disconnect(true);
        }
    
        private async Task<string> GetEmailTemplateHtml(string body)
        {
            using var reader = new StreamReader(
                Path.Combine(_env.ContentRootPath,@"Templates\SamuraiTemplate.html")
            );

            var emailTemplateHtml = await reader.ReadToEndAsync();

            reader.Close();

            return emailTemplateHtml.Replace("{{BODY}}", body);
        }
    }
}
