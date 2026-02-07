using System.Drawing;

namespace AiPlayground.Models;

/// <summary>
/// 游戏配置常量
/// </summary>
public static class GameConfig
{
    /// <summary>
    /// 方向常量 - 上
    /// </summary>
    public static readonly Point DirectionUp = new(0, -1);

    /// <summary>
    /// 方向常量 - 下
    /// </summary>
    public static readonly Point DirectionDown = new(0, 1);

    /// <summary>
    /// 方向常量 - 左
    /// </summary>
    public static readonly Point DirectionLeft = new(-1, 0);

    /// <summary>
    /// 方向常量 - 右
    /// </summary>
    public static readonly Point DirectionRight = new(1, 0);
    /// <summary>
    /// 网格大小（格子数量）
    /// </summary>
    public const int GridSize = 20;

    /// <summary>
    /// 每个单元格的像素大小
    /// </summary>
    public const int CellSize = 20;

    /// <summary>
    /// 信息面板宽度
    /// </summary>
    public const int InfoPanelWidth = 200;

    /// <summary>
    /// 最小速度等级
    /// </summary>
    public const int MinSpeed = 1;

    /// <summary>
    /// 最大速度等级
    /// </summary>
    public const int MaxSpeed = 10;

    /// <summary>
    /// 简单模式食物数量
    /// </summary>
    public const int EasyFoodCount = 3;

    /// <summary>
    /// 中等/困难模式食物数量
    /// </summary>
    public const int NormalFoodCount = 1;

    /// <summary>
    /// 简单模式基础得分
    /// </summary>
    public const int EasyBasePoints = 5;

    /// <summary>
    /// 中等模式基础得分
    /// </summary>
    public const int MediumBasePoints = 10;

    /// <summary>
    /// 困难模式基础得分
    /// </summary>
    public const int HardBasePoints = 20;

    /// <summary>
    /// 困难模式基础间隔时间（毫秒）
    /// </summary>
    public const int HardBaseInterval = 80;

    /// <summary>
    /// 普通模式基础间隔时间（毫秒）
    /// </summary>
    public const int NormalBaseInterval = 150;

    /// <summary>
    /// 每级速度减少的间隔时间（毫秒）
    /// </summary>
    public const int SpeedIntervalReduction = 15;

    /// <summary>
    /// 最小间隔时间（毫秒）
    /// </summary>
    public const int MinInterval = 40;
}
