using System;
using Cronos;
/// <summary>
/// Helper class to convert cron expressions between UTC and local time zones.
/// </summary>
public static class CronHelper
{
    public static string ConvertUtcCronToLocalCron(string utcCronExpression, string localTimeZoneId)
    {
        // Kiểm tra tính hợp lệ của biểu thức cron
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

        // Chuyển đổi sang múi giờ local
        var nextOccurrenceLocal = TimeZoneInfo.ConvertTimeFromUtc(nextOccurrenceUtc.Value, localTimeZone);

        // Tạo biểu thức cron mới cho local time
        var localHour = nextOccurrenceLocal.Hour;
        var localMinute = nextOccurrenceLocal.Minute;
        var localDayOfWeek = ((int)nextOccurrenceLocal.DayOfWeek + 6) % 7; // Chuyển sang định dạng cron (0=Chủ nhật)

        // Cập nhật giờ, phút và thứ
        parts[0] = localMinute.ToString(); // Phút
        parts[1] = localHour.ToString();   // Giờ
        if (parts[4] != "*") // Chỉ cập nhật nếu trường thứ không phải "*"
        {
            // Điều chỉnh trường thứ dựa trên chênh lệch ngày
            var utcDayOfWeek = ((int)nextOccurrenceUtc.Value.DayOfWeek + 6) % 7;
            var dayOfWeekShift = (localDayOfWeek - utcDayOfWeek + 7) % 7;
            if (int.TryParse(parts[4], out var originalDayOfWeek))
            {
                var newDayOfWeek = (originalDayOfWeek + dayOfWeekShift) % 7;
                parts[4] = newDayOfWeek.ToString();
            }
            // Lưu ý: Với các biểu thức phức tạp (như 1-5, */2), cần xử lý thêm
        }

        return string.Join(" ", parts);
    }
}