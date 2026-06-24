using System.Globalization;
using System.Net;
using System.Text;
using SaunaBooking.V2.Models;

namespace SaunaBooking.V2.Rendering;

public static class V2Html
{
    private static readonly CultureInfo SwedishCulture = CultureInfo.GetCultureInfo("sv-SE");

    public static string RenderNav(string currentUser, string currentRole)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<header id=\"site-nav\" class=\"sb-topbar\">");
        sb.AppendLine("  <div class=\"container sb-topbar-inner\">");
        sb.AppendLine("    <div>");
        sb.AppendLine("      <div class=\"sb-eyebrow\">Sauna Booking v2</div>");
        sb.AppendLine("      <a class=\"sb-brand\" href=\"/sauna/v2/\">Dyviksudds bastuflotte</a>");
        sb.AppendLine("    </div>");
        sb.AppendLine("    <nav class=\"sb-nav\">");
        sb.AppendLine("      <a href=\"/sauna/v2/\">Hem</a>");
        sb.AppendLine("      <a href=\"/sauna/v2/calendar.html\">Kalender</a>");
        sb.AppendLine("      <a href=\"/sauna/v2/dashboard.html\">Dashboard</a>");
        sb.AppendLine("      <a href=\"/sauna/v2/login.html\">Logga in</a>");
        sb.AppendLine("      <a href=\"/sauna/v2/admin/users.html\">Användare</a>");
        sb.AppendLine("    </nav>");
        sb.AppendLine("    <div class=\"d-flex align-items-center gap-2\">");

        if (!string.IsNullOrWhiteSpace(currentUser))
        {
            sb.AppendLine($"      <span class=\"sb-pill\">{Encode(currentUser)} ({Encode(currentRole)})</span>");
            sb.AppendLine("      <form method=\"post\" hx-post=\"actions/logout\" hx-swap=\"none\" class=\"m-0\">");
            sb.AppendLine("        <button type=\"submit\" class=\"btn sb-btn-primary btn-sm\">Logga ut</button>");
            sb.AppendLine("      </form>");
        }
        else
        {
            sb.AppendLine("      <a class=\"btn sb-btn-primary btn-sm\" href=\"/sauna/v2/login.html\">Logga in</a>");
        }

        sb.AppendLine("    </div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</header>");
        return sb.ToString();
    }

    public static string RenderCalendarShell(CalendarWeekVm week, BookingPanelVm panel, int weekOffset, string currentUser, string currentRole, string? message = null)
    {
        var slotLookup = week.Slots.ToDictionary(slot => (slot.Date, slot.StartTime));
        var weekNumber = SwedishCulture.Calendar.GetWeekOfYear(
            week.WeekStart.ToDateTime(TimeOnly.MinValue),
            CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);

        var sb = new StringBuilder();
        sb.AppendLine("<section id=\"calendar-shell\" class=\"row g-3\">");
        sb.AppendLine("  <div class=\"col-12\">");
        sb.AppendLine("    <div class=\"card sb-card\">");
        sb.AppendLine("      <div class=\"card-body p-3 p-md-4\">");
        sb.AppendLine("        <div class=\"d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3\">");
        sb.AppendLine("          <div>");
        sb.AppendLine("            <div class=\"sb-eyebrow\">Bokningskalender</div>");
        sb.AppendLine($"            <h1 class=\"h3 mb-0\">Vecka {weekNumber}: {week.WeekStart:yyyy-MM-dd} - {week.WeekEnd:yyyy-MM-dd}</h1>");
        sb.AppendLine("          </div>");
        sb.AppendLine("          <div class=\"d-flex flex-wrap gap-2\">");
        sb.AppendLine($"            <button class=\"btn btn-outline-secondary\" hx-get=\"fragments/calendar-shell?weekOffset={weekOffset - 1}\" hx-target=\"#calendar-shell\" hx-swap=\"outerHTML\">Föregående vecka</button>");
        sb.AppendLine($"            <button class=\"btn btn-outline-secondary\" hx-get=\"fragments/calendar-shell?weekOffset={weekOffset + 1}\" hx-target=\"#calendar-shell\" hx-swap=\"outerHTML\">Nästa vecka</button>");
        sb.AppendLine("          </div>");
        sb.AppendLine("        </div>");

        if (!string.IsNullOrWhiteSpace(message))
        {
            sb.AppendLine($"        <div class=\"alert alert-info sb-alert\">{Encode(message)}</div>");
        }

        if (!string.IsNullOrWhiteSpace(currentUser))
        {
            sb.AppendLine($"        <div class=\"mb-3\"><span class=\"sb-pill\">Inloggad som {Encode(currentUser)}</span></div>");
        }

        sb.AppendLine("        <div class=\"table-responsive\">");
        sb.AppendLine("          <table class=\"table sb-calendar-table align-middle\">");
        sb.AppendLine("            <thead><tr><th>Tid</th>");

        foreach (var day in week.Days)
        {
            sb.AppendLine($"              <th>{day.ToString("ddd dd MMM", SwedishCulture)}</th>");
        }

        sb.AppendLine("            </tr></thead>");
        sb.AppendLine("            <tbody>");

        foreach (var time in week.Times)
        {
            sb.AppendLine("              <tr>");
            sb.AppendLine($"                <th scope=\"row\">{time:HH\\:mm}</th>");

            foreach (var day in week.Days)
            {
                var slot = slotLookup[(day, time)];
                var statusClass = slot.Status.ToLowerInvariant();
                var slotTitle = string.IsNullOrWhiteSpace(slot.Username)
                    ? $"{slot.Status} {day:yyyy-MM-dd} {time:HH\\:mm}"
                    : $"{slot.Status} av {slot.Username}";

                sb.AppendLine("                <td>");
                sb.AppendLine($"                  <button type=\"button\" class=\"sb-slot sb-slot-{statusClass}\" title=\"{Encode(slotTitle)}\" hx-get=\"fragments/booking-panel?date={day:yyyy-MM-dd}&startTime={time:HH\\:mm\\:ss}\" hx-target=\"#booking-panel\" hx-swap=\"innerHTML\">");
                sb.AppendLine($"                    <span class=\"sb-slot-status\">{Encode(slot.Status)}</span>");

                if (!string.IsNullOrWhiteSpace(slot.Initials))
                {
                    sb.AppendLine($"                    <span class=\"sb-slot-initials\">{Encode(slot.Initials)}</span>");
                }

                sb.AppendLine("                  </button>");
                sb.AppendLine("                </td>");
            }

            sb.AppendLine("              </tr>");
        }

        sb.AppendLine("            </tbody>");
        sb.AppendLine("          </table>");
        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("  <div class=\"col-12 col-xl-4\">");
        sb.AppendLine(RenderBookingPanel(panel));
        sb.AppendLine("  </div>");
        sb.AppendLine("</section>");
        return sb.ToString();
    }

    public static string RenderBookingPanel(BookingPanelVm panel)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<div id=\"booking-panel\">");

        if (!panel.HasSelection)
        {
            sb.AppendLine("  <div class=\"sb-empty-panel\">Välj en tid i kalendern för att börja.</div>");
            sb.AppendLine("</div>");
            return sb.ToString();
        }

        sb.AppendLine("  <div class=\"card sb-card\">");
        sb.AppendLine("    <div class=\"card-body\">");
        sb.AppendLine("      <div class=\"sb-eyebrow\">Bokningsruta</div>");
        sb.AppendLine($"      <h2 class=\"h4 mb-1\">{panel.Date:yyyy-MM-dd} {panel.StartTime:HH\\:mm}</h2>");
        sb.AppendLine($"      <div class=\"text-secondary mb-2\">Nuvarande status: {Encode(panel.Status)}</div>");

        if (!string.IsNullOrWhiteSpace(panel.Username))
        {
            sb.AppendLine($"      <div class=\"mb-3\"><span class=\"sb-pill\">{Encode(panel.Username)}</span></div>");
        }

        if (!string.IsNullOrWhiteSpace(panel.Message))
        {
            sb.AppendLine($"      <div class=\"alert alert-info sb-alert\">{Encode(panel.Message)}</div>");
        }

        if (string.IsNullOrWhiteSpace(panel.CurrentUser))
        {
            sb.AppendLine("      <div class=\"alert alert-warning sb-alert\">Du måste vara inloggad för att boka.</div>");
            sb.AppendLine("      <a class=\"btn sb-btn-primary\" href=\"/sauna/v2/login.html\">Logga in</a>");
        }
        else
        {
            var selectedType = panel.Status == "Free" ? "Private" : panel.Status;
            sb.AppendLine("      <form method=\"post\" hx-post=\"actions/book\" hx-target=\"#calendar-shell\" hx-swap=\"outerHTML\">");
            sb.AppendLine($"        <input type=\"hidden\" name=\"Date\" value=\"{panel.Date:yyyy-MM-dd}\" />");
            sb.AppendLine($"        <input type=\"hidden\" name=\"StartTime\" value=\"{panel.StartTime:HH\\:mm\\:ss}\" />");
            sb.AppendLine("        <div class=\"mb-3\">");
            sb.AppendLine("          <label class=\"form-label\" for=\"booking-type\">Bokningstyp</label>");
            sb.AppendLine("          <select class=\"form-select\" id=\"booking-type\" name=\"Type\">");
            sb.AppendLine($"            <option value=\"Private\"{Selected(selectedType, "Private")}>Privat</option>");
            sb.AppendLine($"            <option value=\"Open\"{Selected(selectedType, "Open")}>Öppen</option>");

            if (panel.CanBookBlocked || string.Equals(panel.CurrentRole, "admin", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine($"            <option value=\"Blocked\"{Selected(selectedType, "Blocked")}>Blockerad</option>");
            }

            sb.AppendLine("          </select>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"d-flex flex-wrap gap-2\">");
            sb.AppendLine("          <button class=\"btn sb-btn-primary\" type=\"submit\">Boka</button>");

            if (!string.IsNullOrWhiteSpace(panel.Username))
            {
                sb.AppendLine("          <button class=\"btn btn-outline-danger\" type=\"submit\" formaction=\"actions/clear\">Avboka</button>");
            }

            sb.AppendLine("        </div>");
            sb.AppendLine("      </form>");
        }

        sb.AppendLine("    </div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</div>");
        return sb.ToString();
    }

    public static string RenderDashboardShell(DashboardStatsVm stats, string currentUser, string currentRole)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<section id=\"dashboard-shell\" class=\"row g-3\">");
        sb.AppendLine("  <div class=\"col-12 col-lg-8\">");
        sb.AppendLine("    <div class=\"card sb-card h-100\">");
        sb.AppendLine("      <div class=\"card-body p-4\">");
        sb.AppendLine("        <div class=\"sb-eyebrow mb-2\">Översikt</div>");
        sb.AppendLine($"        <h1 class=\"h3 mb-3\">Välkommen {Encode(string.IsNullOrWhiteSpace(currentUser) ? "gäst" : currentUser)}</h1>");
        sb.AppendLine("        <div class=\"row g-3\">");
        sb.AppendLine(Card("Bokningar", stats.BookingCount));
        sb.AppendLine(Card("Användare", stats.UserCount));
        sb.AppendLine(Card("Privata", stats.PrivateCount));
        sb.AppendLine(Card("Öppna", stats.OpenCount));
        sb.AppendLine(Card("Blockerade", stats.BlockedCount));
        sb.AppendLine("        </div>");
        sb.AppendLine("      </div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("  <div class=\"col-12 col-lg-4\">");
        sb.AppendLine("    <div class=\"card sb-card h-100\">");
        sb.AppendLine("      <div class=\"card-body p-4\">");
        sb.AppendLine("        <div class=\"sb-eyebrow mb-2\">Roll</div>");
        sb.AppendLine($"        <h2 class=\"h5\">{Encode(currentRole)}</h2>");
        sb.AppendLine("        <p class=\"mb-0\">Det här v2-skalet är byggt för att senare kopplas direkt till den riktiga databasen och backend-kontraktet.</p>");
        sb.AppendLine("      </div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</section>");
        return sb.ToString();
    }

    public static string RenderAdminShell(IReadOnlyList<DemoUser> users, bool isAdmin, string message)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<section id=\"admin-shell\" class=\"row g-3\">");
        sb.AppendLine("  <div class=\"col-12 col-lg-8\">");
        sb.AppendLine("    <div class=\"card sb-card\">");
        sb.AppendLine("      <div class=\"card-body p-4\">");
        sb.AppendLine("        <div class=\"sb-eyebrow mb-2\">Administratör</div>");
        sb.AppendLine("        <h1 class=\"h3 mb-3\">Användarhantering</h1>");

        if (!string.IsNullOrWhiteSpace(message))
        {
            sb.AppendLine($"        <div class=\"alert alert-info sb-alert\">{Encode(message)}</div>");
        }

        if (!isAdmin)
        {
            sb.AppendLine("        <div class=\"alert alert-warning sb-alert\">Endast administratörer kan hantera användare.</div>");
        }
        else
        {
            sb.AppendLine("        <div class=\"table-responsive mb-4\">");
            sb.AppendLine("          <table class=\"table table-striped align-middle\">");
            sb.AppendLine("            <thead><tr><th>ID</th><th>Användarnamn</th><th>Roll</th><th></th></tr></thead>");
            sb.AppendLine("            <tbody>");

            foreach (var user in users)
            {
                sb.AppendLine("              <tr>");
                sb.AppendLine($"                <td>{user.Id}</td>");
                sb.AppendLine($"                <td>{Encode(user.Username)}</td>");
                sb.AppendLine($"                <td>{Encode(user.Role)}</td>");
                sb.AppendLine("                <td class=\"text-end\">");
                sb.AppendLine("                  <form method=\"post\" hx-post=\"actions/admin/delete\" hx-target=\"#admin-shell\" hx-swap=\"outerHTML\" class=\"d-inline\">");
                sb.AppendLine($"                    <input type=\"hidden\" name=\"id\" value=\"{user.Id}\" />");
                sb.AppendLine("                    <button class=\"btn btn-sm btn-outline-danger\" type=\"submit\">Ta bort</button>");
                sb.AppendLine("                  </form>");
                sb.AppendLine("                </td>");
                sb.AppendLine("              </tr>");
            }

            sb.AppendLine("            </tbody>");
            sb.AppendLine("          </table>");
            sb.AppendLine("        </div>");

            sb.AppendLine("        <div class=\"card sb-subcard\">");
            sb.AppendLine("          <div class=\"card-body\">");
            sb.AppendLine("            <h2 class=\"h5\">Skapa användare</h2>");
            sb.AppendLine("            <form method=\"post\" hx-post=\"actions/admin/create\" hx-target=\"#admin-shell\" hx-swap=\"outerHTML\">");
            sb.AppendLine("              <div class=\"row g-3\">");
            sb.AppendLine("                <div class=\"col-md-4\"><label class=\"form-label\" for=\"admin-username\">Användarnamn</label><input class=\"form-control\" id=\"admin-username\" name=\"Username\" /></div>");
            sb.AppendLine("                <div class=\"col-md-4\"><label class=\"form-label\" for=\"admin-password\">Lösenord</label><input class=\"form-control\" id=\"admin-password\" name=\"Password\" type=\"password\" /></div>");
            sb.AppendLine("                <div class=\"col-md-4\"><label class=\"form-label\" for=\"admin-role\">Roll</label><select class=\"form-select\" id=\"admin-role\" name=\"Role\"><option value=\"user\">user</option><option value=\"admin\">admin</option></select></div>");
            sb.AppendLine("              </div>");
            sb.AppendLine("              <div class=\"mt-3\"><button class=\"btn sb-btn-primary\" type=\"submit\">Skapa</button></div>");
            sb.AppendLine("            </form>");
            sb.AppendLine("          </div>");
            sb.AppendLine("        </div>");
        }

        sb.AppendLine("      </div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("  <div class=\"col-12 col-lg-4\">");
        sb.AppendLine("    <div class=\"card sb-card h-100\"><div class=\"card-body p-4\"><div class=\"sb-eyebrow mb-2\">Cutover target</div><p class=\"mb-0\">Den här sidan är avsedd att ersätta Blazor-användarhanteringen med vanliga formulär och partial swaps.</p></div></div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</section>");
        return sb.ToString();
    }

    public static string RenderLoginResult(LoginResultVm result)
    {
        var cssClass = result.Success ? "alert-success" : "alert-danger";
        return $"<div class=\"alert {cssClass} sb-alert\" role=\"alert\">{Encode(result.Message)}</div>";
    }

    private static string Card(string label, int value)
    {
        return $"<div class=\"col-md-3\"><div class=\"sb-metric h-100\"><div>{Encode(label)}</div><strong>{value}</strong></div></div>";
    }

    private static string Selected(string current, string expected) => string.Equals(current, expected, StringComparison.OrdinalIgnoreCase) ? " selected" : string.Empty;

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
