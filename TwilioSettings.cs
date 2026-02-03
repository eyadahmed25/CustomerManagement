namespace CustomerManagement.ExternalServices.Configuration
{
    public class TwilioSettings
    {
        public required string AccountSid { get; set; }
        public required string AuthToken { get; set; }
        public required string BaseUrl { get; set; } = "https://lookups.twilio.com";
    }
}