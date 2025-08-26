using System;
using Cronos;
/// <summary>
/// Helper class to convert cron expressions between local and UTC time zones.
/// </summary>
public static class CronHelper
{
    public static string ConvertLocalCronToUtcCron(string localCronExpression, string localTimeZoneId)
    {
        // Kiểm tra tính hợp lệ của biểu thức cron
        var cron = CronExpression.Parse(localCronExpression, CronFormat.Standard);
        var parts = localCronExpression.Split(' ');
        if (parts.Length < 5)
            throw new ArgumentException("Invalid cron expression");

        // Lấy múi giờ local
        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneId);

        // Tính thời điểm xảy ra tiếp theo trong múi giờ local
        var nowUtc = DateTime.UtcNow;
        var nextOccurrenceLocal = cron.GetNextOccurrence(nowUtc, localTimeZone);
        if (!nextOccurrenceLocal.HasValue)
            throw new InvalidOperationException("Cannot determine next occurrence");

        // Cronos trả về Unspecified, cần gán Kind = Unspecified cho đúng time zone
        var localTime = DateTime.SpecifyKind(nextOccurrenceLocal.Value, DateTimeKind.Unspecified);

        // Chuyển đổi sang UTC
        var nextOccurrenceUtc = TimeZoneInfo.ConvertTimeToUtc(localTime, localTimeZone);

        // Tạo biểu thức cron mới cho UTC
        var utcHour = nextOccurrenceUtc.Hour;
        var utcMinute = nextOccurrenceUtc.Minute;
        var utcDayOfWeek = ((int)nextOccurrenceUtc.DayOfWeek + 6) % 7; // Chuyển sang định dạng cron (0=Chủ nhật)

        // Cập nhật giờ, phút và thứ
        parts[0] = utcMinute.ToString(); // Phút
        parts[1] = utcHour.ToString();   // Giờ
        if (parts[4] != "*") // Chỉ cập nhật nếu trường thứ không phải "*"
        {
            // Điều chỉnh trường thứ dựa trên chênh lệch ngày
            var localDayOfWeek = ((int)nextOccurrenceLocal.Value.DayOfWeek + 6) % 7;
            var dayOfWeekShift = (utcDayOfWeek - localDayOfWeek + 7) % 7;
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