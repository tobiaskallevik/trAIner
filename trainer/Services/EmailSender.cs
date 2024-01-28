using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration; 

namespace trainer.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
        ILogger<EmailSender> logger, IConfiguration configuration) 
    {
        Options = optionsAccessor.Value;
        _logger = logger;
        _configuration = configuration; 
    }

    public AuthMessageSenderOptions Options { get; } 

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var apiKey = _configuration.GetSection("SendGridSettings:ApiKey").Value; 
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("Null SendGridKey");
        }
        await Execute(apiKey, subject, message, toEmail);
    }

    public async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress("ikt201project@gmail.com", "Email Verification"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        // Disable click tracking.
        // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        msg.SetClickTracking(false, false);
        
        var response = await client.SendEmailAsync(msg);
        
        _logger.LogInformation(response.IsSuccessStatusCode 
            ? $"Email to {toEmail} queued successfully!"
            : $"Failure Email to {toEmail}");
    }
}
