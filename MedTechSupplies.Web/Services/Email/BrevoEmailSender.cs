using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace MedTechSupplies.Web.Services.Email;

/// <summary>
/// Sends email through the Brevo (formerly Sendinblue) HTTP API.
/// Used when Email:Provider = "Brevo" or a BrevoApiKey is set. No SMTP details needed —
/// just a Brevo account, a verified sender, and an API key.
/// </summary>
public class BrevoEmailSender : IEmailSender
{
    private const string Endpoint = "https://api.brevo.com/v3/smtp/email";
    private readonly EmailOptions _o;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<BrevoEmailSender> _logger;

    public BrevoEmailSender(IOptions<EmailOptions> options, IHttpClientFactory httpFactory, ILogger<BrevoEmailSender> logger)
    {
        _o = options.Value;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var payload = new
        {
            sender = new { name = _o.FromName, email = _o.FromAddress },
            to = new[] { new { email = message.ToAddress, name = message.ToName } },
            subject = message.Subject,
            htmlContent = message.HtmlBody,
            textContent = string.IsNullOrWhiteSpace(message.PlainTextBody) ? null : message.PlainTextBody
        };

        var client = _httpFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        req.Headers.Add("api-key", _o.BrevoApiKey);
        req.Headers.Add("accept", "application/json");
        req.Content = JsonContent.Create(payload);

        var res = await client.SendAsync(req, ct);
        if (res.IsSuccessStatusCode)
        {
            _logger.LogInformation("Brevo email sent to {To} ({Subject})", message.ToAddress, message.Subject);
        }
        else
        {
            var body = await res.Content.ReadAsStringAsync(ct);
            _logger.LogError("Brevo send failed ({Status}) to {To}: {Body}", res.StatusCode, message.ToAddress, body);
        }
    }
}
