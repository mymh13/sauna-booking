using Microsoft.EntityFrameworkCore;
using SaunaBooking.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

//  Create a builder
var builder = WebApplication.CreateBuilder(args);

// Load Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register EF Core with SQLite
builder.Services.AddDbContext<SaunaBookingDbContext>(options =>
{
    options.UseSqlite(connectionString);
    options.LogTo(Console.WriteLine, LogLevel.Information);
});

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found in configuration");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not found in configuration");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Add Authorization services
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("admin"));
});

// Detect allowed origins dynamically
string[] allowedOrigins = builder.Environment.IsDevelopment()
    ? new[] { "https://localhost:5067" }
    : new[] { "https://mymh.dev", "https://www.mymh.dev", "https://sauna.mymh.dev" };

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Content-Disposition");
    });
});

// Add services to the container
builder.Services.AddControllers();

// Build the app
var app = builder.Build();

// Add startup logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
try
{
    logger.LogInformation("Starting application...");
    logger.LogInformation("Database connection string: {ConnectionString}", connectionString);
    logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
    logger.LogInformation("Allowed Origins: {Origins}", string.Join(", ", allowedOrigins));

    // Log JWT configuration
    logger.LogInformation("JWT Configuration - Issuer: {Issuer}, Audience: {Audience}", 
        builder.Configuration["Jwt:Issuer"],
        builder.Configuration["Jwt:Audience"]);

    // Apply any pending migrations at startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SaunaBookingDbContext>();
        logger.LogInformation("Applying database migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
    }

    // Configure middleware pipeline
    app.UseRouting();
    logger.LogInformation("Routing middleware configured");

    app.UseCors("MyCorsPolicy");
    logger.LogInformation("CORS middleware configured");

    app.UseAuthentication();
    logger.LogInformation("Authentication middleware configured");

    app.UseAuthorization();
    logger.LogInformation("Authorization middleware configured");

    app.MapControllers();
    logger.LogInformation("Controllers mapped");

    // Optional: test endpoint
    app.MapGet("/", () => Results.Ok("Hello World!"));
    app.MapGet("/test", () => "API OK");
    logger.LogInformation("Test endpoints configured");

    // Health check endpoints
    app.MapGet("/db-check", async (SaunaBookingDbContext db) =>
    {
        try
        {
            var count = await db.Bookings.CountAsync();
            return Results.Ok($"Bookings table has {count} rows.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database health check failed");
            return Results.Problem("DB ERROR: " + ex.Message);
        }
    });

    app.MapGet("/migrations", async (SaunaBookingDbContext db) =>
    {
        var migrations = await db.Database.GetAppliedMigrationsAsync();
        return Results.Ok(migrations);
    });

    app.MapGet("/test-cors", () => Results.StatusCode(403)).RequireCors("MyCorsPolicy");

    app.Run();
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during application startup");
    throw; // Re-throw to ensure the application fails fast
}