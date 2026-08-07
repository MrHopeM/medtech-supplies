using MedTechSupplies.Web.Data;
using MedTechSupplies.Web.Domain.Entities;
using MedTechSupplies.Web.Models;
using MedTechSupplies.Web.Services.Email;
using Microsoft.EntityFrameworkCore;

namespace MedTechSupplies.Web.Services;

public class EnquiryService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly EmailService _email;

    public EnquiryService(IDbContextFactory<AppDbContext> factory, EmailService email)
    {
        _factory = factory;
        _email = email;
    }

    /// <summary>Submits an enquiry basket: persists it with line items and emails the team + client.</summary>
    public async Task<Enquiry> SubmitAsync(EnquiryForm form, IReadOnlyList<CartItem> items, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var enquiry = new Enquiry
        {
            Name = form.Name,
            Email = form.Email,
            Phone = form.Phone,
            Company = form.Company,
            Type = form.Type,
            Subject = items.Count > 0 ? $"Enquiry basket ({items.Count} item{(items.Count == 1 ? "" : "s")})" : "Website enquiry",
            Message = form.Message,
            Status = "New"
        };
        foreach (var i in items)
            enquiry.Items.Add(new EnquiryItem { ProductId = i.ProductId, Name = i.Name, Quantity = i.Quantity });

        db.Enquiries.Add(enquiry);
        await db.SaveChangesAsync(ct);

        await _email.SendEnquiryEmailsAsync(enquiry, ct);
        return enquiry;
    }

    /// <summary>A plain contact-form message (no basket).</summary>
    public async Task CreateMessageAsync(EnquiryForm form, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var enquiry = new Enquiry
        {
            Name = form.Name,
            Email = form.Email,
            Phone = form.Phone,
            Company = form.Company,
            Subject = "Website message",
            Message = form.Message,
            Status = "New"
        };
        db.Enquiries.Add(enquiry);
        await db.SaveChangesAsync(ct);
        await _email.SendEnquiryEmailsAsync(enquiry, ct);
    }

    public async Task<List<Enquiry>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Enquiries
            .Include(e => e.Items)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var e = await db.Enquiries.FindAsync(id);
        if (e is null) return;
        e.Status = status;
        await db.SaveChangesAsync();
    }
}
