using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CryptoWallet
{
    public class VerifyEmail
    {
        private readonly string smtpHost;
        private readonly int smtpPort;
        private readonly string senderEmail;
        private readonly string senderPassword;

        public string Code { get; private set; }
        public DateTime ExpiresAt { get; private set; }

        public VerifyEmail(string smtpHost, int smtpPort, string senderEmail, string senderPassword)
        {
            this.smtpHost = smtpHost;
            this.smtpPort = smtpPort;
            this.senderEmail = senderEmail;
            this.senderPassword = senderPassword;
        }

        public string GenerateCode()
        {
            var rnd = new Random();
            Code = rnd.Next(100000, 999999).ToString();
            ExpiresAt = DateTime.Now.AddMinutes(10);
            return Code;
        }

        public bool IsValid(string inputCode)
        {
            if (DateTime.Now > ExpiresAt)
                return false;

            return Code == inputCode?.Trim();
        }

        public async Task<(bool Ok, string Error)> SendCodeAsync(string toEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Code)) GenerateCode();

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(senderEmail);
                    message.To.Add(toEmail);
                    message.Subject = "CryptoWallet verification code";
                    message.Body = $"Your verification code: {Code}";
                    message.IsBodyHtml = false;

                    using (var client = new SmtpClient(smtpHost, smtpPort))
                    {
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;

                        await client.SendMailAsync(message);
                    }
                }

                return (true, null);
            }
            catch (SmtpException ex)
            {
                return (false, $"SMTP ошибка: {ex.StatusCode}\n{ex.Message}\n{ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка отправки: {ex.Message}");
            }
        }

        public void Reset()
        {
            Code = null;
            ExpiresAt = DateTime.MinValue;
        }
    }
}