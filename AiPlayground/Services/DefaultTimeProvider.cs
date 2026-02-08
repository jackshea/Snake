using AiPlayground.Services.Abstractions;

namespace AiPlayground.Services;

/// <summary>
/// 默认时间提供者实现 - 使用系统时间
/// </summary>
public class DefaultTimeProvider : ITimeProvider
{
    public long GetCurrentUnixTimeMilliseconds() =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public DateTime Now => DateTime.Now;

    public DateTime GetCurrentDateTime() => DateTime.UtcNow;

    public DateTimeOffset GetCurrentDateTimeOffset() => DateTimeOffset.UtcNow;
}
