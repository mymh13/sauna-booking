using Microsoft.AspNetCore.Mvc;
using SaunaBooking.V2.Models;
using SaunaBooking.V2.Rendering;
using SaunaBooking.V2.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DemoDataStore>();

var app = builder.Build();

app.UsePathBase("/sauna/v2");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

var cookieOptions = new CookieOptions
{
    Path = "/sauna/v2",
    HttpOnly = true,
    SameSite = SameSiteMode.Lax,
    IsEssential = true,
    Secure = !app.Environment.IsDevelopment()
};

app.MapGet("/health", () => Results.Ok("OK"));

app.MapGet("/fragments/nav", (HttpContext context) =>
    Results.Content(
        V2Html.RenderNav(GetCurrentUser(context), GetCurrentRole(context)),
        "text/html; charset=utf-8"));

app.MapGet("/fragments/calendar-shell", (HttpContext context, DemoDataStore store, int weekOffset = 0) =>
{
    var currentUser = GetCurrentUser(context);
    var currentRole = GetCurrentRole(context);
    var anchorDate = DateOnly.FromDateTime(DateTime.Today).AddDays(weekOffset * 7);
    var week = store.BuildWeek(anchorDate, currentUser);
    var panel = new BookingPanelVm(
        false,
        anchorDate,
        new TimeOnly(11, 0),
        "Free",
        null,
        currentUser,
        currentRole,
        string.Equals(currentRole, "admin", StringComparison.OrdinalIgnoreCase),
        "Välj en tid i kalendern för att börja.");

    return Results.Content(
        V2Html.RenderCalendarShell(week, panel, weekOffset, currentUser, currentRole),
        "text/html; charset=utf-8");
});

app.MapGet("/fragments/booking-panel", (HttpContext context, DemoDataStore store, DateOnly? date, TimeOnly? startTime) =>
{
    var currentUser = GetCurrentUser(context);
    var currentRole = GetCurrentRole(context);

    if (date is null || startTime is null)
    {
        var placeholder = new BookingPanelVm(
            false,
            DateOnly.FromDateTime(DateTime.Today),
            new TimeOnly(11, 0),
            "Free",
            null,
            currentUser,
            currentRole,
            string.Equals(currentRole, "admin", StringComparison.OrdinalIgnoreCase),
            "Välj en tid i kalendern för att börja.");

        return Results.Content(
            V2Html.RenderBookingPanel(placeholder),
            "text/html; charset=utf-8");
    }

    var panel = store.BuildBookingPanel(date.Value, startTime.Value, currentUser, currentRole);
    return Results.Content(
        V2Html.RenderBookingPanel(panel),
        "text/html; charset=utf-8");
});

app.MapGet("/fragments/dashboard-shell", (HttpContext context, DemoDataStore store) =>
{
    var currentUser = GetCurrentUser(context);
    var currentRole = GetCurrentRole(context);
    var stats = store.GetStats();

    return Results.Content(
        V2Html.RenderDashboardShell(stats, currentUser, currentRole),
        "text/html; charset=utf-8");
});

app.MapGet("/fragments/admin-shell", (HttpContext context, DemoDataStore store) =>
{
    var currentRole = GetCurrentRole(context);
    var users = store.GetUsers();

    return Results.Content(
        V2Html.RenderAdminShell(users, IsAdmin(currentRole), string.Empty),
        "text/html; charset=utf-8");
});

app.MapPost("/actions/login", ([FromForm] LoginInput input, HttpContext context, DemoDataStore store) =>
{
    var user = store.Authenticate(input.Username, input.Password);
    if (user is null)
    {
        return Results.Content(
            V2Html.RenderLoginResult(new LoginResultVm(false, "Felaktiga uppgifter.", string.Empty, string.Empty)),
            "text/html; charset=utf-8");
    }

    context.Response.Cookies.Append("sbv2-user", user.Username, cookieOptions);
    context.Response.Cookies.Append("sbv2-role", user.Role, cookieOptions);

    if (IsHtmxRequest(context))
    {
        context.Response.Headers["HX-Redirect"] = "/sauna/v2/dashboard.html";
        return Results.Empty;
    }

    return Results.Redirect("/sauna/v2/dashboard.html");
});

app.MapPost("/actions/logout", (HttpContext context) =>
{
    context.Response.Cookies.Delete("sbv2-user", cookieOptions);
    context.Response.Cookies.Delete("sbv2-role", cookieOptions);

    if (IsHtmxRequest(context))
    {
        context.Response.Headers["HX-Redirect"] = "/sauna/v2/";
        return Results.Empty;
    }

    return Results.Redirect("/sauna/v2/");
});

