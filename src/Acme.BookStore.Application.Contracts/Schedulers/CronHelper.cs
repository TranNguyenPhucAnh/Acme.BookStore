using System;
using Cronos;
/// <summary>
/// Helper class to convert cron expressions between UTC and local time zones.
/// </summary>
public static class CronHelper
{
    public static string ConvertUtcCronToLocalCron(string utcCronExpression, string localTimeZoneId)
    {
        var cron = CronExpression.Parse(utcCronExpression, CronFormat.Standard);
        var parts = utcCronExpression.Split(' ');
        if (parts.Length < 5)
            throw new ArgumentException("Invalid cron expression");

        // Lấy múi giờ local
        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneId);

        // Tính thời điểm xảy ra tiếp theo trong UTC
        var nowUtc = DateTime.UtcNow;
        var nextOccurrenceUtc = cron.GetNextOccurrence(nowUtc, TimeZoneInfo.Utc);
        if (!nextOccurrenceUtc.HasValue)
            throw new InvalidOperationException("Cannot determine next occurrence");

        // Convert sang local
        var nextOccurrenceLocal = TimeZoneInfo.ConvertTimeFromUtc(nextOccurrenceUtc.Value, localTimeZone);

        // Double offset (cộng thêm lần nữa)
        var offset = localTimeZone.GetUtcOffset(nextOccurrenceUtc.Value);
        var doubleShifted = nextOccurrenceLocal.Add(offset);

        // Build cron mới dựa trên double-shifted
        var localHour = doubleShifted.Hour;
        var localMinute = doubleShifted.Minute;
        var localDayOfWeek = ((int)doubleShifted.DayOfWeek + 6) % 7;

        parts[0] = localMinute.ToString(); // phút
        parts[1] = localHour.ToString();   // giờ

        if (parts[4] != "*")
        {
            var utcDayOfWeek = ((int)nextOccurrenceUtc.Value.DayOfWeek + 6) % 7;
            var dayOfWeekShift = (localDayOfWeek - utcDayOfWeek + 7) % 7;
            if (int.TryParse(parts[4], out var originalDayOfWeek))
            {
                var newDayOfWeek = (originalDayOfWeek + dayOfWeekShift) % 7;
                parts[4] = newDayOfWeek.ToString();
            }
        }

        return string.Join(" ", parts);
    }
}