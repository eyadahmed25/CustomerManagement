using CustomerManagement.ExternalServices.Clients;
using CustomerManagement.ExternalServices.Configuration;
using CustomerManagement.ExternalServices.DTOs.Twilio;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CustomerManagement.ExternalServices.Clients
{
    public class TwilioClient : ITwilioClient
    {
        private readonly TwilioSettings _settings;
        private readonly HttpClient _httpClient;

        public TwilioClient(IOptions<TwilioSettings> settings, HttpClient httpClient)
        {
            _settings = settings.Value;
            _httpClient = httpClient;

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            
            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.AccountSid}:{_settings.AuthToken}"));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        }

        public async Task<PhoneValidationResponse> ValidatePhoneAsync(string phoneNumber)
        {
            try
            {
                var endpoint = $"/v1/PhoneNumbers/{Uri.EscapeDataString(phoneNumber)}";
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Twilio Response: {content}");
                    var twilioResponse = JsonSerializer.Deserialize<JsonElement>(content);
                    return new PhoneValidationResponse
                    {
                        IsValid = true,
                        PhoneNumber = twilioResponse.GetProperty("phone_number").GetString(),
                        CountryCode = twilioResponse.GetProperty("country_code").GetString(),
                        Carrier = twilioResponse.TryGetProperty("carrier", out var carrier)
                                  && carrier.ValueKind != JsonValueKind.Null
                                  && carrier.TryGetProperty("name", out var name)
                                ? name.GetString()
                                : null,
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new PhoneValidationResponse
                    {
                        IsValid = false,
                        ErrorMessage = $"Twilio Validation failed: {response.StatusCode}. {errorContent}"
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                return new PhoneValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = $"HTTP Request Error: {httpEx.Message}"
                };
            }
            catch (Exception ex)
            {
                return new PhoneValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = $"Unexpected error during phone validation: {ex.Message}"
                };
            }
        }
    }
}