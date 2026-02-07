namespace AiPlayground.Services.Abstractions;

/// <summary>
/// 随机数生成器抽象接口 - 用于测试时模拟随机行为
/// </summary>
public interface IRandomProvider
{
    /// <summary>
    /// 返回非负随机整数（小于指定最大值）
    /// </summary>
    int Next(int maxValue);

    /// <summary>
    /// 返回指定范围内的随机整数
    /// </summary>
    int Next(int minValue, int maxValue);

    /// <summary>
    /// 返回 0.0 到 1.0 之间的随机浮点数
    /// </summary>
    double NextDouble();
}
