using MedTechSupplies.Web.Domain.Entities;
using MedTechSupplies.Web.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace MedTechSupplies.Web.Services;

/// <summary>
/// Per-circuit shopping cart, persisted to the browser's protected local storage
/// so it survives page refreshes. Call <see cref="InitializeAsync"/> from
/// OnAfterRenderAsync(firstRender) before reading state.
/// </summary>
public class CartState
{
    private const string StorageKey = "medtech_cart";
    private readonly ProtectedLocalStorage _storage;
    private bool _loaded;

    public List<CartItem> Items { get; private set; } = new();
    public event Action? OnChange;

    public CartState(ProtectedLocalStorage storage) => _storage = storage;

    public int Count => Items.Sum(i => i.Quantity);
    public decimal Total => Items.Sum(i => i.LineTotal);
    public bool IsEmpty => Items.Count == 0;

    public async Task InitializeAsync()
    {
        if (_loaded) return;
        _loaded = true;
        try
        {
            var result = await _storage.GetAsync<List<CartItem>>(StorageKey);
            if (result.Success && result.Value is not null)
                Items = result.Value;
        }
        catch { /* storage unavailable during prerender — ignore */ }
        NotifyChanged();
    }

    public async Task AddAsync(Product product, int quantity = 1)
    {
        var existing = Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing is not null)
            existing.Quantity += quantity;
        else
            Items.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Image = product.Image,
                Price = product.Price,
                Quantity = quantity
            });
        await PersistAsync();
    }

    public async Task SetQuantityAsync(int productId, int quantity)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return;
        if (quantity <= 0) Items.Remove(item);
        else item.Quantity = quantity;
        await PersistAsync();
    }

    public async Task RemoveAsync(int productId)
    {
        Items.RemoveAll(i => i.ProductId == productId);
        await PersistAsync();
    }

    public async Task ClearAsync()
    {
        Items = new();
        await PersistAsync();
    }

    private async Task PersistAsync()
    {
        try { await _storage.SetAsync(StorageKey, Items); }
        catch { /* ignore during prerender */ }
        NotifyChanged();
    }

    private void NotifyChanged() => OnChange?.Invoke();
}
