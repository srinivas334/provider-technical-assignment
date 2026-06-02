using Microsoft.EntityFrameworkCore;
using ProviderAssignmentStarter.Data;
using ProviderAssignmentStarter.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDev", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IProviderService, ProviderService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Idempotent: add AuditLogs table and indexes if not present (supports existing DBs)
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS AuditLogs (
            AuditLogId    INTEGER PRIMARY KEY AUTOINCREMENT,
            EntityType    TEXT NOT NULL,
            EntityId      INTEGER NOT NULL,
            Action        TEXT NOT NULL,
            ChangeSummary TEXT NOT NULL,
            PerformedAt   TEXT NOT NULL,
            PerformedBy   TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_AuditLogs_EntityType_EntityId ON AuditLogs (EntityType, EntityId);
        CREATE INDEX IF NOT EXISTS IX_AuditLogs_PerformedAt ON AuditLogs (PerformedAt);
    ");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("ReactDev");
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Serve the React SPA at /dashboard
app.MapGet("/dashboard", async (IWebHostEnvironment env) =>
    Results.File(Path.Combine(env.WebRootPath, "index.html"), "text/html"));

app.Run();
