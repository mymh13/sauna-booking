var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Health check during development
app.MapMethods("/", new[] { "GET", "HEAD" }, () => Results.Ok("Hello World!"));

app.Run();