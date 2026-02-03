namespace CustomerManagement.ExternalServices.DTOs.Twilio
{
    public class PhoneValidationResponse
    {
        public bool IsValid { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CountryCode { get; set; }
        public string? Carrier { get; set; }
        public string? ErrorMessage { get; set; }
    }
}