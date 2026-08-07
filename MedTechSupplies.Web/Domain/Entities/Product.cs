namespace MedTechSupplies.Web.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public string? Brand { get; set; }
    public string? Sku { get; set; }
    public string? Description { get; set; }

    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int ReorderLevel { get; set; } = 15;

    /// <summary>Emoji or image filename used as the product thumbnail in this demo.</summary>
    public string? Image { get; set; }

    public bool Featured { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsLowStock => Stock <= ReorderLevel;
    public bool IsOutOfStock => Stock <= 0;
}
