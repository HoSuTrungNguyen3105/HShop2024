using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using HShop2024.Models;
using HShop2024.Controllers;
using MailKit.Security;
using MailKit.Net.Smtp;
using MimeKit.Text;
using MimeKit;
using System.Net.Mail;
using System.Net;

namespace ASP.NET_Web_App_NetFramework_1.Services
{
    public static class EmailService
    {
        private static string _Host = "lsmtp.gamil.com";
        private static int _Port = 587;

        private static string _From = "Unknow 101";
        // This email is our
        private static string _Email = "[your email]";
        // The password is app password
        private static string _Password = "[your app password]";


        // Send Method
        //public static bool Send(EmailDTO emailDTO)
        //{
        //    try
        //    {
        //        var email = new MimeMessage();

        //        // From
        //        email.From.Add(new MailboxAddress(_From, _Email));
        //        // To
        //        email.To.Add(MailboxAddress.Parse(emailDTO.To));
        //        // Subject
        //        email.Subject = emailDTO.Subject;
        //        // Body
        //        email.Body = new TextPart(TextFormat.Html)
        //        {
        //            Text = emailDTO.Body
        //        };
        //        var smtp = new SmtpClient("smtp.gmail.com", 587)
        //        {
        //            Credentials = new NetworkCredential("trungnguyenhs3105@gmail.com", "Nguyen31052002"),
        //            EnableSsl = true
        //        };

        //        // Conect the SMTP with the host
        //        smtp.Connect(_Host, _Port, SecureSocketOptions.StartTls);

        //        // Authenticate
        //        smtp.Authenticate(_Email, _Password);
        //        smtp.Send(email);
        //        smtp.Disconnect(true);

        //        return true;

        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
    }
}