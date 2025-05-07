using Microsoft.EntityFrameworkCore;
using SaunaBooking.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

//  Create a builder
var builder = WebApplication.CreateBuilder(args);

// Load Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register EF Core with SQLite
builder.Services.AddDbContext<SaunaBookingDbContext>(options =>
    options.UseSqlite(connectionString));

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found in configuration");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not found in configuration");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
logger.LogInformation("Starting application...");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Allowed Origins: {Origins}", string.Join(", ", allowedOrigins));

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
app.UseCors("MyCorsPolicy"); // CORS must be after UseRouting but before UseAuthentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Optional: test endpoint
app.MapGet("/", () => Results.Ok("Hello World!"));
app.MapGet("/test", () => "API OK");

app.MapPost("/debug-booking", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    Console.WriteLine("Raw body: " + body);
    return Results.Ok("Debug endpoint received something");
});

app.MapGet("/db-check", async (SaunaBookingDbContext db) =>
{
    try
    {
        var count = await db.Bookings.CountAsync();
        return Results.Ok($"Bookings table has {count} rows.");
    }
    catch (Exception ex)
    {
        return Results.Problem("DB ERROR: " + ex.Message);
    }
});

app.MapGet("/migrations", async (SaunaBookingDbContext db) =>
{
    var migrations = await db.Database.GetAppliedMigrationsAsync();
    return Results.Ok(migrations);
});

app.Run();