using System;
using System.Drawing;
using System.Windows.Forms;
using AiPlayground.Game;
using AiPlayground.Models;

namespace AiPlayground.Forms;

/// <summary>
/// 测试游戏窗体 - 在独立窗口中测试关卡
/// </summary>
public partial class TestGameForm : Form
{
    private readonly GameState _gameState;
    private readonly GameEngine _gameEngine;
    private readonly LevelManager _levelManager;
    private GamePanel _gamePanel = null!;
    private InfoPanel _infoPanel = null!;
    private readonly System.Windows.Forms.Timer _gameTimer;
    private readonly System.Windows.Forms.Timer _levelTimeTimer;
    private bool _isShowingLevelComplete;

    public TestGameForm(Level level, LevelManager levelManager)
    {
        _gameState = new GameState();
        _gameEngine = new GameEngine(_gameState);
        _levelManager = levelManager;
        _gameTimer = new System.Windows.Forms.Timer();
        _levelTimeTimer = new System.Windows.Forms.Timer { Interval = 1000 };

        _gameEngine.SetLevel(level);
        _gameEngine.Initialize();
        _gameTimer.Interval = _gameEngine.GetTimerInterval();
        _gameTimer.Tick += OnGameTick;
        _levelTimeTimer.Tick += OnLevelTimeTick;

        InitializeForm();
        InitializeLayout();
        StartTest();
    }

    private void InitializeForm()
    {
        Text = $"测试关卡 - {_gameState.CurrentLevel?.Name ?? "未命名"}";
        ClientSize = new Size(
            GameConfig.GridSize * GameConfig.CellSize + GameConfig.InfoPanelWidth,
            GameConfig.GridSize * GameConfig.CellSize + 25);
        FormBorderStyle = FormBorderStyle.Sizable;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.Black;
    }

    private void InitializeLayout()
    {
        _gamePanel = new GamePanel(_gameState, _gameEngine)
        {
            Location = new Point(0, 25),
            Size = new Size(GameConfig.GridSize * GameConfig.CellSize, GameConfig.GridSize * GameConfig.CellSize)
        };

        _infoPanel = new InfoPanel(_gameState, 0, _levelManager)
        {
            Location = new Point(_gamePanel.Right, 25),
            Size = new Size(GameConfig.InfoPanelWidth, _gamePanel.Height)
        };

        Controls.Add(_gamePanel);
        Controls.Add(_infoPanel);

        var instructionsLabel = new Label
        {
            Text = "F1 - 帮助 | F2 - 重新开始 | ESC - 关闭",
            Location = new Point(10, 5),
            AutoSize = true,
            ForeColor = Color.White
        };
        Controls.Add(instructionsLabel);

        KeyPreview = true;
        KeyDown += OnKeyDown;
    }

    private void StartTest()
    {
        _gameState.IsWaitingToStart = false;
        _gameTimer.Start();
        _levelTimeTimer.Start();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Up:
            case Keys.W:
                _gameEngine.TryChangeDirection(GameConfig.DirectionUp);
                break;
            case Keys.Down:
            case Keys.S:
                _gameEngine.TryChangeDirection(GameConfig.DirectionDown);
                break;
            case Keys.Left:
            case Keys.A:
                _gameEngine.TryChangeDirection(GameConfig.DirectionLeft);
                break;
            case Keys.Right:
            case Keys.D:
                _gameEngine.TryChangeDirection(GameConfig.DirectionRight);
                break;
            case Keys.Space:
                TogglePause();
                break;
            case Keys.F2:
                RestartTest();
                break;
            case Keys.Escape:
                Close();
                break;
            case Keys.F1:
                ShowHelp();
                break;
        }
    }

    private void OnGameTick(object? sender, EventArgs e)
    {
        if (_gameState.IsPaused) return;

        try
        {
            _gameEngine.MoveSnake();
            _gameEngine.CheckCollisions();
            _gameEngine.UpdateDynamicObstacles();

            // 检查通关条件
            if (_gameState.CurrentLevel != null && !_gameState.IsLevelCompleted && !_isShowingLevelComplete)
            {
                if (_levelManager.CheckVictoryCondition(_gameState))
                {
                    _gameTimer.Stop();
                    _levelTimeTimer.Stop();
                    _gameState.IsLevelCompleted = true;
                    _isShowingLevelComplete = true;
                    OnLevelCompleted();
                    return;
                }
            }

            // 更新 UI
            _gamePanel.Invalidate();
            _infoPanel.Invalidate();
        }
        finally
        {
            _isShowingLevelComplete = false;
        }
    }

    private void OnLevelTimeTick(object? sender, EventArgs e)
    {
        if (!_gameState.IsPaused && !_gameState.IsGameOver && !_gameState.IsLevelCompleted)
        {
            _gameState.LevelTime++;
            _infoPanel.Invalidate();
        }
    }

    private void OnLevelCompleted()
    {
        var result = MessageBox.Show(
            $"恭喜通关！\n\n" +
            $"分数: {_gameState.Score}\n" +
            $"用时: {_gameState.LevelTime} 秒\n" +
            $"蛇长: {_gameState.Snake.Count}\n\n" +
            "是否重新测试？",
            "关卡完成",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information);

        if (result == DialogResult.Yes)
        {
            RestartTest();
        }
        else
        {
            Close();
        }
    }

    private void TogglePause()
    {
        if (_gameState.IsGameOver || _gameState.IsLevelCompleted)
        {
            RestartTest();
            return;
        }

        _gameState.IsPaused = !_gameState.IsPaused;
        _gamePanel.Invalidate();
    }

    private void RestartTest()
    {
        _gameTimer.Stop();
        _levelTimeTimer.Stop();

        if (_gameState.CurrentLevel != null)
        {
            _gameEngine.SetLevel(_gameState.CurrentLevel);
        }
        _gameEngine.Initialize();
        _gameTimer.Interval = _gameEngine.GetTimerInterval();

        _isShowingLevelComplete = false;
        StartTest();

        _gamePanel.Invalidate();
        _infoPanel.Invalidate();
    }

    private void ShowHelp()
    {
        MessageBox.Show(
            "测试模式控制：\n\n" +
            "方向键 / WASD - 控制蛇移动\n" +
            "空格键 - 暂停/继续\n" +
            "F2 - 重新开始测试\n" +
            "ESC - 关闭测试窗口",
            "测试帮助",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _gameTimer.Stop();
        _levelTimeTimer.Stop();
        base.OnFormClosing(e);
    }
}