app.MapPost("/actions/book", ([FromForm] BookingInput input, HttpContext context, DemoDataStore store) =>
{
    var currentUser = GetCurrentUser(context);
    var currentRole = GetCurrentRole(context);

    if (string.IsNullOrWhiteSpace(currentUser))
    {
        return Results.Content(
            V2Html.RenderBookingPanel(new BookingPanelVm(
                true,
                input.Date,
                input.StartTime,
                "Free",
                null,
                currentUser,
                currentRole,
                IsAdmin(currentRole),
                "Du måste vara inloggad för att boka.")),
            "text/html; charset=utf-8");
    }

    var success = store.TryBook(input.Date, input.StartTime, input.Type, currentUser, currentRole, out var message);
    var weekOffset = CalculateWeekOffset(input.Date);
    var week = store.BuildWeek(input.Date, currentUser);
    var panel = store.BuildBookingPanel(input.Date, input.StartTime, currentUser, currentRole, message);
    var shell = V2Html.RenderCalendarShell(week, panel, weekOffset, currentUser, currentRole, message);

    return Results.Content(shell, "text/html; charset=utf-8");
});

app.MapPost("/actions/clear", ([FromForm] BookingInput input, HttpContext context, DemoDataStore store) =>
{
    var currentUser = GetCurrentUser(context);
    var currentRole = GetCurrentRole(context);

    if (string.IsNullOrWhiteSpace(currentUser))
    {
        return Results.Content(
            V2Html.RenderBookingPanel(new BookingPanelVm(
                true,
                input.Date,
                input.StartTime,
                "Free",
                null,
                currentUser,
                currentRole,
                IsAdmin(currentRole),
                "Du måste vara inloggad för att avboka.")),
            "text/html; charset=utf-8");
    }

    var success = store.TryClear(input.Date, input.StartTime, currentUser, currentRole, out var message);
    var weekOffset = CalculateWeekOffset(input.Date);
    var week = store.BuildWeek(input.Date, currentUser);
    var panel = store.BuildBookingPanel(input.Date, input.StartTime, currentUser, currentRole, message);
    var shell = V2Html.RenderCalendarShell(week, panel, weekOffset, currentUser, currentRole, message);

    return Results.Content(shell, "text/html; charset=utf-8");
});

app.MapPost("/actions/admin/create", ([FromForm] UserInput input, HttpContext context, DemoDataStore store) =>
{
    var currentRole = GetCurrentRole(context);
    if (!IsAdmin(currentRole))
    {
        return Results.Content(
            V2Html.RenderAdminShell(store.GetUsers(), false, "Endast administratörer kan skapa användare."),
            "text/html; charset=utf-8");
    }

    var _ = store.TryCreateUser(input.Username, input.Password, input.Role, out var message);
    return Results.Content(
        V2Html.RenderAdminShell(store.GetUsers(), true, message),
        "text/html; charset=utf-8");
});

app.MapPost("/actions/admin/delete", ([FromForm] int id, HttpContext context, DemoDataStore store) =>
{
    var currentRole = GetCurrentRole(context);
    if (!IsAdmin(currentRole))
    {
        return Results.Content(
            V2Html.RenderAdminShell(store.GetUsers(), false, "Endast administratörer kan ta bort användare."),
            "text/html; charset=utf-8");
    }

    var _ = store.TryDeleteUser(id, out var message);
    return Results.Content(
        V2Html.RenderAdminShell(store.GetUsers(), true, message),
        "text/html; charset=utf-8");
});

app.Run();

static string GetCurrentUser(HttpContext context) => context.Request.Cookies["sbv2-user"] ?? string.Empty;

static string GetCurrentRole(HttpContext context) => context.Request.Cookies["sbv2-role"] ?? "user";

static bool IsAdmin(string role) => string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);

static bool IsHtmxRequest(HttpContext context) => string.Equals(context.Request.Headers["HX-Request"], "true", StringComparison.OrdinalIgnoreCase);

static int CalculateWeekOffset(DateOnly date)
{
    var todayWeekStart = GetWeekStart(DateOnly.FromDateTime(DateTime.Today));
    var targetWeekStart = GetWeekStart(date);
    return (targetWeekStart.DayNumber - todayWeekStart.DayNumber) / 7;
}

static DateOnly GetWeekStart(DateOnly date)
{
    var offset = ((int)date.DayOfWeek + 6) % 7;
    return date.AddDays(-offset);
}
