using CustomerManagement.ExternalServices.DTOs.Twilio;

namespace CustomerManagement.ExternalServices.Clients
{
    public interface ITwilioClient
    {
        Task<PhoneValidationResponse> ValidatePhoneAsync(string phoneNumber);
    }
}