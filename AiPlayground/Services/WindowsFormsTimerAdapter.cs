using System.Windows.Forms;
using AiPlayground.Services.Abstractions;

namespace AiPlayground.Services;

/// <summary>
/// Windows Forms Timer 适配器 - 将System.Windows.Forms.Timer适配到IGameTimer接口
/// </summary>
public class WindowsFormsTimerAdapter : IGameTimer
{
    private readonly System.Windows.Forms.Timer _timer;

    public WindowsFormsTimerAdapter()
    {
        _timer = new System.Windows.Forms.Timer();
    }

    public event EventHandler? Tick
    {
        add => _timer.Tick += value;
        remove => _timer.Tick -= value;
    }

    public int Interval
    {
        get => _timer.Interval;
        set => _timer.Interval = value;
    }

    public bool IsEnabled => _timer.Enabled;

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
