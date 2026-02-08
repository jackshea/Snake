using AiPlayground.Services.Abstractions;

namespace AiPlayground.Tests.TestHelpers;

/// <summary>
/// Mock时间提供者 - 用于测试时控制时间
/// </summary>
public class MockTimeProvider : ITimeProvider
{
    private long _currentTime;

    public MockTimeProvider()
    {
        _currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public MockTimeProvider(long initialTime)
    {
        _currentTime = initialTime;
    }

    public long GetCurrentUnixTimeMilliseconds() => _currentTime;

    public DateTime Now => new DateTime(_currentTime * 10000, DateTimeKind.Utc).AddYears(1969);

    public DateTime GetCurrentDateTime() => Now;

    public DateTimeOffset GetCurrentDateTimeOffset() => new DateTimeOffset(Now, TimeSpan.Zero);

    /// <summary>
    /// 设置当前时间（用于测试）
    /// </summary>
    public void SetCurrentTime(long time)
    {
        _currentTime = time;
    }

    /// <summary>
    /// 前进指定毫秒数（用于测试）
    /// </summary>
    public void AdvanceTime(long milliseconds)
    {
        _currentTime += milliseconds;
    }

    /// <summary>
    /// 前进指定秒数（用于测试）
    /// </summary>
    public void AdvanceSeconds(int seconds)
    {
        AdvanceTime(seconds * 1000L);
    }
}
