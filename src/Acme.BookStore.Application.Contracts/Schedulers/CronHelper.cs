using System;
using Cronos;

public static class CronHelper
{
    public static string ConvertUtcCronToLocalCron(string utcCronExpression, string localTimeZoneId)
    {
        var parts = utcCronExpression.Split(' ');
        if (parts.Length < 5)
            throw new ArgumentException("Invalid cron expression");

        // Lấy timezone
        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneId);

        // Parse minute/hour
        if (!int.TryParse(parts[0], out var minute) ||
            !int.TryParse(parts[1], out var hour))
        {
            throw new NotSupportedException("Chỉ hỗ trợ cron số đơn giản (không có */5, 1-5, range, step)");
        }

        // Dựng một thời điểm mẫu theo cron UTC
        var sampleUtc = new DateTime(2025, 1, 6, hour, minute, 0, DateTimeKind.Utc);

        // Convert sang local
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(sampleUtc, localTimeZone);

        // Update cron parts
        parts[0] = localTime.Minute.ToString();
        parts[1] = localTime.Hour.ToString();

        return string.Join(" ", parts);
    }
}