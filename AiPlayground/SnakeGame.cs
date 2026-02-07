using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AiPlayground;

public enum Difficulty
{
    Easy = 1,      // 简单：食物多，速度慢
    Medium = 2,    // 中等：标准
    Hard = 3       // 困难：食物少，速度快
}

// 自定义双缓冲 Panel
public class DoubleBufferPanel : Panel
{
    public DoubleBufferPanel()
    {
        DoubleBuffered = true;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);
        UpdateStyles();
    }
}

public class SnakeGame : Form
{
    // 游戏常量
    private const int GridSize = 20;
    private const int CellSize = 20;
    private const int InfoPanelWidth = 200;
    private const int MinSpeed = 1;
    private const int MaxSpeed = 10;

    // 游戏状态
    private readonly System.Windows.Forms.Timer _gameTimer;
    private readonly Random _random;
    private List<Point> _snake;
    private List<Point> _foods;
    private Point _direction;
    private bool _gameOver;
    private bool _isPaused;
    private bool _waitingToStart;
    private bool _isNewHighScore;
    private int _score;
    private int _highScore;
    private int _speedLevel;
    private Difficulty _difficulty;

    // UI 组件
    private MenuStrip _menuStrip = null!;
    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _statusLabel = null!;
    private DoubleBufferPanel _gamePanel = null!;
    private DoubleBufferPanel _infoPanel = null!;
    private Button _startButton = null!;

    // 最高分存储路径
    private readonly string _highScoreFilePath;

    public SnakeGame()
    {
        // 启用双缓冲以防止闪烁
        DoubleBuffered = true;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true
        );
        UpdateStyles();

