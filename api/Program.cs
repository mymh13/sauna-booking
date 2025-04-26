//  Create a builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Build the app
var app = builder.Build();

// Map controllers
app.MapControllers();

// Development: a root Hello World endpoint for health check
app.MapGet("/", () => Results.Ok("Hello World!"));

app.Run();