using Microsoft.EntityFrameworkCore;
using SaunaBooking.Api.Data;

//  Create a builder
var builder = WebApplication.CreateBuilder(args);

// Load Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register EF Core with SQLite
builder.Services.AddDbContext<SaunaBookingDbContext>(options =>
    options.UseSqlite(connectionString));

// Detect allowed origins dynamically
string[] allowedOrigins = builder.Environment.IsDevelopment()
    ? new[] { "https://localhost:5067" }
    : new[] { "https://mymh.dev", "https://www.mymh.dev" };

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services to the container
builder.Services.AddControllers();

// Build the app
var app = builder.Build();

app.UseRouting();
app.UseCors("MyCorsPolicy");
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

app.Run();