using System;
using System.Drawing;
using System.Windows.Forms;
using AiPlayground.Game;
using AiPlayground.Models;

namespace AiPlayground.Forms;

/// <summary>
/// 关卡完成界面
/// </summary>
public class LevelCompleteForm : Form
{
    private readonly Level _level;
    private readonly GameState _gameState;
    private readonly LevelManager _levelManager;

    public bool PlayNextLevel { get; private set; }

    public LevelCompleteForm(Level level, GameState gameState, LevelManager levelManager)
    {
        _level = level;
        _gameState = gameState;
        _levelManager = levelManager;

        Text = "关卡完成！";
        Size = new Size(500, 450);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(45, 45, 45);

        InitializeControls();
    }

    private void InitializeControls()
    {
        // 主面板
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(30)
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

        // 标题
        var titleLabel = new Label
        {
            Text = "🎉 关卡完成！🎉",
            Font = new Font("Microsoft YaHei UI", 24, FontStyle.Bold),
            ForeColor = Color.Gold,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };

        // 内容面板
        var contentPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 10, 20, 10)
        };

        // 关卡信息
        var levelLabel = CreateInfoLabel($"关卡 {_level.LevelNumber}: {_level.Name}");
        contentPanel.Controls.Add(levelLabel);

        // 分数
        var scoreLabel = CreateInfoLabel($"获得分数: {_gameState.Score}", Color.Lime);
        contentPanel.Controls.Add(scoreLabel);

        // 用时
        int minutes = _gameState.LevelTime / 60;
        int seconds = _gameState.LevelTime % 60;
        var timeLabel = CreateInfoLabel($"用时: {minutes}:{seconds:D2}");
        contentPanel.Controls.Add(timeLabel);

        // 蛇的长度
        var lengthLabel = CreateInfoLabel($"蛇的长度: {_gameState.Snake.Count}");
        contentPanel.Controls.Add(lengthLabel);

        // 检查是否有下一关
        var nextLevel = _levelManager.GetLevelByNumber(_level.LevelNumber + 1);

        // 按钮面板
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight
        };

        // 重新挑战按钮
        var retryButton = CreateButton("重新挑战", Color.FromArgb(50, 100, 200));
        retryButton.Click += (s, e) =>
        {
            PlayNextLevel = false;
            DialogResult = DialogResult.OK;
            Close();
        };
        buttonPanel.Controls.Add(retryButton);

        // 下一关按钮
        if (nextLevel != null && nextLevel.IsUnlocked)
        {
            var nextButton = CreateButton("挑战下一关", Color.FromArgb(50, 150, 50));
            nextButton.Click += (s, e) =>
            {
                PlayNextLevel = true;
                DialogResult = DialogResult.OK;
                Close();
            };
            buttonPanel.Controls.Add(nextButton);
        }

        // 返回按钮
        var backButton = CreateButton("返回", Color.FromArgb(150, 50, 50));
        backButton.Click += (s, e) =>
        {
            PlayNextLevel = false;
            DialogResult = DialogResult.Cancel;
            Close();
        };
        buttonPanel.Controls.Add(backButton);

        mainPanel.Controls.Add(titleLabel, 0, 0);
        mainPanel.Controls.Add(contentPanel, 0, 1);
        mainPanel.Controls.Add(buttonPanel, 0, 2);

        Controls.Add(mainPanel);
    }

    private Label CreateInfoLabel(string text, Color? color = null)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Microsoft YaHei UI", 14),
            ForeColor = color ?? Color.White,
            AutoSize = true,
            Margin = new Padding(0, 5, 0, 5)
        };
    }

    private Button CreateButton(string text, Color backColor)
    {
        return new Button
        {
            Text = text,
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Microsoft YaHei UI", 12, FontStyle.Bold),
            Size = new Size(140, 45),
            Margin = new Padding(10),
            Cursor = Cursors.Hand
        };
    }
}
