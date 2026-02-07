using System;
using System.Windows.Forms;
using AiPlayground.Controls;
using AiPlayground.Forms;
using AiPlayground.Game;
using AiPlayground.Models;
using AiPlayground.Services;

namespace AiPlayground;

/// <summary>
/// 主窗体 - 只负责 UI 组装和事件委托
/// </summary>
public partial class MainForm : Form
{
    // UI 组件
    private MenuStrip _menuStrip = null!;
    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _statusLabel = null!;
    private GamePanel _gamePanel = null!;
    private InfoPanel _infoPanel = null!;
    private Button _startButton = null!;

    // 游戏组件
    private readonly GameState _gameState;
    private readonly GameEngine _gameEngine;
    private readonly HighScoreService _highScoreService;
    private readonly LevelManager _levelManager;
    private readonly LevelStorageService _levelStorageService;
    private readonly System.Windows.Forms.Timer _gameTimer;
    private readonly System.Windows.Forms.Timer _levelTimeTimer;

    // 数据
    private int _highScore;

    public MainForm()
    {
        // 启用双缓冲
        DoubleBuffered = true;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);
        UpdateStyles();
        KeyPreview = true;

        // 初始化游戏组件
        _gameState = new GameState();
        _gameEngine = new GameEngine(_gameState);
        _highScoreService = new HighScoreService();
        _levelStorageService = new LevelStorageService();
        _levelManager = new LevelManager(_levelStorageService);
        _gameTimer = new System.Windows.Forms.Timer();
        _levelTimeTimer = new System.Windows.Forms.Timer { Interval = 1000 }; // 每秒更新关卡时间

        // 订阅关卡完成事件
        _levelManager.LevelCompleted += OnLevelCompleted;

        // 加载最高分
        _highScore = _highScoreService.LoadHighScore();

        // 初始化游戏
        _gameEngine.Initialize();
        _gameTimer.Interval = _gameEngine.GetTimerInterval();
        _gameTimer.Tick += OnGameTick;
        _levelTimeTimer.Tick += OnLevelTimeTick;

