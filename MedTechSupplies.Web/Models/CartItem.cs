namespace MedTechSupplies.Web.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Image { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;

    public decimal LineTotal => Price * Quantity;
}
