using FreeStyleApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace FreeStyleApp.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordEmailAsync(string toEmail, string userName, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                throw new ArgumentException("Email người nhận không được để trống.", nameof(toEmail));
            }

            var smtpServer = _configuration["SystemConfiguration:SmtpServer"];
            var smtpPort = int.Parse(_configuration["SystemConfiguration:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["SystemConfiguration:SmtpUsername"];
            var smtpPassword = _configuration["SystemConfiguration:SmtpPassword"];
            var enableSsl = bool.Parse(_configuration["SystemConfiguration:SmtpEnableSsl"] ?? "true");

            var missingConfigs = new List<string>();
            if (string.IsNullOrWhiteSpace(smtpServer))
                missingConfigs.Add("SmtpServer");
            if (string.IsNullOrWhiteSpace(smtpUsername))
                missingConfigs.Add("SmtpUsername");
            if (string.IsNullOrWhiteSpace(smtpPassword))
                missingConfigs.Add("SmtpPassword");

            if (missingConfigs.Any())
            {
                var missingList = string.Join(", ", missingConfigs);
                throw new InvalidOperationException($"Cấu hình SMTP chưa được thiết lập đầy đủ. Vui lòng kiểm tra và điền các giá trị sau trong appsettings.json: {missingList}.");
            }

            try
            {
                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUsername, "FreeStyleApp"),
                    Subject = "Khôi phục mật khẩu - FreeStyleApp",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>Khôi phục mật khẩu</h2>
                            <p>Xin chào <strong>{userName}</strong>,</p>
                            <p>Quản trị viên đã tạo mật khẩu mới cho tài khoản của bạn.</p>
                            <p><strong>Mật khẩu mới của bạn là: <span style='color: #2563eb; font-size: 18px;'>{newPassword}</span></strong></p>
                            <p>Vui lòng đăng nhập và đổi mật khẩu ngay sau khi nhận được email này.</p>
                            <hr>
                            <p style='color: #666; font-size: 12px;'>Đây là email tự động, vui lòng không trả lời email này.</p>
                        </body>
                        </html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Đã gửi email khôi phục mật khẩu đến {Email} cho người dùng {UserName}.", toEmail, userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email khôi phục mật khẩu đến {Email} cho người dùng {UserName}.", toEmail, userName);
                throw new InvalidOperationException($"Không thể gửi email: {ex.Message}", ex);
            }
        }
    }
}

