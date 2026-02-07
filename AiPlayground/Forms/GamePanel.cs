using System;
using System.Drawing;
using System.Windows.Forms;
using AiPlayground.Controls;
using AiPlayground.Game;
using AiPlayground.Models;
using AiPlayground.Models.Obstacles;

namespace AiPlayground.Forms;

/// <summary>
/// 游戏面板 - 负责渲染游戏画面
/// </summary>
public class GamePanel : DoubleBufferPanel
{
    private readonly GameState _gameState;
    private readonly GameEngine _gameEngine;

    public GamePanel(GameState gameState, GameEngine gameEngine)
    {
        _gameState = gameState;
        _gameEngine = gameEngine;
        BackColor = Color.Black;
        BorderStyle = BorderStyle.Fixed3D;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

        DrawGrid(g);
        DrawObstacles(g);
        DrawFoods(g);
        DrawSnake(g);

        if (_gameState.IsGameOver)
        {
            DrawGameOver(g);
        }

        if (_gameState.IsPaused && !_gameState.IsGameOver && !_gameState.IsWaitingToStart)
        {
            DrawPaused(g);
        }
    }

    private void DrawGrid(Graphics g)
    {
        int gridWidth = _gameState.CurrentLevel?.GridWidth ?? GameConfig.GridSize;
        int gridHeight = _gameState.CurrentLevel?.GridHeight ?? GameConfig.GridSize;

        using Pen gridPen = new Pen(Color.FromArgb(30, Color.Gray), 1);
        for (int i = 0; i <= gridWidth; i++)
        {
            g.DrawLine(gridPen, i * GameConfig.CellSize, 0, i * GameConfig.CellSize, gridHeight * GameConfig.CellSize);
        }
        for (int i = 0; i <= gridHeight; i++)
        {
            g.DrawLine(gridPen, 0, i * GameConfig.CellSize, gridWidth * GameConfig.CellSize, i * GameConfig.CellSize);
        }
    }

    private void DrawObstacles(Graphics g)
    {
        var obstacles = _gameEngine.GetObstacles();

        foreach (var obstacle in obstacles)
        {
            int x = obstacle.Position.X * GameConfig.CellSize;
            int y = obstacle.Position.Y * GameConfig.CellSize;

            switch (obstacle)
            {
                case StaticObstacle:
                    // 静态障碍物：灰色方块
                    using (var brush = new SolidBrush(Color.FromArgb(100, 100, 100)))
                    {
                        g.FillRectangle(brush, x + 1, y + 1, GameConfig.CellSize - 2, GameConfig.CellSize - 2);
                    }
                    using (var pen = new Pen(Color.FromArgb(80, 80, 80), 2))
                    {
                        g.DrawRectangle(pen, x + 1, y + 1, GameConfig.CellSize - 2, GameConfig.CellSize - 2);
                    }
                    break;

                case DestructibleObstacle destructible:
                    // 可破坏障碍物：棕色方块，带裂纹效果
                    int alpha = 100 + (destructible.RemainingPasses * 50);
                    using (var brush = new SolidBrush(Color.FromArgb(Math.Min(255, alpha), 139, 69, 19)))
                    {
                        g.FillRectangle(brush, x + 1, y + 1, GameConfig.CellSize - 2, GameConfig.CellSize - 2);
                    }
                    // 画裂纹
                    using (var pen = new Pen(Color.FromArgb(200, 100, 50, 0), 1))
                    {
                        g.DrawLine(pen, x + 3, y + 3, x + GameConfig.CellSize - 5, y + GameConfig.CellSize - 5);
                        g.DrawLine(pen, x + GameConfig.CellSize - 5, y + 3, x + 3, y + GameConfig.CellSize - 5);
                    }
                    break;

                case DynamicObstacle:
                    // 动态障碍物：紫色圆形，带移动箭头
                    using (var brush = new SolidBrush(Color.FromArgb(180, 128, 0, 128)))
                    {
                        g.FillEllipse(brush, x + 2, y + 2, GameConfig.CellSize - 4, GameConfig.CellSize - 4);
                    }
                    using (var pen = new Pen(Color.White, 2))
                    {
                        g.DrawEllipse(pen, x + 2, y + 2, GameConfig.CellSize - 4, GameConfig.CellSize - 4);
                        // 画箭头指示可以移动
                        g.DrawLine(pen, x + GameConfig.CellSize / 2, y + 4, x + GameConfig.CellSize / 2, y + GameConfig.CellSize - 4);
                        g.DrawLine(pen, x + 4, y + GameConfig.CellSize / 2, x + GameConfig.CellSize - 4, y + GameConfig.CellSize / 2);
                    }
                    break;

                case SpecialEffectObstacle special:
                    // 特殊效果障碍物：根据类型绘制
                    DrawSpecialEffectObstacle(g, special, x, y);
                    break;
            }
        }
    }

