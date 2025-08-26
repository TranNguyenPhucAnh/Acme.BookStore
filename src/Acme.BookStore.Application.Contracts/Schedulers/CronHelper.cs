using System;
/// <summary>
/// Helper class to convert cron expressions between local and UTC time zones.
/// </summary>
public static class CronHelper
{
    public static string ConvertLocalCronToUtcCron(string localCronExpression, string localTimeZoneId)
    {
        var parts = localCronExpression.Split(' ');
        if (parts.Length < 5)
            throw new ArgumentException("Invalid cron expression");

        // Lấy timezone
        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneId);

        // Lấy offset hiện tại
        var now = DateTime.UtcNow;
        var offset = localTimeZone.GetUtcOffset(now);

        // Giả sử cron chỉ có minute hour day month dayOfWeek
        if (!int.TryParse(parts[0], out var minute) ||
            !int.TryParse(parts[1], out var hour))
        {
            throw new NotSupportedException("Chỉ hỗ trợ cron số đơn giản (không có */5, 1-5)");
        }

        // Tạo thời gian local mẫu
        var localSample = new DateTime(2025, 1, 6, hour, minute, 0, DateTimeKind.Unspecified);
        var localWithKind = DateTime.SpecifyKind(localSample, DateTimeKind.Unspecified);

        // Chuyển sang UTC
        var utcTime = TimeZoneInfo.ConvertTimeToUtc(localWithKind, localTimeZone);

        // Cập nhật lại cron parts
        parts[0] = utcTime.Minute.ToString();
        parts[1] = utcTime.Hour.ToString();

        return string.Join(" ", parts);
    }
}