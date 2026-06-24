using System.ComponentModel.DataAnnotations;

namespace SaunaBooking.V2.Models;

public sealed record DemoUser(int Id, string Username, string Password, string Role);

public sealed record DemoBooking(int Id, DateOnly Date, TimeOnly StartTime, string Username, string Type);

public sealed record CalendarSlotVm(
    DateOnly Date,
    TimeOnly StartTime,
    string Status,
    string? Username,
    string? Initials,
    bool IsOwnedByCurrentUser);

public sealed record CalendarWeekVm(
    DateOnly WeekStart,
    DateOnly WeekEnd,
    IReadOnlyList<DateOnly> Days,
    IReadOnlyList<TimeOnly> Times,
    IReadOnlyList<CalendarSlotVm> Slots);

public sealed record BookingPanelVm(
    bool HasSelection,
    DateOnly Date,
    TimeOnly StartTime,
    string Status,
    string? Username,
    string CurrentUser,
    string CurrentRole,
    bool CanBookBlocked,
    string Message);

public sealed record DashboardStatsVm(
    int BookingCount,
    int UserCount,
    int PrivateCount,
    int OpenCount,
    int BlockedCount);

public sealed record LoginResultVm(bool Success, string Message, string Username, string Role);

public sealed class LoginInput
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed class BookingInput
{
    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public string Type { get; set; } = "Private";
}

public sealed class UserInput
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "user";
}
