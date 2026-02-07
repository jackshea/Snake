using AiPlayground.Services.Abstractions;

namespace AiPlayground.Tests.TestHelpers;

/// <summary>
/// 模拟随机数生成器 - 用于测试时控制随机行为
/// </summary>
public class MockRandomProvider : IRandomProvider
{
    private readonly Queue<int> _intValues = new();
    private readonly Queue<double> _doubleValues = new();

    /// <summary>
    /// 设置下次调用 Next(int maxValue) 时返回的值
    /// </summary>
    public void SetupNext(int value)
    {
        _intValues.Enqueue(value);
    }

    /// <summary>
    /// 设置多次调用 Next(int maxValue) 时返回的值序列
    /// </summary>
    public void SetupNextSequence(params int[] values)
    {
        foreach (var value in values)
        {
            _intValues.Enqueue(value);
        }
    }

    /// <summary>
    /// 设置下次调用 NextDouble() 时返回的值
    /// </summary>
    public void SetupNextDouble(double value)
    {
        _doubleValues.Enqueue(value);
    }

    public int Next(int maxValue)
    {
        if (_intValues.Count > 0)
        {
            var value = _intValues.Dequeue();
            return Math.Min(value, maxValue - 1);
        }
        return 0; // 默认返回 0
    }

    public int Next(int minValue, int maxValue)
    {
        if (_intValues.Count > 0)
        {
            var value = _intValues.Dequeue();
            return Math.Clamp(value, minValue, maxValue - 1);
        }
        return minValue; // 默认返回最小值
    }

    public double NextDouble()
    {
        if (_doubleValues.Count > 0)
        {
            return _doubleValues.Dequeue();
        }
        return 0.5; // 默认返回 0.5
    }

    /// <summary>
    /// 清除所有预设值
    /// </summary>
    public void Reset()
    {
        _intValues.Clear();
        _doubleValues.Clear();
    }
}