    private void DrawSpecialEffectObstacle(Graphics g, SpecialEffectObstacle obstacle, int x, int y)
    {
        Color color;
        string symbol;

        switch (obstacle.Type)
        {
            case ObstacleType.SpeedUp:
                color = Color.FromArgb(0, 200, 255);
                symbol = "⚡";
                break;
            case ObstacleType.SpeedDown:
                color = Color.FromArgb(100, 100, 255);
                symbol = "🐢";
                break;
            case ObstacleType.ScoreMultiplier:
                color = Color.FromArgb(255, 215, 0);
                symbol = "×2";
                break;
            case ObstacleType.Teleport:
                color = Color.FromArgb(148, 0, 211);
                symbol = "🌀";
                break;
            default:
                color = Color.Gray;
                symbol = "?";
                break;
        }

        // 绘制发光背景
        using (var brush = new SolidBrush(Color.FromArgb(150, color)))
        {
            g.FillEllipse(brush, x + 2, y + 2, GameConfig.CellSize - 4, GameConfig.CellSize - 4);
        }

        // 绘制符号
        using (var font = new Font("Segoe UI Emoji", 10))
        using (var brush = new SolidBrush(Color.White))
        {
            g.DrawString(symbol, font, brush, x + 1, y - 2);
        }
    }

    private void DrawFoods(Graphics g)
    {
        for (int i = 0; i < _gameState.Foods.Count; i++)
        {
            Color foodColor = _gameState.Difficulty switch
            {
                Difficulty.Easy => i == 0 ? Color.Red : Color.Orange,
                Difficulty.Medium => Color.Red,
                Difficulty.Hard => Color.Gold,
                _ => Color.Red
            };

            using Brush foodBrush = new SolidBrush(foodColor);
            g.FillEllipse(foodBrush,
                _gameState.Foods[i].X * GameConfig.CellSize + 2,
                _gameState.Foods[i].Y * GameConfig.CellSize + 2,
                GameConfig.CellSize - 4,
                GameConfig.CellSize - 4);
        }
    }

    private void DrawSnake(Graphics g)
    {
        int gridWidth = _gameState.CurrentLevel?.GridWidth ?? GameConfig.GridSize;
        int gridHeight = _gameState.CurrentLevel?.GridHeight ?? GameConfig.GridSize;

        using Brush headBrush = new SolidBrush(Color.Lime);
        using Brush bodyBrush = new SolidBrush(Color.FromArgb(0, 180, 0));
        {
            int index = 0;
            var current = _gameState.Snake.First;
            while (current != null)
            {
                int x = current.Value.X;
                int y = current.Value.Y;

                // 只渲染在有效网格范围内的蛇身部分
                // 这样蛇头撞墙时不会渲染到墙外面
                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                {
                    g.FillRectangle(index == 0 ? headBrush : bodyBrush,
                        x * GameConfig.CellSize + 1,
                        y * GameConfig.CellSize + 1,
                        GameConfig.CellSize - 2,
                        GameConfig.CellSize - 2);
                }

                current = current.Next;
                index++;
            }
        }
    }

    private void DrawGameOver(Graphics g)
    {
        using Font titleFont = new Font("Microsoft YaHei UI", _gameState.IsNewHighScore ? 22 : 24, FontStyle.Bold);
        using Font infoFont = new Font("Microsoft YaHei UI", 14);
        using Brush titleBrush = new SolidBrush(_gameState.IsNewHighScore ? Color.Gold : Color.Yellow);
        using Brush infoBrush = new SolidBrush(Color.White);
        using StringFormat format = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Near
        };

        float centerY = 10;

        if (_gameState.IsNewHighScore)
        {
            g.DrawString("🎉 新纪录！🎉", titleFont, titleBrush,
                ClientSize.Width / 2f, centerY, format);
            centerY += 40;
        }
        else
        {
            g.DrawString("游戏结束", titleFont, titleBrush,
                ClientSize.Width / 2f, centerY, format);
            centerY += 35;
        }

        g.DrawString($"最终分数: {_gameState.Score}", infoFont, infoBrush,
            ClientSize.Width / 2f, centerY, format);
        centerY += 25;

        // 这里需要访问最高分，暂时从状态中获取
        g.DrawString($"蛇的长度: {_gameState.Snake.Count}", infoFont, infoBrush,
            ClientSize.Width / 2f, centerY, format);
    }

    private void DrawPaused(Graphics g)
    {
        using Font font = new Font("Microsoft YaHei UI", 32, FontStyle.Bold);
        using Brush brush = new SolidBrush(Color.White);
        using StringFormat format = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        g.DrawString("暂停", font, brush,
            ClientSize.Width / 2f,
            ClientSize.Height / 2f, format);
    }
}
