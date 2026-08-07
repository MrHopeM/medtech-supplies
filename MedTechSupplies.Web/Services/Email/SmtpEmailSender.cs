using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace MedTechSupplies.Web.Services.Email;

/// <summary>
/// Sends email over standard SMTP — works with any provider (your domain mailbox host,
/// Brevo, Gmail app password, etc.). Used when Email:Provider = "Smtp" or an SmtpHost is set.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _o;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _o = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        using var mail = new MailMessage
        {
            From = new MailAddress(_o.FromAddress, _o.FromName),
            Subject = message.Subject,
            Body = message.HtmlBody,
            IsBodyHtml = true
        };
        mail.To.Add(new MailAddress(message.ToAddress, message.ToName));
        if (!string.IsNullOrWhiteSpace(message.PlainTextBody))
        {
            mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                message.PlainTextBody, null, "text/plain"));
            mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                message.HtmlBody, null, "text/html"));
        }

        using var client = new SmtpClient(_o.SmtpHost, _o.SmtpPort)
        {
            EnableSsl = _o.SmtpUseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = string.IsNullOrWhiteSpace(_o.SmtpUsername)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_o.SmtpUsername, _o.SmtpPassword)
        };

        try
        {
            await client.SendMailAsync(mail, ct);
            _logger.LogInformation("SMTP email sent to {To} ({Subject})", message.ToAddress, message.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP send failed to {To} via {Host}:{Port}", message.ToAddress, _o.SmtpHost, _o.SmtpPort);
            throw;
        }
    }
}
