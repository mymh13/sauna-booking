using Microsoft.EntityFrameworkCore;
using SaunaBooking.Api.Data;

//  Create a builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddDbContext<SaunaBookingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Build the app
var app = builder.Build();

// Map controllers
app.MapControllers();

// Development: a root Hello World endpoint for health check
app.MapGet("/", () => Results.Ok("Hello World!"));

app.Run();