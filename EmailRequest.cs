namespace CustomerManagement.ExternalServices.DTOs.SendGrid
{ 
    public class EmailRequest
    {
        public required string ToEmail { get; set; }
        public string? ToName { get; set; }
        public required string Subject { get; set; }
        public required string TextContent { get; set; }
        public string? HtmlContent { get; set; }
    }
}