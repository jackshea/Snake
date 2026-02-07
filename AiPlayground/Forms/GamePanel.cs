using System;
using System.Drawing;
using System.Windows.Forms;
using AiPlayground.Controls;
using AiPlayground.Game;
using AiPlayground.Models;

namespace AiPlayground.Forms;

/// <summary>
/// 游戏面板 - 负责渲染游戏画面
/// </summary>
public class GamePanel : DoubleBufferPanel
{
    private readonly GameState _gameState;

    public GamePanel(GameState gameState)
    {
        _gameState = gameState;
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
        using Pen gridPen = new Pen(Color.FromArgb(30, Color.Gray), 1);
        for (int i = 0; i <= GameConfig.GridSize; i++)
        {
            g.DrawLine(gridPen, i * GameConfig.CellSize, 0, i * GameConfig.CellSize, GameConfig.GridSize * GameConfig.CellSize);
            g.DrawLine(gridPen, 0, i * GameConfig.CellSize, GameConfig.GridSize * GameConfig.CellSize, i * GameConfig.CellSize);
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
        using Brush headBrush = new SolidBrush(Color.Lime);
        using Brush bodyBrush = new SolidBrush(Color.FromArgb(0, 180, 0));
        {
            for (int i = 0; i < _gameState.Snake.Count; i++)
            {
                g.FillRectangle(i == 0 ? headBrush : bodyBrush,
                    _gameState.Snake[i].X * GameConfig.CellSize + 1,
                    _gameState.Snake[i].Y * GameConfig.CellSize + 1,
                    GameConfig.CellSize - 2,
                    GameConfig.CellSize - 2);
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
