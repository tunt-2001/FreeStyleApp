namespace FreeStyleApp.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordEmailAsync(string toEmail, string userName, string newPassword);
    }
}

