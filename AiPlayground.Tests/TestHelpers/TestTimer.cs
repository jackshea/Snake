using AiPlayground.Services.Abstractions;

namespace AiPlayground.Tests.TestHelpers;

/// <summary>
/// 测试用定时器 - 手动触发Tick事件，便于测试
/// </summary>
public class TestTimer : IGameTimer
{
    public event EventHandler? Tick;
    public int Interval { get; set; }
    public bool IsEnabled { get; private set; }

    public void Start()
    {
        IsEnabled = true;
    }

    public void Stop()
    {
        IsEnabled = false;
    }

    /// <summary>
    /// 手动触发Tick事件（用于测试）
    /// </summary>
    public void SimulateTick()
    {
        Tick?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 手动触发多次Tick事件（用于测试）
    /// </summary>
    public void SimulateTicks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SimulateTick();
        }
    }

    public void Dispose()
    {
        // 测试中不需要释放资源
    }
}
