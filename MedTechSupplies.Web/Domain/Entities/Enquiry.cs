namespace MedTechSupplies.Web.Domain.Entities;

public class Enquiry
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Type { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = "New"; // New | Contacted | Quoted | Closed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Products the client added to their enquiry basket (empty for a plain contact message).</summary>
    public ICollection<EnquiryItem> Items { get; set; } = new List<EnquiryItem>();
}
