using System.Net.Mail;
using System.Net;
using System.Text;

public class EmailSender
{
    public void SendEmail(string toEmail, string subject)
    {
        SmtpClient client = new SmtpClient("smtp-relay.brevo.com", 587);
        client.EnableSsl = true;
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential("expensestracker4@gmail.com", "1kdBMJPFTqmEYwVR");
        MailMessage mailMessage = new MailMessage();
        mailMessage.From = new MailAddress("expensestracker4@gmail.com");
        mailMessage.To.Add(toEmail);
        mailMessage.Subject = "ExpensesTracker";
        mailMessage.IsBodyHtml = true;
        StringBuilder mailBody = new StringBuilder();
        mailBody.AppendFormat("<p>Password Reset</p>");
        mailBody.AppendFormat("<br />");
        mailBody.AppendFormat($"<p>Code:{subject}</p>");
        mailBody.AppendFormat($"<p>Expire after 5 minute</p>");
        mailMessage.Body = mailBody.ToString();
        client.Send(mailMessage);
    }
}
