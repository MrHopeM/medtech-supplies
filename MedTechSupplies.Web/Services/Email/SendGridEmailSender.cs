using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MedTechSupplies.Web.Services.Email;

/// <summary>Sends email through SendGrid. Used when an API key is configured.</summary>
public class SendGridEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SendGridEmailSender> _logger;

    public SendGridEmailSender(IOptions<EmailOptions> options, ILogger<SendGridEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var client = new SendGridClient(_options.SendGridApiKey);
        var from = new EmailAddress(_options.FromAddress, _options.FromName);
        var to = new EmailAddress(message.ToAddress, message.ToName);
        var msg = MailHelper.CreateSingleEmail(
            from, to, message.Subject,
            message.PlainTextBody ?? string.Empty, message.HtmlBody);

        var response = await client.SendEmailAsync(msg, ct);
        if ((int)response.StatusCode >= 400)
        {
            var body = await response.Body.ReadAsStringAsync(ct);
            _logger.LogError("SendGrid send failed ({Status}) to {To}: {Body}", response.StatusCode, message.ToAddress, body);
        }
        else
        {
            _logger.LogInformation("SendGrid email sent to {To} ({Subject})", message.ToAddress, message.Subject);
        }
    }
}
