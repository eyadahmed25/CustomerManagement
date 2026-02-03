using CustomerManagement.ExternalServices.DTOs.SendGrid;

namespace CustomerManagement.ExternalServices.Clients
{
    public interface ISendGridClient
    {
        Task<EmailResponse> SendWelcomeEmailAsync(string toEmail, string firstName);
        Task<EmailResponse> SendEmailAsync(EmailRequest request);
    }
}