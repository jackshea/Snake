namespace AiPlayground.Services.Abstractions;

/// <summary>
/// 游戏定时器接口 - 用于解耦System.Windows.Forms.Timer依赖
/// </summary>
public interface IGameTimer : IDisposable
{
    /// <summary>
    /// 定时器触发事件
    /// </summary>
    event EventHandler? Tick;

    /// <summary>
    /// 定时器间隔（毫秒）
    /// </summary>
    int Interval { get; set; }

    /// <summary>
    /// 是否正在运行
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// 启动定时器
    /// </summary>
    void Start();

    /// <summary>
    /// 停止定时器
    /// </summary>
    void Stop();
}
