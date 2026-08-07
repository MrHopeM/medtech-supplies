namespace MedTechSupplies.Web.Services.Email;

/// <summary>
/// Development fallback: writes each email as an .html file to App_Data/outbox
/// so the order/receipt flow can be demonstrated without a SendGrid key.
/// </summary>
public class FileEmailSender : IEmailSender
{
    private readonly string _outboxPath;
    private readonly ILogger<FileEmailSender> _logger;

    public FileEmailSender(IWebHostEnvironment env, ILogger<FileEmailSender> logger)
    {
        _logger = logger;
        _outboxPath = Path.Combine(env.ContentRootPath, "App_Data", "outbox");
        Directory.CreateDirectory(_outboxPath);
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var safeTo = string.Concat(message.ToAddress.Split(Path.GetInvalidFileNameChars()));
        var fileName = $"{DateTime.Now:yyyyMMdd-HHmmss-fff}_{safeTo}.html";
        var fullPath = Path.Combine(_outboxPath, fileName);

        var contents = $"""
            <!-- TO: {message.ToName} <{message.ToAddress}> -->
            <!-- SUBJECT: {message.Subject} -->
            {message.HtmlBody}
            """;

        await File.WriteAllTextAsync(fullPath, contents, ct);
        _logger.LogInformation("[DEV OUTBOX] Email to {To} saved → {Path}", message.ToAddress, fullPath);
    }
}
