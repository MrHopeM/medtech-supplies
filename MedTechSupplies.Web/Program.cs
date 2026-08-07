using System.Security.Claims;
using MedTechSupplies.Web.Components;
using MedTechSupplies.Web.Data;
using MedTechSupplies.Web.Services;
using MedTechSupplies.Web.Services.Email;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ---------- Blazor ----------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ---------- EF Core (SQLite by default; Azure SQL via config) ----------
var provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        options.UseSqlServer(builder.Configuration.GetConnectionString("AzureSql"));
    else
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                          ?? "Data Source=App_Data/medtech.db");
});

// ---------- Email (SendGrid when configured, else local dev outbox) ----------
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IEmailSender>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<EmailOptions>>().Value;
    var io = sp.GetRequiredService<IOptions<EmailOptions>>();
    var provider = (opts.Provider ?? "").Trim().ToLowerInvariant();

    bool useBrevo = provider == "brevo" || (provider.Length == 0 && !string.IsNullOrWhiteSpace(opts.BrevoApiKey));
    bool useSmtp = provider == "smtp" || (provider.Length == 0 && !string.IsNullOrWhiteSpace(opts.SmtpHost));
    bool useSendGrid = provider == "sendgrid" || (provider.Length == 0 && !string.IsNullOrWhiteSpace(opts.SendGridApiKey));

    if (useBrevo)
        return new BrevoEmailSender(io, sp.GetRequiredService<IHttpClientFactory>(),
            sp.GetRequiredService<ILogger<BrevoEmailSender>>());
    if (useSmtp)
        return new SmtpEmailSender(io, sp.GetRequiredService<ILogger<SmtpEmailSender>>());
    if (useSendGrid)
        return new SendGridEmailSender(io, sp.GetRequiredService<ILogger<SendGridEmailSender>>());
    return new FileEmailSender(sp.GetRequiredService<IWebHostEnvironment>(),
        sp.GetRequiredService<ILogger<FileEmailSender>>());
});

// ---------- App services ----------
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<CartState>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<EnquiryService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();

// ---------- Auth ----------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "MedTechAuth";
        options.LoginPath = "/admin/login";
        options.AccessDeniedPath = "/admin/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ---------- Database init + seed ----------
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "App_Data"));
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await DbSeeder.SeedAsync(db);
}

// ---------- Pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ---------- Auth endpoints (plain form posts) ----------
app.MapPost("/account/login", async (HttpContext http, AuthService auth) =>
{
    var form = await http.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();

    var user = await auth.ValidateAsync(username, password);
    if (user is null)
        return Results.Redirect("/admin/login?error=1");

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, user.Username),
        new(ClaimTypes.GivenName, user.DisplayName),
        new(ClaimTypes.Role, user.Role)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = true });

    return Results.Redirect("/admin");
}).DisableAntiforgery();

app.MapPost("/account/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
}).DisableAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
