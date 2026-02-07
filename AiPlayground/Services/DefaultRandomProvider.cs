using AiPlayground.Services.Abstractions;

namespace AiPlayground.Services;

/// <summary>
/// 默认随机数生成器实现 - 使用 System.Random
/// </summary>
public class DefaultRandomProvider : IRandomProvider
{
    private readonly Random _random = new();

    public int Next(int maxValue) => _random.Next(maxValue);

    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    public double NextDouble() => _random.NextDouble();
}
