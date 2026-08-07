using MedTechSupplies.Web.Domain.Entities;
using Microsoft.Extensions.Options;

namespace MedTechSupplies.Web.Services.Email;

/// <summary>Composes and sends the emails for a submitted client enquiry.</summary>
public class EmailService
{
    private readonly IEmailSender _sender;
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IEmailSender sender, IOptions<EmailOptions> options, ILogger<EmailService> logger)
    {
        _sender = sender;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEnquiryEmailsAsync(Enquiry enquiry, CancellationToken ct = default)
    {
        try
        {
            await _sender.SendAsync(BuildTeamNotification(enquiry), ct);

            if (!string.IsNullOrWhiteSpace(enquiry.Email))
                await _sender.SendAsync(BuildClientAcknowledgement(enquiry), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed sending enquiry emails for enquiry {Id}", enquiry.Id);
        }
    }

    private static string Reference(Enquiry e) => $"ENQ-{e.Id:D5}";

    // ---- Shared building blocks (table-based, inline styles for email-client compatibility) ----

    private string Shell(string preheader, string innerHtml) => $"""
        <div style="background:#eef3f8;padding:24px 12px;font-family:Arial,Helvetica,sans-serif;">
          <span style="display:none;max-height:0;overflow:hidden;opacity:0">{preheader}</span>
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:600px;margin:0 auto">
            <tr><td style="background:linear-gradient(135deg,#103a5a,#2BA9E0);padding:22px 28px;border-radius:14px 14px 0 0">
              <span style="color:#fff;font-size:21px;font-weight:bold;letter-spacing:.5px">MEDTECH <span style="color:#9ade4f">SUPPLIES</span></span>
            </td></tr>
            <tr><td style="background:#ffffff;border:1px solid #e3e9f0;border-top:none;padding:30px 28px;color:#13283a">
              {innerHtml}
            </td></tr>
            <tr><td style="background:#103a5a;border-radius:0 0 14px 14px;padding:18px 28px">
              <p style="margin:0;color:#aec3d8;font-size:12px;line-height:1.7">
                MedTech Supplies (Pty) Ltd · SAHPRA registered &amp; licensed<br>
                3 Halifax Street, Bryanston, Johannesburg, 2191<br>
                +27 67 618 5543 · enquire@medtechsupplies.co.za
              </p>
            </td></tr>
          </table>
        </div>
        """;

    private static string ItemsTable(Enquiry enquiry)
    {
        if (enquiry.Items.Count == 0)
            return "<p style='color:#5e6e80;font-size:14px;margin:0 0 8px'>General enquiry — no specific products listed.</p>";

        var rows = string.Concat(enquiry.Items.Select(i => $"""
            <tr>
              <td style="padding:11px 14px;border-top:1px solid #eef1f5;font-size:14px;color:#13283a">{i.Name}</td>
              <td style="padding:11px 14px;border-top:1px solid #eef1f5;font-size:14px;font-weight:bold;text-align:right;white-space:nowrap">{i.Quantity}</td>
            </tr>
        """));

        return $"""
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse;border:1px solid #eef1f5;border-radius:10px;overflow:hidden">
              <tr style="background:#f4f8fb">
                <th align="left" style="padding:10px 14px;font-size:11px;text-transform:uppercase;letter-spacing:.5px;color:#5e6e80">Product</th>
                <th align="right" style="padding:10px 14px;font-size:11px;text-transform:uppercase;letter-spacing:.5px;color:#5e6e80">Qty</th>
              </tr>
              {rows}
            </table>
            """;
    }

    // ---- Client acknowledgement ----

    private EmailMessage BuildClientAcknowledgement(Enquiry enquiry)
    {
        var inner = $"""
            <table role="presentation" cellpadding="0" cellspacing="0" style="margin:0 0 18px">
              <tr><td style="background:#e6f6ec;color:#1aa179;font-weight:bold;font-size:13px;padding:8px 14px;border-radius:999px">✓ Enquiry received</td></tr>
            </table>
            <h2 style="margin:0 0 8px;color:#103a5a;font-size:21px">Thank you, {enquiry.Name}!</h2>
            <p style="color:#5e6e80;font-size:14.5px;line-height:1.6;margin:0 0 22px">
              We've received your enquiry and our team is on it. Below is a summary of the products you're interested in —
              we'll be in touch within one business day with availability and the best pricing for your needs.
            </p>

            <p style="margin:0 0 8px;font-size:12px;color:#9aa7ba">REFERENCE</p>
            <p style="margin:0 0 20px;font-size:15px;font-weight:bold;color:#103a5a">{Reference(enquiry)}</p>

            <h3 style="color:#103a5a;font-size:15px;margin:0 0 10px">Products you enquired about</h3>
            {ItemsTable(enquiry)}

            <h3 style="color:#103a5a;font-size:15px;margin:26px 0 10px">What happens next</h3>
            <table role="presentation" cellpadding="0" cellspacing="0" style="font-size:14px;color:#3d4d5e;line-height:1.5">
              <tr><td style="padding:4px 0"><strong style="color:#2BA9E0">1.</strong>&nbsp; We source your items from our trusted wholesaler network.</td></tr>
              <tr><td style="padding:4px 0"><strong style="color:#2BA9E0">2.</strong>&nbsp; We send you a tailored quote with availability and lead time.</td></tr>
              <tr><td style="padding:4px 0"><strong style="color:#2BA9E0">3.</strong>&nbsp; You approve and we deliver to your facility.</td></tr>
            </table>

            <p style="margin:24px 0 0;font-size:14px;color:#5e6e80">
              Need to add something or have a question? Just reply to this email or call us on
              <a href="tel:+27676185543" style="color:#1E88C7;text-decoration:none">+27 67 618 5543</a>.
            </p>
            """;

        return new EmailMessage
        {
            ToName = enquiry.Name,
            ToAddress = enquiry.Email!,
            Subject = $"We've received your enquiry ({Reference(enquiry)}) — MedTech Supplies",
            HtmlBody = Shell("Your enquiry has been received — we'll be in touch shortly.", inner),
            PlainTextBody = $"Hi {enquiry.Name}, thank you for your enquiry ({Reference(enquiry)}). "
                + $"Items: {string.Join(", ", enquiry.Items.Select(i => $"{i.Name} x{i.Quantity}"))}. "
                + "Our team will contact you within one business day with availability and pricing."
        };
    }

    // ---- Team notification ----

    private EmailMessage BuildTeamNotification(Enquiry enquiry)
    {
        string Row(string label, string? value) => string.IsNullOrWhiteSpace(value) ? "" : $"""
            <tr>
              <td style="padding:6px 0;font-size:13px;color:#9aa7ba;width:90px;vertical-align:top">{label}</td>
              <td style="padding:6px 0;font-size:14px;color:#13283a;font-weight:600">{value}</td>
            </tr>
        """;

        var contactPhone = string.IsNullOrWhiteSpace(enquiry.Phone) ? null
            : $"<a href=\"tel:{enquiry.Phone}\" style=\"color:#1E88C7;text-decoration:none\">{enquiry.Phone}</a>";
        var contactEmail = string.IsNullOrWhiteSpace(enquiry.Email) ? null
            : $"<a href=\"mailto:{enquiry.Email}\" style=\"color:#1E88C7;text-decoration:none\">{enquiry.Email}</a>";

        var inner = $"""
            <table role="presentation" cellpadding="0" cellspacing="0" style="margin:0 0 18px">
              <tr><td style="background:#fff4e5;color:#9a6a00;font-weight:bold;font-size:13px;padding:8px 14px;border-radius:999px">Action required — follow up</td></tr>
            </table>
            <h2 style="margin:0 0 4px;color:#103a5a;font-size:21px">New client enquiry</h2>
            <p style="color:#5e6e80;font-size:14px;margin:0 0 22px">Reference {Reference(enquiry)} · {enquiry.CreatedAt.ToLocalTime():dd MMM yyyy, HH:mm}</p>

            <h3 style="color:#103a5a;font-size:15px;margin:0 0 8px">Client</h3>
            <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%;margin:0 0 22px">
              {Row("Name", enquiry.Name)}
              {Row("Company", enquiry.Company)}
              {Row("Type", enquiry.Type)}
              {Row("Email", contactEmail)}
              {Row("Phone", contactPhone)}
            </table>

            <h3 style="color:#103a5a;font-size:15px;margin:0 0 10px">Items of interest</h3>
            {ItemsTable(enquiry)}

            {(string.IsNullOrWhiteSpace(enquiry.Message) ? "" : $"""
              <h3 style="color:#103a5a;font-size:15px;margin:24px 0 8px">Message</h3>
              <div style="background:#f4f8fb;border-radius:10px;padding:14px 16px;font-size:14px;color:#3d4d5e;line-height:1.6">{enquiry.Message}</div>
            """)}

            {(string.IsNullOrWhiteSpace(enquiry.Email) ? "" : $"""
              <table role="presentation" cellpadding="0" cellspacing="0" style="margin:24px 0 0">
                <tr><td style="background:#2BA9E0;border-radius:8px"><a href="mailto:{enquiry.Email}?subject=Your MedTech Supplies enquiry {Reference(enquiry)}" style="display:inline-block;padding:11px 22px;color:#fff;font-size:14px;font-weight:bold;text-decoration:none">Reply to client</a></td></tr>
              </table>
            """)}
            """;

        return new EmailMessage
        {
            ToName = _options.SupplierName,
            ToAddress = _options.SupplierAddress,
            Subject = $"New enquiry — {enquiry.Name} ({enquiry.Items.Count} item{(enquiry.Items.Count == 1 ? "" : "s")}) [{Reference(enquiry)}]",
            HtmlBody = Shell($"New enquiry from {enquiry.Name} — {enquiry.Items.Count} item(s)", inner),
            PlainTextBody = $"New enquiry {Reference(enquiry)} from {enquiry.Name} ({enquiry.Email}). "
                + $"Items: {string.Join(", ", enquiry.Items.Select(i => $"{i.Name} x{i.Quantity}"))}."
        };
    }
}
