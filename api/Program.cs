using Microsoft.EntityFrameworkCore;
using SaunaBooking.Api.Data;

//  Create a builder
var builder = WebApplication.CreateBuilder(args);

// Load Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register EF Core with SQLite
builder.Services.AddDbContext<SaunaBookingDbContext>(options =>
    options.UseSqlite(connectionString));

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("https://localhost:5067")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins("https://mymh.dev", "https://www.mymh.dev")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // ADD THIS
        }
    });
});

// Add services to the container
builder.Services.AddControllers();

// Build the app
var app = builder.Build();

// Use Services
app.UseCors("MyCorsPolicy"); // Explicitly apply the named policy

app.MapControllers();

// Development: a root Hello World endpoint for health check
app.MapGet("/", () => Results.Ok("Hello World!"));

app.Run();