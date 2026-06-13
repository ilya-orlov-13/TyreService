namespace TyreServiceApp.Utils;

public static class PermTime
{
    private static readonly TimeZoneInfo PermZone =
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
}
