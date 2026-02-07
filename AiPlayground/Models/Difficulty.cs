namespace AiPlayground.Models;

/// <summary>
/// 游戏难度级别
/// </summary>
public enum Difficulty
{
    /// <summary>
    /// 简单模式：3个食物，基础分5分，速度较慢
    /// </summary>
    Easy = 1,

    /// <summary>
    /// 中等模式：1个食物，基础分10分，标准速度
    /// </summary>
    Medium = 2,

    /// <summary>
    /// 困难模式：1个食物，基础分20分，速度快
    /// </summary>
    Hard = 3
}
