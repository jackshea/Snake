namespace AiPlayground.Services.Abstractions;

/// <summary>
/// 时间提供者抽象接口 - 用于测试时控制时间
/// </summary>
public interface ITimeProvider
{
    /// <summary>
    /// 获取当前时间的 Unix 时间戳（毫秒）
    /// </summary>
    long GetCurrentUnixTimeMilliseconds();

    /// <summary>
    /// 获取当前时间
    /// </summary>
    DateTime Now { get; }
}
