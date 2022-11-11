using LibraryAPI.Domain;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly string email;
        private readonly string password;

        public EmailService(string email, string password)
        {
            this.email = email;
            this.password = password;            
        }

        public void SendEmail(string to, string subject, string body)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(email, email));
            msg.To.Add(new MailboxAddress(to, to));
            msg.Subject = subject;
            msg.Body = new TextPart("plain") { Text = body };

            using(var client = new SmtpClient())
            {
                client.Connect("smtp.mail.us-east-1.awsapps.com", 465, true);
                client.Authenticate(email, password);
                client.Send(msg);
                client.Disconnect(true);
            }
        }
    }
}
