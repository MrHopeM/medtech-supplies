using MedTechSupplies.Web.Data;
using MedTechSupplies.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedTechSupplies.Web.Services;

public class CatalogService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CatalogService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<List<Category>> GetCategoriesAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Categories
            .Include(c => c.Products.Where(p => p.IsActive))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsAsync(string? categorySlug = null, string? search = null,
        bool featuredOnly = false, string? sort = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var q = db.Products.Include(p => p.Category).Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(categorySlug))
            q = q.Where(p => p.Category != null && p.Category.Slug == categorySlug);

        if (featuredOnly)
            q = q.Where(p => p.Featured);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(p => EF.Functions.Like(p.Name, $"%{s}%")
                          || EF.Functions.Like(p.Description ?? "", $"%{s}%")
                          || EF.Functions.Like(p.Brand ?? "", $"%{s}%"));
        }

        var list = await q.ToListAsync();
        return sort switch
        {
            "name_asc" => list.OrderBy(p => p.Name).ToList(),
            "name_desc" => list.OrderByDescending(p => p.Name).ToList(),
            _ => list.OrderByDescending(p => p.Featured).ThenBy(p => p.Name).ToList()
        };
    }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Products.Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
    }

    public async Task<List<Product>> GetRelatedAsync(Product product, int take = 4)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Products
            .Where(p => p.IsActive && p.CategoryId == product.CategoryId && p.Id != product.Id)
            .Take(take)
            .ToListAsync();
    }
}
