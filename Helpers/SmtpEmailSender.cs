
using Microsoft.AspNetCore.Builder.Extensions;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;
using HShop2024.ViewModels;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _smtpOptions;

    public SmtpEmailSender(SmtpSettings smtpOptions)
    {
        _smtpOptions = smtpOptions ?? throw new ArgumentNullException(nameof(smtpOptions));
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var smtpClient = new SmtpClient(_smtpOptions.Host)
        {
            Port = _smtpOptions.Port,
            Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password),
            EnableSsl = _smtpOptions.EnableSsl,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpOptions.Username),
            Subject = subject,
            Body = message,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(email);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            // Xử lý lỗi gửi email
            Console.WriteLine($"Error sending email: {ex.Message}");
            throw;
        }
    }
}
