namespace Checklist.Infrastructure.Services;

internal static class BusinessDate
{
    private static readonly TimeZoneInfo SaoPauloTimeZone = ResolveTimeZone();

    public static DateTime TodayKeyUtc()
    {
        var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, SaoPauloTimeZone);
        return new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    private static TimeZoneInfo ResolveTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
    }
}
