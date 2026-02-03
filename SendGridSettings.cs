namespace CustomerManagement.ExternalServices.Configuration
{
    public class SendGridSettings
    {
        public required string ApiKey { get; set; }
        public required string FromEmail { get; set; }
        public required string FromName { get; set; }
        public string BaseUrl { get; set; } = "https://api.sendgrid.com";
    }
}