        // 初始化 UI
        InitializeComponent();
        InitializeMenu();
        InitializeLayout();
        UpdateUI();
    }

    private void InitializeComponent()
    {
        // 窗体属性
        Text = "贪吃蛇游戏 - 贪吃蛇 v1.0";
        ClientSize = new Size(
            GameConfig.GridSize * GameConfig.CellSize + GameConfig.InfoPanelWidth,
            GameConfig.GridSize * GameConfig.CellSize + 25);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.Black;
    }

    private void InitializeMenu()
    {
        _menuStrip = new MenuStrip();

        // 游戏菜单
        var gameMenu = new ToolStripMenuItem("游戏(&G)");
        var newGameItem = new ToolStripMenuItem("新游戏(&N)", null, (s, e) => NewGame()) { ShortcutKeys = Keys.F2 };
        var pauseItem = new ToolStripMenuItem("暂停(&P)", null, (s, e) => TogglePause());
        var exitItem = new ToolStripMenuItem("退出(&X)", null, (s, e) => Close()) { ShortcutKeys = Keys.Alt | Keys.F4 };
        gameMenu.DropDownItems.Add(newGameItem);
        gameMenu.DropDownItems.Add(new ToolStripSeparator());
        gameMenu.DropDownItems.Add(pauseItem);
        gameMenu.DropDownItems.Add(new ToolStripSeparator());
        gameMenu.DropDownItems.Add(exitItem);

        // 难度菜单
        var difficultyMenu = new ToolStripMenuItem("难度(&D)");
        foreach (Difficulty difficulty in Enum.GetValues<Difficulty>())
        {
            var diff = difficulty;
            var item = new ToolStripMenuItem(GetDifficultyName(difficulty), null, (s, e) => SetDifficulty(diff))
            {
                Checked = _gameState.Difficulty == diff
            };
            difficultyMenu.DropDownItems.Add(item);
        }

        // 速度菜单
        var speedMenu = new ToolStripMenuItem("速度(&S)");
        for (int i = GameConfig.MinSpeed; i <= GameConfig.MaxSpeed; i++)
        {
            int speed = i;
            var item = new ToolStripMenuItem($"等级 {i} - {GetSpeedDescription(i)}", null, (s, e) => SetSpeed(speed))
            {
                Checked = _gameState.SpeedLevel == i
            };
            speedMenu.DropDownItems.Add(item);
        }

        // 关卡菜单
        var levelMenu = new ToolStripMenuItem("关卡(&L)");
        var selectLevelItem = new ToolStripMenuItem("选择关卡(&S)", null, (s, e) => ShowLevelSelection());
        var levelEditorItem = new ToolStripMenuItem("关卡编辑器(&E)", null, (s, e) => ShowLevelEditor());
        var resetProgressItem = new ToolStripMenuItem("重置进度(&R)", null, (s, e) => ResetProgression());
        levelMenu.DropDownItems.Add(selectLevelItem);
        levelMenu.DropDownItems.Add(levelEditorItem);
        levelMenu.DropDownItems.Add(new ToolStripSeparator());
        levelMenu.DropDownItems.Add(resetProgressItem);

        // 查看菜单
        var viewMenu = new ToolStripMenuItem("查看(&V)");
        viewMenu.DropDownItems.Add("最高分记录(&H)", null, (s, e) => GameHelp.ShowHighScores(_gameState.Score, _highScore));

        // 帮助菜单
        var helpMenu = new ToolStripMenuItem("帮助(&H)");
        var helpItem = new ToolStripMenuItem("游戏说明(&I)", null, (s, e) => GameHelp.ShowHelp()) { ShortcutKeys = Keys.F1 };
        var aboutItem = new ToolStripMenuItem("关于(&A)", null, (s, e) => GameHelp.ShowAbout());
        helpMenu.DropDownItems.Add(helpItem);
        helpMenu.DropDownItems.Add(new ToolStripSeparator());
        helpMenu.DropDownItems.Add(aboutItem);

        _menuStrip.Items.Add(gameMenu);
        _menuStrip.Items.Add(levelMenu);
        _menuStrip.Items.Add(difficultyMenu);
        _menuStrip.Items.Add(speedMenu);
        _menuStrip.Items.Add(viewMenu);
        _menuStrip.Items.Add(helpMenu);

        Controls.Add(_menuStrip);
    }

    private void InitializeLayout()
    {
        // 创建开始按钮
        _startButton = new Button
        {
            Text = "开始游戏",
            Size = new Size(120, 45),
            Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold),
            BackColor = Color.Lime,
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _startButton.FlatAppearance.BorderSize = 2;
        _startButton.FlatAppearance.BorderColor = Color.White;
        _startButton.Click += (s, e) => StartGame();

        // 创建游戏面板
        _gamePanel = new GamePanel(_gameState, _gameEngine)
        {
            Location = new Point(0, 24),
            Size = new Size(GameConfig.GridSize * GameConfig.CellSize, GameConfig.GridSize * GameConfig.CellSize)
        };
        _gamePanel.Controls.Add(_startButton);
        Controls.Add(_gamePanel);

        // 创建信息面板
        _infoPanel = new InfoPanel(_gameState, _highScore, _levelManager)
        {
            Location = new Point(GameConfig.GridSize * GameConfig.CellSize, 24),
            Size = new Size(GameConfig.InfoPanelWidth, GameConfig.GridSize * GameConfig.CellSize)
        };
        Controls.Add(_infoPanel);

        // 创建状态栏
        _statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel("点击按钮开始游戏 | 空格键暂停 | F1 帮助");
        _statusStrip.Items.Add(_statusLabel);
        Controls.Add(_statusStrip);

        // 居中按钮
        CenterButton();

        // 事件处理
        KeyDown += OnKeyDown;
    }

    private void OnGameTick(object? sender, EventArgs e)
    {
        if (_gameState.IsGameOver)
        {
            _gameTimer.Stop();
            _levelTimeTimer.Stop();

            if (_highScoreService.TryUpdateHighScore(_gameState.Score, ref _highScore))
            {
                _gameState.IsNewHighScore = true;
            }

            UpdateUI();
            ShowStartButton();
            _gamePanel.Invalidate();
            return;
        }

        if (_gameState.IsPaused)
        {
            _gameTimer.Stop();
            return;
        }

        // 更新动态障碍物
        _gameEngine.UpdateDynamicObstacles();

        // 移动蛇和检查碰撞
        _gameEngine.MoveSnake();
        _gameEngine.CheckCollisions();

        // 检查关卡通关条件
        if (_gameState.CurrentLevel != null && !_gameState.IsLevelCompleted)
        {
            if (_levelManager.CheckVictoryCondition(_gameState))
            {
                OnLevelCompleted(_gameState.CurrentLevel);
            }
        }

        _gamePanel.Invalidate();
        _infoPanel.Invalidate();
        UpdateUI();
    }

    private void OnLevelTimeTick(object? sender, EventArgs e)
    {
        _gameEngine.UpdateLevelTime();
        _infoPanel.Invalidate();
    }

    private async void OnLevelCompleted(Level level)
    {
        _gameState.IsLevelCompleted = true;
        _gameTimer.Stop();
        _levelTimeTimer.Stop();

        // 保存关卡进度
        await _levelManager.CompleteLevelAsync(_gameState);

        // 显示通关消息
        var message = $"恭喜完成关卡 {level.LevelNumber}!\n\n" +
                     $"分数: {_gameState.Score}\n" +
                     $"用时: {_gameState.LevelTime / 60}:{_gameState.LevelTime % 60:D2}\n\n" +
                     "下一关已解锁！";

        MessageBox.Show(message, "关卡完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // 显示开始按钮，但不重置游戏（让玩家看到完成状态）
        ShowStartButton();
        _gamePanel.Invalidate();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Up:
                HandleDirectionKey(GameConfig.DirectionUp);
                break;
            case Keys.Down:
                HandleDirectionKey(GameConfig.DirectionDown);
                break;
            case Keys.Left:
                HandleDirectionKey(GameConfig.DirectionLeft);
                break;
            case Keys.Right:
                HandleDirectionKey(GameConfig.DirectionRight);
                break;
            case Keys.Space:
                e.Handled = true;
                if (_gameState.IsWaitingToStart || _gameState.IsGameOver)
                    StartGame();
                else
                    TogglePause();
                break;
            case Keys.F2:
                e.Handled = true;
                NewGame();
                break;
            case Keys.F1:
                e.Handled = true;
                GameHelp.ShowHelp();
                break;
            case Keys.D1: SetSpeed(1); break;
            case Keys.D2: SetSpeed(2); break;
            case Keys.D3: SetSpeed(3); break;
            case Keys.D4: SetSpeed(4); break;
            case Keys.D5: SetSpeed(5); break;
            case Keys.D6: SetSpeed(6); break;
            case Keys.D7: SetSpeed(7); break;
            case Keys.D8: SetSpeed(8); break;
            case Keys.D9: SetSpeed(9); break;
            case Keys.D0: SetSpeed(10); break;
        }
    }

    private void HandleDirectionKey(Point direction)
    {
        if (!_gameState.IsWaitingToStart && !_gameState.IsGameOver)
        {
            _gameEngine.TryChangeDirection(direction);
        }
    }

    private void NewGame()
    {
        _gameState.CurrentLevel = null; // 清除关卡，回到自由模式
        _gameEngine.Initialize();
        _gameTimer.Interval = _gameEngine.GetTimerInterval();
        _levelTimeTimer.Stop();
        _statusLabel.Text = "点击按钮开始游戏 | 空格键暂停 | F1 帮助";
        ShowStartButton();
        UpdateUI();
    }

    private void ShowLevelSelection()
    {
        var wasRunning = _gameTimer.Enabled;
        if (wasRunning)
        {
            _gameTimer.Stop();
            _levelTimeTimer.Stop();
        }

        using var form = new LevelSelectionForm(_levelManager);
        if (form.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(form.SelectedLevelId))
        {
            if (_levelManager.TryLoadLevel(form.SelectedLevelId))
            {
                var level = _levelManager.CurrentLevel;
                if (level != null)
                {
                    _gameEngine.SetLevel(level);
                    _gameEngine.Initialize();
                    _statusLabel.Text = $"关卡 {level.LevelNumber}: {level.Name} | 目标: {_levelManager.GetVictoryConditionDescription()}";
                    ShowStartButton();
                    UpdateUI();
                }
            }
        }
        else if (wasRunning)
        {
            _gameTimer.Start();
            _levelTimeTimer.Start();
        }
    }

    private void ShowLevelEditor()
    {
        var wasRunning = _gameTimer.Enabled;
        if (wasRunning)
        {
            _gameTimer.Stop();
            _levelTimeTimer.Stop();
        }

        try
        {
            // TODO: 实现关卡编辑器
            MessageBox.Show(
                "关卡编辑器功能即将推出！\n\n您可以手动编辑 JSON 文件来创建自定义关卡。\n" +
                "自定义关卡保存在:\n" +
                "%AppData%\\AiPlayground\\Levels\\Custom\\",
                "关卡编辑器",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        finally
        {
            if (wasRunning && !_gameState.IsGameOver)
            {
                _gameTimer.Start();
                _levelTimeTimer.Start();
            }
        }
    }

    private void ResetProgression()
    {
        var result = MessageBox.Show(
            "确定要重置所有关卡进度吗？\n\n这将锁定所有关卡（除第一关外），并清除所有完成记录。",
            "重置进度",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            if (_levelManager.ResetProgression())
            {
                MessageBox.Show("关卡进度已重置！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateUI();
            }
            else
            {
                MessageBox.Show("重置进度失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void StartGame()
    {
        _gameEngine.StartGame();
        HideStartButton();
        _gameTimer.Interval = _gameEngine.GetTimerInterval();
        _gameTimer.Start();

        // 如果是关卡模式，启动关卡计时器
        if (_gameState.CurrentLevel != null)
        {
            _levelTimeTimer.Start();
        }

        _statusLabel.Text = "游戏进行中... | 空格键暂停";
    }

    private void TogglePause()
    {
        _gameEngine.TogglePause();
        if (_gameState.IsPaused)
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
        _gameState.Difficulty = difficulty;
        UpdateMenuCheckStates();
        NewGame();
        GameHelp.ShowDifficultyInfo(GetDifficultyName(difficulty));
    }

    private void SetSpeed(int level)
    {
        _gameState.SpeedLevel = Math.Clamp(level, GameConfig.MinSpeed, GameConfig.MaxSpeed);
        UpdateMenuCheckStates();
        _gameTimer.Interval = _gameEngine.GetTimerInterval();
    }

    private void UpdateUI()
    {
        Text = $"贪吃蛇 - 分数: {_gameState.Score} | 速度: {_gameState.SpeedLevel} | 难度: {GetDifficultyName(_gameState.Difficulty)}";
        _infoPanel.Invalidate();
    }

    private void UpdateMenuCheckStates()
    {
        // 更新难度菜单
        var difficultyMenu = _menuStrip.Items[1] as ToolStripMenuItem;
        if (difficultyMenu != null)
        {
            for (int i = 0; i < difficultyMenu.DropDownItems.Count; i++)
            {
                if (difficultyMenu.DropDownItems[i] is ToolStripMenuItem item)
                {
                    var diff = (Difficulty)i;
                    item.Checked = _gameState.Difficulty == diff;
                }
            }
        }

        // 更新速度菜单
        var speedMenu = _menuStrip.Items[2] as ToolStripMenuItem;
        if (speedMenu != null)
        {
            for (int i = 0; i < speedMenu.DropDownItems.Count; i++)
            {
                if (speedMenu.DropDownItems[i] is ToolStripMenuItem item)
                {
                    item.Checked = (i + 1) == _gameState.SpeedLevel;
                }
            }
        }

        _infoPanel.Invalidate();
    }

    private void CenterButton()
    {
        _startButton.Location = new Point(
            (_gamePanel.ClientSize.Width - _startButton.Width) / 2,
            (_gamePanel.ClientSize.Height - _startButton.Height) / 2
        );
    }

    private void ShowStartButton()
    {
        CenterButton();
        _startButton.Visible = true;
        _startButton.BringToFront();
    }

    private void HideStartButton()
    {
        _startButton.Visible = false;
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

    private string GetSpeedDescription(int level)
    {
        return level switch
        {
            1 => "超慢", 2 => "很慢", 3 => "慢", 4 => "较慢", 5 => "中等",
            6 => "较快", 7 => "快", 8 => "很快", 9 => "超快", 10 => "极速",
            _ => "未知"
        };
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _gameTimer?.Dispose();
        _levelTimeTimer?.Dispose();
        base.OnFormClosing(e);
    }
}
