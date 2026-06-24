using SaunaBooking.V2.Models;

namespace SaunaBooking.V2.Services;

public sealed class DemoDataStore
{
    private readonly object _gate = new();
    private readonly List<DemoUser> _users = new();
    private readonly List<DemoBooking> _bookings = new();
    private int _nextBookingId = 1;
    private int _nextUserId = 1;

    public DemoDataStore()
    {
        Seed();
    }

    public IReadOnlyList<DemoUser> GetUsers()
    {
        lock (_gate)
        {
            return _users.OrderBy(user => user.Id).ToList();
        }
    }

    public DemoUser? Authenticate(string username, string password)
    {
        lock (_gate)
        {
            return _users.FirstOrDefault(user =>
                string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase) &&
                user.Password == password);
        }
    }

    public DashboardStatsVm GetStats()
    {
        lock (_gate)
        {
            return new DashboardStatsVm(
                _bookings.Count,
                _users.Count,
                _bookings.Count(booking => booking.Type == "Private"),
                _bookings.Count(booking => booking.Type == "Open"),
                _bookings.Count(booking => booking.Type == "Blocked"));
        }
    }

    public CalendarWeekVm BuildWeek(DateOnly anchorDate, string? currentUser = null)
    {
        lock (_gate)
        {
            var weekStart = GetWeekStart(anchorDate);
            var days = Enumerable.Range(0, 7).Select(offset => weekStart.AddDays(offset)).ToList();
            var times = Enumerable.Range(11, 12).Select(hour => new TimeOnly(hour, 0)).ToList();
            var slots = days.SelectMany(day => times.Select(time => BuildSlot(day, time, currentUser))).ToList();

            return new CalendarWeekVm(weekStart, weekStart.AddDays(6), days, times, slots);
        }
    }

    public BookingPanelVm BuildBookingPanel(DateOnly date, TimeOnly startTime, string? currentUser, string currentRole, string message = "")
    {
        lock (_gate)
        {
            var booking = GetBooking(date, startTime);
            var status = booking?.Type ?? "Free";
            var canBookBlocked = string.Equals(currentRole, "admin", StringComparison.OrdinalIgnoreCase);
            return new BookingPanelVm(
                true,
                date,
                startTime,
                status,
                booking?.Username,
                currentUser ?? string.Empty,
                currentRole,
                canBookBlocked,
                message);
        }
    }

    public bool TryBook(DateOnly date, TimeOnly startTime, string type, string username, string role, out string message)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                message = "Du måste vara inloggad för att boka.";
                return false;
            }

            if (string.Equals(type, "Blocked", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
            {
                message = "Endast administratörer kan skapa blockerade tider.";
                return false;
            }

            if (GetBooking(date, startTime) is not null)
            {
                message = "Tiden är redan bokad.";
                return false;
            }

            _bookings.Add(new DemoBooking(_nextBookingId++, date, startTime, username, type));
            message = "Bokningen sparades.";
            return true;
        }
    }

    public bool TryClear(DateOnly date, TimeOnly startTime, string username, string role, out string message)
    {
        lock (_gate)
        {
            var booking = GetBooking(date, startTime);
            if (booking is null)
            {
                message = "Ingen bokning hittades.";
                return false;
            }

            if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(booking.Username, username, StringComparison.OrdinalIgnoreCase))
            {
                message = "Du kan bara ta bort dina egna bokningar.";
                return false;
            }

            _bookings.Remove(booking);
            message = "Bokningen togs bort.";
            return true;
        }
    }

    public bool TryCreateUser(string username, string password, string role, out string message)
    {
        lock (_gate)
        {
            if (_users.Any(user => string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase)))
            {
                message = "Användarnamnet finns redan.";
                return false;
            }

            _users.Add(new DemoUser(_nextUserId++, username, password, role));
            message = "Användaren skapades.";
            return true;
        }
    }

    public bool TryDeleteUser(int id, out string message)
    {
        lock (_gate)
        {
            var user = _users.FirstOrDefault(candidate => candidate.Id == id);
            if (user is null)
            {
                message = "Användaren hittades inte.";
                return false;
            }

            _users.Remove(user);
            _bookings.RemoveAll(booking => string.Equals(booking.Username, user.Username, StringComparison.OrdinalIgnoreCase));
            message = "Användaren togs bort.";
            return true;
        }
    }

    public DemoBooking? GetBooking(DateOnly date, TimeOnly startTime)
    {
        lock (_gate)
        {
            return _bookings.FirstOrDefault(booking => booking.Date == date && booking.StartTime == startTime);
        }
    }

    private CalendarSlotVm BuildSlot(DateOnly date, TimeOnly startTime, string? currentUser)
    {
        var booking = GetBooking(date, startTime);
        var initials = booking is null
            ? null
            : string.Join(string.Empty, booking.Username.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(part => part[..1].ToUpperInvariant()));

        return new CalendarSlotVm(
            date,
            startTime,
            booking?.Type ?? "Free",
            booking?.Username,
            initials,
            !string.IsNullOrWhiteSpace(currentUser) && booking is not null &&
            string.Equals(booking.Username, currentUser, StringComparison.OrdinalIgnoreCase));
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-offset);
    }

    private void Seed()
    {
        _users.AddRange(
        [
            new DemoUser(_nextUserId++, "admin", "admin", "admin"),
            new DemoUser(_nextUserId++, "anna", "anna", "user"),
            new DemoUser(_nextUserId++, "jonas", "jonas", "user")
        ]);

        var weekStart = GetWeekStart(DateOnly.FromDateTime(DateTime.Today));
        _bookings.AddRange(
        [
            new DemoBooking(_nextBookingId++, weekStart.AddDays(0), new TimeOnly(11, 0), "anna", "Private"),
            new DemoBooking(_nextBookingId++, weekStart.AddDays(1), new TimeOnly(12, 0), "jonas", "Open"),
            new DemoBooking(_nextBookingId++, weekStart.AddDays(3), new TimeOnly(17, 0), "admin", "Blocked")
        ]);
    }
}
