namespace MedTechSupplies.Web.Domain.Entities;

/// <summary>A product a client added to their enquiry basket.</summary>
public class EnquiryItem
{
    public int Id { get; set; }

    public int EnquiryId { get; set; }
    public Enquiry? Enquiry { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
}
