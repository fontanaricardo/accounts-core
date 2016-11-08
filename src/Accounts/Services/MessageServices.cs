using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;

namespace Accounts.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private AppSettings _appSettings;

        public AuthMessageSender(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        
        public Task SendEmailAsync(string email, string subject, string message)
        {
            // For more info about e-mail send component https://github.com/jstedfast/MailKit

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Prefeitura de Joinville", "no-reply@joinville.sc.gov.br"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new SmtpClient())
            {
                var credentials = new System.Net.NetworkCredential
                {
                    UserName = _appSettings.MailUser,
                    Password = _appSettings.MailPassword
                };
                
                client.Connect(_appSettings.MailServer, _appSettings.MailPort, false);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                client.Authenticate(credentials);
                client.Send(emailMessage);
                client.Disconnect(true);
            }

            return Task.FromResult(0);
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
