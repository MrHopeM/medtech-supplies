namespace MedTechSupplies.Web.Services.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string FromName { get; set; } = "MedTech Supplies";
    public string FromAddress { get; set; } = "enquire@medtechsupplies.co.za";
    public string SupplierName { get; set; } = "MedTech Supplies";
    public string SupplierAddress { get; set; } = "enquire@medtechsupplies.co.za";

    /// <summary>"Brevo", "Smtp", "SendGrid", or "Outbox". Blank = auto-detect from the settings provided.</summary>
    public string Provider { get; set; } = "";

    // --- Brevo (HTTP API — no SMTP details needed) ---
    public string BrevoApiKey { get; set; } = string.Empty;

    // --- SendGrid ---
    public string SendGridApiKey { get; set; } = string.Empty;

    // --- SMTP (any provider: your domain host, Brevo, Gmail app password, etc.) ---
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool SmtpUseSsl { get; set; } = true;
}
