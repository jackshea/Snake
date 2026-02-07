using System;
using System.Collections.Generic;
using System.Drawing;
using AiPlayground.Models;

namespace AiPlayground.Game;

/// <summary>
/// 游戏状态类
/// </summary>
public class GameState
{
    /// <summary>
    /// 蛇身各段的位置列表
    /// </summary>
    public List<Point> Snake { get; set; } = new();

    /// <summary>
    /// 食物位置列表
    /// </summary>
    public List<Point> Foods { get; set; } = new();

    /// <summary>
    /// 当前移动方向
    /// </summary>
    public Point Direction { get; set; } = new(1, 0);

    /// <summary>
    /// 当前分数
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// 游戏是否结束
    /// </summary>
    public bool IsGameOver { get; set; }

    /// <summary>
    /// 是否暂停
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// 是否等待开始
    /// </summary>
    public bool IsWaitingToStart { get; set; } = true;

    /// <summary>
    /// 是否是新纪录
    /// </summary>
    public bool IsNewHighScore { get; set; }

    /// <summary>
    /// 当前难度级别
    /// </summary>
    public Difficulty Difficulty { get; set; } = Difficulty.Medium;

    /// <summary>
    /// 速度等级 (1-10)
    /// </summary>
    public int SpeedLevel { get; set; } = 5;

    /// <summary>
    /// 复制游戏状态
    /// </summary>
    public GameState Clone()
    {
        return new GameState
        {
            Snake = new List<Point>(Snake),
            Foods = new List<Point>(Foods),
            Direction = Direction,
            Score = Score,
            IsGameOver = IsGameOver,
            IsPaused = IsPaused,
            IsWaitingToStart = IsWaitingToStart,
            IsNewHighScore = IsNewHighScore,
            Difficulty = Difficulty,
            SpeedLevel = SpeedLevel
        };
    }
}
