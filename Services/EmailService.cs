using MailKit.Net.Smtp;
using MimeKit;

public class EmailService
{
    public void SendEmail(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sender", "trungnguyenhs3105@gmail.com"));
        message.To.Add(new MailboxAddress("Recipient", to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using (var client = new SmtpClient())
        {
            client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate("trungnguyenhs3105@gmail.com", "Nguyen31052002");
            client.Send(message);
            client.Disconnect(true);
        }
    }
}
