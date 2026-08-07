using MedTechSupplies.Web.Data;
using MedTechSupplies.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedTechSupplies.Web.Services;

public class AuthService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public AuthService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    /// <summary>Validates credentials and returns the user on success, otherwise null.</summary>
    public async Task<AppUser?> ValidateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        await using var db = await _factory.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null) return null;
        return PasswordHasher.Verify(password, user.PasswordHash) ? user : null;
    }
}