        // 初始化最高分文件路径
        _highScoreFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AiPlayground",
            "snake_highscore.txt"
        );

        _random = new Random();
        _snake = new List<Point>();
        _foods = new List<Point>();
        _gameTimer = new System.Windows.Forms.Timer();
        _direction = new Point(1, 0);
        _gameOver = false;
        _isPaused = false;
        _waitingToStart = true;
        _isNewHighScore = false;
        _score = 0;
        _speedLevel = 5;
        _difficulty = Difficulty.Medium;

        // 加载最高分
        LoadHighScore();

        InitializeUI();
        InitializeGame();
    }

    private void InitializeUI()
    {
        // 设置窗体属性
        Text = "贪吃蛇游戏 - 贪吃蛇 v1.0";
        ClientSize = new Size(GridSize * CellSize + InfoPanelWidth, GridSize * CellSize + 25);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.Black;
        KeyPreview = true;  // 窗体优先接收键盘事件

        // 创建菜单栏
        CreateMenu();

        // 创建开始按钮（先创建按钮，再添加到面板）
        _startButton = new Button
        {
            Text = "开始游戏",
            Size = new Size(120, 45),
            Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold),
            BackColor = Color.Lime,
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            TabStop = false  // 禁止按钮获得焦点
        };
        _startButton.FlatAppearance.BorderSize = 2;
        _startButton.FlatAppearance.BorderColor = Color.White;
        _startButton.Click += (s, e) => StartGame();

        // 创建游戏面板（使用双缓冲面板）
        _gamePanel = new DoubleBufferPanel
        {
            Location = new Point(0, 24),
            Size = new Size(GridSize * CellSize, GridSize * CellSize),
            BackColor = Color.Black,
            BorderStyle = BorderStyle.Fixed3D
        };
        _gamePanel.Paint += OnGamePaint;
        _gamePanel.Controls.Add(_startButton);
        Controls.Add(_gamePanel);

        // 创建信息面板（使用双缓冲面板）
        _infoPanel = new DoubleBufferPanel
        {
            Location = new Point(GridSize * CellSize, 24),
            Size = new Size(InfoPanelWidth, GridSize * CellSize),
            BackColor = Color.FromArgb(30, 30, 30),
            BorderStyle = BorderStyle.Fixed3D
        };
        _infoPanel.Paint += OnInfoPaint;
        Controls.Add(_infoPanel);

        // 创建状态栏
        _statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel("点击按钮开始游戏 | 空格键暂停 | F1 帮助");
        _statusStrip.Items.Add(_statusLabel);
        Controls.Add(_statusStrip);

        // 居中显示按钮
        CenterButton();

        // 事件处理
        KeyDown += OnKeyDown;
    }

    private void CreateMenu()
    {
        _menuStrip = new MenuStrip();

        // 游戏菜单
        var gameMenu = new ToolStripMenuItem("游戏(&G)");
        var newGameItem = new ToolStripMenuItem("新游戏(&N)", null, (s, e) => NewGame());
        newGameItem.ShortcutKeys = Keys.F2;
        var pauseItem = new ToolStripMenuItem("暂停(&P)", null, (s, e) => TogglePause());
        var exitItem = new ToolStripMenuItem("退出(&X)", null, (s, e) => Close());
        exitItem.ShortcutKeys = Keys.Alt | Keys.F4;

        gameMenu.DropDownItems.Add(newGameItem);
        gameMenu.DropDownItems.Add(new ToolStripSeparator());
        gameMenu.DropDownItems.Add(pauseItem);
        gameMenu.DropDownItems.Add(new ToolStripSeparator());
        gameMenu.DropDownItems.Add(exitItem);

        // 难度菜单
        var difficultyMenu = new ToolStripMenuItem("难度(&D)");

        var easyItem = new ToolStripMenuItem("简单(&E)", null, (s, e) => SetDifficulty(Difficulty.Easy));
        easyItem.Checked = _difficulty == Difficulty.Easy;

        var mediumItem = new ToolStripMenuItem("中等(&M)", null, (s, e) => SetDifficulty(Difficulty.Medium));
        mediumItem.Checked = _difficulty == Difficulty.Medium;

        var hardItem = new ToolStripMenuItem("困难(&H)", null, (s, e) => SetDifficulty(Difficulty.Hard));
        hardItem.Checked = _difficulty == Difficulty.Hard;

        difficultyMenu.DropDownItems.Add(easyItem);
        difficultyMenu.DropDownItems.Add(mediumItem);
        difficultyMenu.DropDownItems.Add(hardItem);

        // 速度菜单
        var speedMenu = new ToolStripMenuItem("速度(&S)");

        for (int i = MinSpeed; i <= MaxSpeed; i++)
        {
            int speed = i;
            var speedItem = new ToolStripMenuItem($"等级 {i} - {GetSpeedDescription(i)}", null, (s, e) => SetSpeed(speed));
            speedItem.Checked = _speedLevel == i;
            speedMenu.DropDownItems.Add(speedItem);
        }

        // 查看菜单
        var viewMenu = new ToolStripMenuItem("查看(&V)");
        var highScoreItem = new ToolStripMenuItem("最高分记录(&H)", null, (s, e) => ShowHighScores());
        viewMenu.DropDownItems.Add(highScoreItem);

        // 帮助菜单
        var helpMenu = new ToolStripMenuItem("帮助(&H)");
        var instructionsItem = new ToolStripMenuItem("游戏说明(&I)", null, (s, e) => ShowHelp());
        instructionsItem.ShortcutKeys = Keys.F1;
        var aboutItem = new ToolStripMenuItem("关于(&A)", null, (s, e) => ShowAbout());

        helpMenu.DropDownItems.Add(instructionsItem);
        helpMenu.DropDownItems.Add(new ToolStripSeparator());
        helpMenu.DropDownItems.Add(aboutItem);

        _menuStrip.Items.Add(gameMenu);
        _menuStrip.Items.Add(difficultyMenu);
        _menuStrip.Items.Add(speedMenu);
        _menuStrip.Items.Add(viewMenu);
        _menuStrip.Items.Add(helpMenu);

        Controls.Add(_menuStrip);
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

    private void InitializeGame()
    {
        // 初始化蛇（从中间开始，长度为3）
        int startX = GridSize / 2;
        int startY = GridSize / 2;

        _snake.Clear();
        _snake.Add(new Point(startX, startY));
        _snake.Add(new Point(startX - 1, startY));
        _snake.Add(new Point(startX - 2, startY));

        _foods.Clear();
        SpawnFood();

        _direction = new Point(1, 0);
        _score = 0;
        _gameOver = false;
        _isPaused = false;
        _waitingToStart = true;
        _isNewHighScore = false;

        _gameTimer.Stop();
        UpdateSpeed();
        UpdateUI();

        _statusLabel.Text = "点击按钮开始游戏 | 空格键暂停 | F1 帮助";
    }

    private void SpawnFood()
    {
        // 根据难度决定食物数量
        int foodCount = _difficulty switch
        {
            Difficulty.Easy => 3,
            Difficulty.Medium => 1,
            Difficulty.Hard => 1,
            _ => 1
        };

        // 补充食物到目标数量
        while (_foods.Count < foodCount)
        {
            Point newFood;
            do
            {
                newFood = new Point(
                    _random.Next(GridSize),
                    _random.Next(GridSize)
                );
            } while (_snake.Contains(newFood) || _foods.Contains(newFood));

            _foods.Add(newFood);
        }
    }

    private void NewGame()
    {
        InitializeGame();
        _statusLabel.Text = "点击按钮开始游戏 | 空格键暂停 | F1 帮助";
        ShowStartButton();
    }

    private void CenterButton()
    {
        if (_startButton != null && _gamePanel != null)
        {
            _startButton.Location = new Point(
                (_gamePanel.ClientSize.Width - _startButton.Width) / 2,
                (_gamePanel.ClientSize.Height - _startButton.Height) / 2
            );
        }
    }

    private void ShowStartButton()
    {
        if (_startButton != null)
        {
            CenterButton();
            _startButton.Visible = true;
            _startButton.BringToFront();
        }
    }

    private void HideStartButton()
    {
        if (_startButton != null)
        {
            _startButton.Visible = false;
        }
    }

    private void StartGame()
    {
        if (_waitingToStart || _gameOver)
        {
            if (_gameOver)
            {
                NewGame();
            }
            _waitingToStart = false;
            HideStartButton();
            _gameTimer.Start();
            _statusLabel.Text = "游戏进行中... | 空格键暂停";
        }
    }

    private void TogglePause()
    {
        if (_gameOver)
        {
            // 游戏结束后，按空格键开始新游戏
            NewGame();
            return;
        }

        if (_waitingToStart)
        {
            // 等待开始状态，直接开始游戏
            StartGame();
            return;
        }

        _isPaused = !_isPaused;
        if (_isPaused)
        {
            _gameTimer.Stop();
            _statusLabel.Text = "游戏已暂停 - 按空格键继续";
        }
        else
        {
            _gameTimer.Start();
            _statusLabel.Text = "游戏进行中... | 空格键暂停";
        }
        _infoPanel.Invalidate();
    }

    private void SetDifficulty(Difficulty difficulty)
    {
        _difficulty = difficulty;
        UpdateMenuCheckStates();
        InitializeGame();
        _statusLabel.Text = $"难度已设置为 {GetDifficultyName(difficulty)} - 按方向键开始";
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

    private void SetSpeed(int level)
    {
        _speedLevel = Math.Clamp(level, MinSpeed, MaxSpeed);
        UpdateMenuCheckStates();
        UpdateSpeed();
    }

    private void UpdateSpeed()
    {
        // 计算速度等级对应的间隔时间（毫秒）
        int baseInterval = _difficulty == Difficulty.Hard ? 80 : 150;
        int interval = baseInterval - (_speedLevel - 1) * 15;
        _gameTimer.Interval = Math.Max(40, interval);

        // 确保定时器有事件处理程序
        _gameTimer.Tick -= OnGameTick;
        _gameTimer.Tick += OnGameTick;
    }

    private void UpdateMenuCheckStates()
    {
        // 更新难度菜单的选中状态
        var difficultyMenu = _menuStrip.Items[1] as ToolStripMenuItem;
        if (difficultyMenu != null)
        {
            for (int i = 0; i < difficultyMenu.DropDownItems.Count; i++)
            {
                if (difficultyMenu.DropDownItems[i] is ToolStripMenuItem item)
                {
                    item.Checked = (i == 0 && _difficulty == Difficulty.Easy) ||
                                   (i == 1 && _difficulty == Difficulty.Medium) ||
                                   (i == 2 && _difficulty == Difficulty.Hard);
                }
            }
        }

        // 更新速度菜单的选中状态
        var speedMenu = _menuStrip.Items[2] as ToolStripMenuItem;
        if (speedMenu != null)
        {
            for (int i = 0; i < speedMenu.DropDownItems.Count; i++)
            {
                if (speedMenu.DropDownItems[i] is ToolStripMenuItem item)
                {
                    item.Checked = (i + 1) == _speedLevel;
                }
            }
        }

        _infoPanel.Invalidate();
    }

    private void UpdateUI()
    {
        Text = $"贪吃蛇 - 分数: {_score} | 速度: {_speedLevel} | 难度: {GetDifficultyName(_difficulty)}";
        _infoPanel.Invalidate();
    }

    private void OnGameTick(object? sender, EventArgs e)
    {
        if (_gameOver)
        {
            _gameTimer.Stop();

            // 检查是否打破最高分
            if (_score > _highScore)
            {
                _highScore = _score;
                _isNewHighScore = true;
                SaveHighScore();
            }

            UpdateUI();
            ShowStartButton();
            _gamePanel.Invalidate();
            return;
        }

        if (_isPaused)
        {
            _gameTimer.Stop();
            return;
        }

        MoveSnake();
        CheckCollisions();
        _gamePanel.Invalidate();
        _infoPanel.Invalidate();
    }

    private void MoveSnake()
    {
        // 计算新的头部位置
        Point head = _snake[0];
        Point newHead = new Point(head.X + _direction.X, head.Y + _direction.Y);

        // 将新头部添加到蛇身前面
        _snake.Insert(0, newHead);

        // 检查是否吃到食物
        bool ateFood = false;
        for (int i = _foods.Count - 1; i >= 0; i--)
        {
            if (newHead == _foods[i])
            {
                // 根据难度计算得分
                int points = _difficulty switch
                {
                    Difficulty.Easy => 5,
                    Difficulty.Medium => 10,
                    Difficulty.Hard => 20,
                    _ => 10
                };

                // 根据速度等级加成
                points += _speedLevel;

                _score += points;
                _foods.RemoveAt(i);
                SpawnFood();
                ateFood = true;
                break;
            }
        }

        if (!ateFood)
        {
            // 如果没有吃到食物，移除尾部
            _snake.RemoveAt(_snake.Count - 1);
        }

        UpdateUI();
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

    private void OnGamePaint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

        // 绘制网格
        using (Pen gridPen = new Pen(Color.FromArgb(30, Color.Gray), 1))
        {
            for (int i = 0; i <= GridSize; i++)
            {
                g.DrawLine(gridPen, i * CellSize, 0, i * CellSize, GridSize * CellSize);
                g.DrawLine(gridPen, 0, i * CellSize, GridSize * CellSize, i * CellSize);
            }
        }

        // 绘制食物（不同难度用不同颜色）
        for (int i = 0; i < _foods.Count; i++)
        {
            Color foodColor = _difficulty switch
            {
                Difficulty.Easy => i == 0 ? Color.Red : Color.Orange,
                Difficulty.Medium => Color.Red,
                Difficulty.Hard => Color.Gold,
                _ => Color.Red
            };

            using (Brush foodBrush = new SolidBrush(foodColor))
            {
                g.FillEllipse(foodBrush,
                    _foods[i].X * CellSize + 2,
                    _foods[i].Y * CellSize + 2,
                    CellSize - 4,
                    CellSize - 4);
            }
        }

        // 绘制蛇
        using (Brush headBrush = new SolidBrush(Color.Lime))
        using (Brush bodyBrush = new SolidBrush(Color.FromArgb(0, 180, 0)))
        {
            for (int i = 0; i < _snake.Count; i++)
            {
                g.FillRectangle(i == 0 ? headBrush : bodyBrush,
                    _snake[i].X * CellSize + 1,
                    _snake[i].Y * CellSize + 1,
                    CellSize - 2,
                    CellSize - 2);
            }
        }

        // 如果游戏结束，显示结果
        if (_gameOver)
        {
            using (Font titleFont = new Font("Microsoft YaHei UI", _isNewHighScore ? 22 : 24, FontStyle.Bold))
            using (Font infoFont = new Font("Microsoft YaHei UI", 14))
            using (Brush titleBrush = new SolidBrush(_isNewHighScore ? Color.Gold : Color.Yellow))
            using (Brush infoBrush = new SolidBrush(Color.White))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Near;

                float centerY = 10;

                if (_isNewHighScore)
                {
                    g.DrawString("🎉 新纪录！🎉", titleFont, titleBrush,
                        _gamePanel.ClientSize.Width / 2f, centerY, format);
                    centerY += 40;
                }
                else
                {
                    g.DrawString("游戏结束", titleFont, titleBrush,
                        _gamePanel.ClientSize.Width / 2f, centerY, format);
                    centerY += 35;
                }

                // 显示分数信息
                g.DrawString($"最终分数: {_score}", infoFont, infoBrush,
                    _gamePanel.ClientSize.Width / 2f, centerY, format);
                centerY += 25;

                g.DrawString($"最高分数: {_highScore}", infoFont, infoBrush,
                    _gamePanel.ClientSize.Width / 2f, centerY, format);
                centerY += 25;

                g.DrawString($"蛇的长度: {_snake.Count}", infoFont, infoBrush,
                    _gamePanel.ClientSize.Width / 2f, centerY, format);
            }
        }

        // 如果暂停，显示"暂停"文字
        if (_isPaused && !_gameOver && !_waitingToStart)
        {
            using (Font font = new Font("Microsoft YaHei UI", 32, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.White))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString("暂停", font, brush,
                    _gamePanel.ClientSize.Width / 2f,
                    _gamePanel.ClientSize.Height / 2f, format);
            }
        }
    }

    private void OnInfoPaint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        Font headerFont = new Font("Microsoft YaHei UI", 14, FontStyle.Bold);
        Font normalFont = new Font("Microsoft YaHei UI", 11);
        Font smallFont = new Font("Microsoft YaHei UI", 9);

        Brush whiteBrush = new SolidBrush(Color.White);
        Brush yellowBrush = new SolidBrush(Color.Yellow);
        Brush cyanBrush = new SolidBrush(Color.Cyan);
        Brush greenBrush = new SolidBrush(Color.Lime);
        Brush goldBrush = new SolidBrush(Color.Gold);

        int y = 15;
        int x = 15;

        // 标题
        g.DrawString("游戏信息", headerFont, yellowBrush, x, y);
        y += 35;

        // 分数
        g.DrawString("当前分数", normalFont, whiteBrush, x, y);
        y += 25;
        g.DrawString(_score.ToString(), headerFont, greenBrush, x, y);
        y += 40;

        // 最高分
        g.DrawString("最高分数", normalFont, whiteBrush, x, y);
        y += 25;
        Color highScoreColor = _isNewHighScore ? Color.Gold : Color.Cyan;
        using (Brush highScoreBrush = new SolidBrush(highScoreColor))
        {
            g.DrawString(_highScore.ToString(), headerFont, highScoreBrush, x, y);
        }
        y += 40;

        // 蛇的长度
        g.DrawString("蛇的长度", normalFont, whiteBrush, x, y);
        y += 25;
        g.DrawString(_snake.Count.ToString(), headerFont, yellowBrush, x, y);
        y += 40;

        // 速度等级
        g.DrawString("速度等级", normalFont, whiteBrush, x, y);
        y += 25;
        g.DrawString($"等级 {_speedLevel}/10", headerFont, yellowBrush, x, y);
        y += 25;
        g.DrawString(GetSpeedDescription(_speedLevel), smallFont, whiteBrush, x, y);
        y += 40;

        // 难度
        g.DrawString("难度级别", normalFont, whiteBrush, x, y);
        y += 25;
        g.DrawString(GetDifficultyName(_difficulty), headerFont, yellowBrush, x, y);
        y += 40;

        // 游戏状态
        g.DrawString("游戏状态", normalFont, whiteBrush, x, y);
        y += 25;
        string statusText = _waitingToStart ? "等待开始" : _gameOver ? "已结束" : _isPaused ? "已暂停" : "进行中";
        Color statusColor = _waitingToStart ? Color.Gray : _gameOver ? Color.Red : _isPaused ? Color.Orange : Color.Lime;
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

        // 清理
        headerFont.Dispose();
        normalFont.Dispose();
        smallFont.Dispose();
        whiteBrush.Dispose();
        yellowBrush.Dispose();
        cyanBrush.Dispose();
        greenBrush.Dispose();
        goldBrush.Dispose();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Up when !_waitingToStart && !_gameOver && _direction.Y != 1:
                _direction = new Point(0, -1);
                break;
            case Keys.Down when !_waitingToStart && !_gameOver && _direction.Y != -1:
                _direction = new Point(0, 1);
                break;
            case Keys.Left when !_waitingToStart && !_gameOver && _direction.X != 1:
                _direction = new Point(-1, 0);
                break;
            case Keys.Right when !_waitingToStart && !_gameOver && _direction.X != -1:
                _direction = new Point(1, 0);
                break;
            case Keys.Space:
                e.Handled = true;
                if (_waitingToStart || _gameOver)
                {
                    StartGame();
                }
                else
                {
                    TogglePause();
                }
                break;
            case Keys.F2:
                e.Handled = true;
                NewGame();
                break;
            case Keys.F1:
                e.Handled = true;
                ShowHelp();
                break;
            case Keys.D1:
                SetSpeed(1);
                break;
            case Keys.D2:
                SetSpeed(2);
                break;
            case Keys.D3:
                SetSpeed(3);
                break;
            case Keys.D4:
                SetSpeed(4);
                break;
            case Keys.D5:
                SetSpeed(5);
                break;
            case Keys.D6:
                SetSpeed(6);
                break;
            case Keys.D7:
                SetSpeed(7);
                break;
            case Keys.D8:
                SetSpeed(8);
                break;
            case Keys.D9:
                SetSpeed(9);
                break;
            case Keys.D0:
                SetSpeed(10);
                break;
        }
    }

    private void ShowHelp()
    {
        string helpText = @"贪吃蛇游戏 - 帮助

=== 游戏目标 ===
控制蛇吃掉食物，使蛇变长并获得分数。

=== 操作方法 ===
方向键 ↑↓←→  - 控制蛇的移动方向
空格键         - 暂停/继续游戏
开始按钮       - 开始新游戏
F2            - 重置游戏
F1            - 显示此帮助

=== 游戏规则 ===
• 吃到食物可以得分并使蛇变长
• 撞到墙壁或自己的身体游戏结束
• 在简单模式下有3个食物，中等和困难模式只有1个食物
• 困难模式下速度更快，但得分也更多

=== 难度级别 ===
• 简单：3个食物，每个食物 5 + 速度等级 分
• 中等：1个食物，每个食物 10 + 速度等级 分
• 困难：1个食物，速度快，每个食物 20 + 速度等级 分

=== 速度等级 ===
按数字键 0-9 可以快速调整速度等级
等级 1 = 最慢，等级 10 = 最快

=== 提示 ===
• 游戏会自动保存您的最高分
• 菜单中有更多选项可以调整游戏设置
• 暂停时可以调整速度和难度
• 点击开始按钮或按空格键开始游戏

祝您游戏愉快！";

        MessageBox.Show(helpText, "游戏帮助", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowAbout()
    {
        MessageBox.Show(
            "贪吃蛇 v1.0\n\n" +
            "使用 C# 和 WinForms 开发\n\n" +
            "功能特点：\n" +
            "• 3 种难度级别\n" +
            "• 10 档速度调节\n" +
            "• 最高分自动保存\n" +
            "• 暂停/继续功能\n" +
            "• 完整的菜单系统\n" +
            "• 无闪烁双缓冲渲染\n\n" +
            "© 2025 AiPlayground",
            "关于贪吃蛇",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void ShowHighScores()
    {
        MessageBox.Show(
            $"当前分数：{_score}\n\n" +
            $"历史最高：{_highScore}\n\n" +
            $"提示：打破最高分会自动保存！",
            "分数记录",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void LoadHighScore()
    {
        try
        {
            if (File.Exists(_highScoreFilePath))
            {
                string scoreText = File.ReadAllText(_highScoreFilePath);
                if (int.TryParse(scoreText, out int score))
                {
                    _highScore = score;
                }
            }
        }
        catch
        {
            _highScore = 0;
        }
    }

    private void SaveHighScore()
    {
        try
        {
            string directory = Path.GetDirectoryName(_highScoreFilePath)!;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(_highScoreFilePath, _highScore.ToString());
        }
        catch
        {
            // 忽略保存错误
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _gameTimer?.Dispose();
        base.OnFormClosing(e);
    }
}
