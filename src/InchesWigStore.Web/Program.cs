using InchesWigStore.Web.Data;
using InchesWigStore.Web.Models;
using InchesWigStore.Web.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Secrets (real connection strings, Admin:Email, Admin:Password) come from `dotnet user-secrets`
// in development and from environment variables (ConnectionStrings__DefaultConnection,
// Admin__Email, Admin__Password) elsewhere. Never put real values in appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>(IdentityDefaults.Configure)
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<AdminSeeder>();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Migrations are applied automatically in Development only; the deploy step runs
// `dotnet ef database update` everywhere else (NFR-OPS-1).
if (app.Environment.IsDevelopment())
{
    using var migrationScope = app.Services.CreateScope();
    await migrationScope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

// Seed roles and the first administrator only when both credentials are configured.
// The password is never logged.
var adminEmail = app.Configuration["Admin:Email"];
var adminPassword = app.Configuration["Admin:Password"];
if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrEmpty(adminPassword))
{
    using var seedScope = app.Services.CreateScope();
    await seedScope.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync(adminEmail, adminPassword);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = StatusCodes.Status200OK,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = StatusCodes.Status200OK,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
    },
    ResponseWriter = (context, report) =>
    {
        context.Response.ContentType = "text/plain";
        return context.Response.WriteAsync(
            report.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy ? "Unhealthy" : "Healthy");
    },
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

public partial class Program { }
