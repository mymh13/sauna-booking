using Microsoft.EntityFrameworkCore;
using SaunaBooking.Api.Data;

//  Create a builder
var builder = WebApplication.CreateBuilder(args);

// Load Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register EF Core with SQLite
builder.Services.AddDbContext<SaunaBookingDbContext>(options =>
    options.UseSqlite(connectionString));

// Allow CORS - Different for Development vs Production
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("https://localhost:5067")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
}
else
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("https://mymh.dev", "https://www.mymh.dev")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
}

// Add services to the container
builder.Services.AddControllers();

// Build the app
var app = builder.Build();

// Use the Services we added
app.UseCors(); // Must come before MapControllers
app.MapControllers();

// Development: a root Hello World endpoint for health check
app.MapGet("/", () => Results.Ok("Hello World!"));

app.Run();