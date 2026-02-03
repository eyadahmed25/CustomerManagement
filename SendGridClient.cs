using CustomerManagement.ExternalServices.Clients;
using CustomerManagement.ExternalServices.Configuration;
using CustomerManagement.ExternalServices.DTOs.SendGrid;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace CustomerManagement.ExternalServices.Clients
{
    public class SendGridClient : ISendGridClient
    {
        private readonly SendGridSettings _settings;
        private readonly HttpClient _httpClient;

        public SendGridClient(IOptions<SendGridSettings> settings, HttpClient httpClient)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<EmailResponse> SendWelcomeEmailAsync(string toEmail, string firstName)
        {
            var emailRequest = new EmailRequest
            {
                ToEmail = toEmail,
                ToName = firstName,
                Subject = "Welcome to Customer Management!",
                TextContent = $"Hello {firstName},\n\n" +
                             $"Welcome to our Customer Management system! We're excited to have you on board.\n\n" +
                             $"Thank you for creating an account with us. If you have any questions, please don't hesitate to reach out.\n\n" +
                             $"Best regards,\n" +
                             $"The Customer Management Team",
                HtmlContent = $"<html><body>" +
                             $"<h2>Hello {firstName},</h2>" +
                             $"<p>Welcome to our Customer Management system! We're excited to have you on board.</p>" +
                             $"<p>Thank you for creating an account with us. If you have any questions, please don't hesitate to reach out.</p>" +
                             $"<p>Best regards,<br>The Customer Management Team</p>" +
                             $"</body></html>"
            };
            return await SendEmailAsync(emailRequest);
        }

        public async Task<EmailResponse> SendEmailAsync (EmailRequest request)
        {
            try
            {
                var sendGridPayload = new
                {
                    personalizations = new[]
                    { 
                        new
                        {
                            to = new[]
                            {
                                new
                                {
                                    email = request.ToEmail,
                                    name = request.ToName
                                }
                            }
                        }
                    },
                    from = new
                    {
                        email = _settings.FromEmail,
                        name = _settings.FromName
                    },
                    subject = request.Subject,
                    content = new[]
                    {
                        new
                        {
                            type = "text/plain",
                            value = request.TextContent
                        },
                        request.HtmlContent != null ?
                            new { type = "text/html", value = request.HtmlContent } :
                            null
                    }.Where(c => c != null).ToArray()
                };

                var jsonContent = JsonSerializer.Serialize(sendGridPayload);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/v3/mail/send", httpContent);

                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    return new EmailResponse
                    {
                        IsSuccess = true,
                        StatusCode = (int)response.StatusCode,
                        MessageId = response.Headers.Contains("X-Message-Id")
                            ? response.Headers.GetValues("X-Message-Id").FirstOrDefault()
                            : null
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    return new EmailResponse
                    {
                        IsSuccess = false,
                        StatusCode = (int)response.StatusCode,
                        ErrorMessage = $"SendGrid API error: {response.StatusCode}. {errorContent}"
                    };
                }

            }
            catch (HttpRequestException httpEx)
            {
                return new EmailResponse
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    ErrorMessage = $"Network error calling SendGrid: {httpEx.Message}"
                };
            }
            catch (Exception ex)
            {
                return new EmailResponse
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    ErrorMessage = $"Unexpected error sending email: {ex.Message}"
                };
            }
        }
    }
}