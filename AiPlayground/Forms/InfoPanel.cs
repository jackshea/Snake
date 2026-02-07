using System;
using System.Drawing;
using System.Windows.Forms;
using AiPlayground.Controls;
using AiPlayground.Game;
using AiPlayground.Models;

namespace AiPlayground.Forms;

/// <summary>
/// 信息面板 - 显示游戏状态和统计信息
/// </summary>
public class InfoPanel : DoubleBufferPanel
{
    private readonly GameState _gameState;
    private readonly int _highScore;

    public InfoPanel(GameState gameState, int highScore)
    {
        _gameState = gameState;
        _highScore = highScore;
        BackColor = Color.FromArgb(30, 30, 30);
        BorderStyle = BorderStyle.Fixed3D;
    }

    public void UpdateHighScore(int highScore)
    {
        // 需要通过某种方式更新，这里暂时省略
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using Font headerFont = new Font("Microsoft YaHei UI", 14, FontStyle.Bold);
        using Font normalFont = new Font("Microsoft YaHei UI", 11);
        using Font smallFont = new Font("Microsoft YaHei UI", 9);
        using Brush whiteBrush = new SolidBrush(Color.White);
        using Brush yellowBrush = new SolidBrush(Color.Yellow);
        using Brush cyanBrush = new SolidBrush(Color.Cyan);
        using Brush greenBrush = new SolidBrush(Color.Lime);
        using Brush goldBrush = new SolidBrush(Color.Gold);

        int y = 15;
        int x = 15;

        // 标题
        g.DrawString("游戏信息", headerFont, yellowBrush, x, y);
        y += 35;

        // 分数
        g.DrawString("当前分数", normalFont, whiteBrush, x, y);
        y += 25;
        g.DrawString(_gameState.Score.ToString(), headerFont, greenBrush, x, y);
        y += 40;

        // 最高分
        g.DrawString("最高分数", normalFont, whiteBrush, x, y);
        y += 25;
        Color highScoreColor = _gameState.IsNewHighScore ? Color.Gold : Color.Cyan;
        using (Brush highScoreBrush = new SolidBrush(highScoreColor))
        {
            g.DrawString(_highScore.ToString(), headerFont, highScoreBrush, x, y);
        }
        y += 40;

        // 蛇的长度
        g.DrawString("蛇的长度", normalFont, whiteBrush, x, y);
        y += 25;
        g.DrawString(_gameState.Snake.Count.ToString(), headerFont, yellowBrush, x, y);
        y += 40;

        // 速度等级
        g.DrawString("速度等级", normalFont, whiteBrush, x, y);
        y += 25;
        g.DrawString($"等级 {_gameState.SpeedLevel}/10", headerFont, yellowBrush, x, y);
        y += 25;
        g.DrawString(GetSpeedDescription(_gameState.SpeedLevel), smallFont, whiteBrush, x, y);
        y += 40;

        // 难度
        g.DrawString("难度级别", normalFont, whiteBrush, x, y);
        y += 25;
        g.DrawString(GetDifficultyName(_gameState.Difficulty), headerFont, yellowBrush, x, y);
        y += 40;

        // 游戏状态
        g.DrawString("游戏状态", normalFont, whiteBrush, x, y);
        y += 25;
        string statusText = _gameState.IsWaitingToStart ? "等待开始" : _gameState.IsGameOver ? "已结束" : _gameState.IsPaused ? "已暂停" : "进行中";
        Color statusColor = _gameState.IsWaitingToStart ? Color.Gray : _gameState.IsGameOver ? Color.Red : _gameState.IsPaused ? Color.Orange : Color.Lime;
        using (Brush statusBrush = new SolidBrush(statusColor))
        {
            g.DrawString(statusText, headerFont, statusBrush, x, y);
        }
        y += 50;

        // 操作提示
        g.DrawString("快捷键", headerFont, yellowBrush, x, y);
        y += 30;
        g.DrawString("↑↓←→ 移动", smallFont, whiteBrush, x, y); y += 20;
        g.DrawString("空格 暂停", smallFont, whiteBrush, x, y); y += 20;
        g.DrawString("按钮 开始", smallFont, whiteBrush, x, y); y += 20;
        g.DrawString("F2 新游戏", smallFont, whiteBrush, x, y); y += 20;
        g.DrawString("F1 帮助", smallFont, whiteBrush, x, y);
    }

    private string GetSpeedDescription(int level)
    {
        return level switch
        {
            1 => "超慢",
            2 => "很慢",
            3 => "慢",
            4 => "较慢",
            5 => "中等",
            6 => "较快",
            7 => "快",
            8 => "很快",
            9 => "超快",
            10 => "极速",
            _ => "未知"
        };
    }

    private string GetDifficultyName(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Easy => "简单",
            Difficulty.Medium => "中等",
            Difficulty.Hard => "困难",
            _ => "未知"
        };
    }
}
