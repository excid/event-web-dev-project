using Microsoft.EntityFrameworkCore;
using event_web_dev_project.Data;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Identity;
using event_web_dev_project.Models;
using event_web_dev_project.Services;
using Microsoft.AspNetCore.DataProtection;

// Add this at the very top of Program.cs before anything else
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Fix 1: EnableRetryOnFailure — stops the app from crashing if SQL Server
// isn't fully ready yet when the app starts (common in Docker)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(10),
        errorCodesToAdd: null
        )
    ));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login/Index";
});

builder.Services.AddHostedService<ExpiryCheckerService>();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/src/DataProtection-Keys"));

var app = builder.Build();

// Fix 2: Auto-migrate on startup — creates the database and applies all
// migrations automatically. No manual "dotnet ef database update" needed.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Seeding User during runtime
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var adminEmail = "sarah@example.com";
    var user = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
    if (user == null)
    {
        user = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            DisplayName = "Sarah Chen",
            About = "Software Engineer and Football enthusiast. Love organizing local sports events!",
            Tags = "Software,Sports,Organizer",
            Interests = "Sports,Dining"
        };
        userManager.CreateAsync(user, "Sarah@123").GetAwaiter().GetResult();
    }
}

var cultureInfo = new CultureInfo("en-US");

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureInfo),
    SupportedCultures = new[] { cultureInfo },
    SupportedUICultures = new[] { cultureInfo }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ORDER MATTERS — Authentication must come before Authorization
app.UseAuthentication();  // ← this was missing from your original file!
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();