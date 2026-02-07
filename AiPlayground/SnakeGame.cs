using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AiPlayground;

public class SnakeGame : Form
{
    private const int GridSize = 20;
    private const int CellSize = 20;
    private const int TimerInterval = 100;

    private readonly System.Windows.Forms.Timer _gameTimer;
    private readonly Random _random;

    private List<Point> _snake;
    private Point _food;
    private Point _direction;
    private bool _gameOver;
    private int _score;

    public SnakeGame()
    {
        _random = new Random();
        _snake = new List<Point>();
        _gameTimer = new System.Windows.Forms.Timer();
        _direction = new Point(1, 0);
        _gameOver = false;
        _score = 0;

        InitializeGame();
        InitializeForm();
    }

    private void InitializeForm()
    {
        Text = "贪吃蛇游戏 - 分数: 0";
        ClientSize = new Size(GridSize * CellSize, GridSize * CellSize);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.Black;

        KeyDown += OnKeyDown;
        Paint += OnPaint;

        _gameTimer.Interval = TimerInterval;
        _gameTimer.Tick += OnGameTick;
        _gameTimer.Start();
    }

    private void InitializeGame()
    {
        // 初始化蛇（从中间开始，长度为3）
        int startX = GridSize / 2;
        int startY = GridSize / 2;

        _snake.Clear();
        _snake.Add(new Point(startX, startY));
        _snake.Add(new Point(startX - 1, startY));
        _snake.Add(new Point(startX - 2, startY));

        _direction = new Point(1, 0);
        _score = 0;
        _gameOver = false;

        SpawnFood();
    }

    private void SpawnFood()
    {
        Point newFood;
        do
        {
            newFood = new Point(
                _random.Next(GridSize),
                _random.Next(GridSize)
            );
        } while (_snake.Contains(newFood));

        _food = newFood;
    }

    private void OnGameTick(object? sender, EventArgs e)
    {
        if (_gameOver)
        {
            _gameTimer.Stop();
            MessageBox.Show($"游戏结束！最终分数: {_score}", "游戏结束", MessageBoxButtons.OK, MessageBoxIcon.Information);
            InitializeGame();
            _gameTimer.Start();
            return;
        }

        MoveSnake();
        CheckCollisions();
        Invalidate();
    }

    private void MoveSnake()
    {
        // 计算新的头部位置
        Point head = _snake[0];
        Point newHead = new Point(head.X + _direction.X, head.Y + _direction.Y);

        // 将新头部添加到蛇身前面
        _snake.Insert(0, newHead);

        // 检查是否吃到食物
        if (newHead == _food)
        {
            _score += 10;
            Text = $"贪吃蛇游戏 - 分数: {_score}";
            SpawnFood();
        }
        else
        {
            // 如果没有吃到食物，移除尾部
            _snake.RemoveAt(_snake.Count - 1);
        }
    }

    private void CheckCollisions()
    {
        Point head = _snake[0];

        // 检查墙壁碰撞
        if (head.X < 0 || head.X >= GridSize || head.Y < 0 || head.Y >= GridSize)
        {
            _gameOver = true;
            return;
        }

        // 检查自身碰撞（从身体第二个段开始检查）
        for (int i = 1; i < _snake.Count; i++)
        {
            if (head == _snake[i])
            {
                _gameOver = true;
                return;
            }
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Up when _direction.Y != 1:
                _direction = new Point(0, -1);
                break;
            case Keys.Down when _direction.Y != -1:
                _direction = new Point(0, 1);
                break;
            case Keys.Left when _direction.X != 1:
                _direction = new Point(-1, 0);
                break;
            case Keys.Right when _direction.X != -1:
                _direction = new Point(1, 0);
                break;
            case Keys.R:
                InitializeGame();
                Text = $"贪吃蛇游戏 - 分数: {_score}";
                break;
        }
    }

    private void OnPaint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // 绘制网格（可选，帮助玩家看到边界）
        using (Pen gridPen = new Pen(Color.FromArgb(30, Color.Gray), 1))
        {
            for (int i = 0; i <= GridSize; i++)
            {
                g.DrawLine(gridPen, i * CellSize, 0, i * CellSize, GridSize * CellSize);
                g.DrawLine(gridPen, 0, i * CellSize, GridSize * CellSize, i * CellSize);
            }
        }

        // 绘制食物
        using (Brush foodBrush = new SolidBrush(Color.Red))
        {
            g.FillEllipse(foodBrush,
                _food.X * CellSize + 2,
                _food.Y * CellSize + 2,
                CellSize - 4,
                CellSize - 4);
        }

        // 绘制蛇
        for (int i = 0; i < _snake.Count; i++)
        {
            Brush segmentBrush = i == 0
                ? new SolidBrush(Color.Lime) // 蛇头
                : new SolidBrush(Color.FromArgb(0, 180, 0)); // 蛇身

            g.FillRectangle(segmentBrush,
                _snake[i].X * CellSize + 1,
                _snake[i].Y * CellSize + 1,
                CellSize - 2,
                CellSize - 2);

            segmentBrush.Dispose();
        }

        // 如果游戏结束，显示"游戏结束"文字
        if (_gameOver)
        {
            using (Font font = new Font("Arial", 24, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.Yellow))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString("游戏结束!", font, brush, ClientSize.Width / 2f, ClientSize.Height / 2f, format);
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _gameTimer?.Dispose();
        base.OnFormClosing(e);
    }
}
