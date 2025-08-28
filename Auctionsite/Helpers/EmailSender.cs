using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Auctionsite.Helpers
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Task SendEmailAsync(string email, string subject, string content)
        {
            string response = "";
            var message = new MimeMessage();
            message.Sender = MailboxAddress.Parse(_configuration["Authentication:GoogleEmailApp:SenderEmail"]);
            message.Sender.Name = _configuration["Authentication:GoogleEmailApp:SenderName"];
            message.To.Add(MailboxAddress.Parse(email));
            message.From.Add(message.Sender);
            message.Subject = subject;
            //We will say we are sending HTML. But there are options for plaintext etc.
            message.Body = new TextPart(TextFormat.Html) { Text = content };
            using (var emailClient = new SmtpClient())
            {
                try
                {
                    emailClient.Connect(_configuration["Authentication:GoogleEmailApp:SmtpServer"], Convert.ToInt32(_configuration["Authentication:GoogleEmailApp:SmtpPort"]), SecureSocketOptions.StartTls);
                }
                catch (SmtpCommandException ex)
                {
                    response = "Error trying to connect:" + ex.Message + " StatusCode: " + ex.StatusCode;
                    return Task.FromResult(response);
                }
                catch (SmtpProtocolException ex)
                {
                    response = "Protocol error while trying to connect:" + ex.Message;
                    return Task.FromResult(response);
                }
                //Remove any OAuth functionality as we won't be using it.
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(_configuration["Authentication:GoogleEmailApp:SmtpUsername"], _configuration["Authentication:GoogleEmailApp:SmtpPassword"]);
                try
                {
                    emailClient.Send(message);
                }
                catch (SmtpCommandException ex)
                {
                    response = "Error sending message: " + ex.Message + " StatusCode: " + ex.StatusCode;
                    switch (ex.ErrorCode)
                    {
                        case SmtpErrorCode.RecipientNotAccepted:
                            response += " Recipient not accepted: " + ex.Mailbox;
                            break;
                        case SmtpErrorCode.SenderNotAccepted:
                            response += " Sender not accepted: " + ex.Mailbox;
                            Console.WriteLine("\tSender not accepted: {0}", ex.Mailbox);
                            break;
                        case SmtpErrorCode.MessageNotAccepted:
                            response += " Message not accepted.";
                            break;
                    }
                }
                emailClient.Disconnect(true);
            }
            return Task.CompletedTask;
        }
    }
}
