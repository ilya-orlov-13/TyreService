namespace TyreServiceApp.Utils;

public static class PermTime
{
    public static readonly TimeZoneInfo PermZone =
        TryGetZone("Asia/Yekaterinburg")
        ?? TryGetZone("Ekaterinburg Standard Time")
        ?? TimeZoneInfo.Local;

    private static TimeZoneInfo? TryGetZone(string id)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
        catch { return null; }
    }

    public static DateTime Now =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PermZone);

    public static DateTime Today =>
        Now.Date;

    /// <summary>
    /// Converts a UTC DateTime to Perm timezone.
    /// If the DateTime is Unspecified or Local, returns it unchanged.
    /// </summary>
    public static DateTime FromUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc
            ? TimeZoneInfo.ConvertTimeFromUtc(dt, PermZone)
            : dt;
}
