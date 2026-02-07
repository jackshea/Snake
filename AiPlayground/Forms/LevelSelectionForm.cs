using System;
using System.Drawing;
using System.Windows.Forms;
using AiPlayground.Game;
using AiPlayground.Models;

namespace AiPlayground.Forms;

/// <summary>
/// 关卡选择窗体
/// </summary>
public class LevelSelectionForm : Form
{
    private readonly LevelManager _levelManager;
    private FlowLayoutPanel _levelsPanel = null!;

    public string? SelectedLevelId { get; private set; }

    public LevelSelectionForm(LevelManager levelManager)
    {
        _levelManager = levelManager;

        Text = "选择关卡";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(45, 45, 45);

        CreateControls();
        LoadLevels();
    }

    private void CreateControls()
    {
        // 主面板
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            Padding = new Padding(20)
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // 标题栏
        var titlePanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60
        };

        var titleLabel = new Label
        {
            Text = "选择关卡",
            Font = new Font("Microsoft YaHei UI", 20, FontStyle.Bold),
            ForeColor = Color.Gold,
            Dock = DockStyle.Left,
            AutoSize = true
        };

        var closeButton = CreateButton("关闭", Color.FromArgb(200, 50, 50));
        closeButton.Click += (s, e) => Close();

        titlePanel.Controls.Add(titleLabel);
        titlePanel.Controls.Add(closeButton);

        // 关卡列表面板
        _levelsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoScroll = true,
            Padding = new Padding(10),
            BackColor = Color.Transparent
        };

        mainPanel.Controls.Add(titlePanel, 0, 0);
        mainPanel.Controls.Add(_levelsPanel, 0, 1);
        Controls.Add(mainPanel);
    }

    private void LoadLevels()
    {
        _levelsPanel.Controls.Clear();

        // 加载预设关卡
        var presetLevels = _levelManager.UnlockedPresetLevels;
        if (presetLevels.Count > 0)
        {
            var header = CreateSectionHeader("预设关卡");
            _levelsPanel.Controls.Add(header);

            foreach (var level in presetLevels)
            {
                var card = CreateLevelCard(level);
                _levelsPanel.Controls.Add(card);
            }
        }

        // 加载自定义关卡
        var customLevels = _levelManager.CustomLevels;
        if (customLevels.Count > 0)
        {
            var header = CreateSectionHeader("自定义关卡");
            _levelsPanel.Controls.Add(header);

            foreach (var level in customLevels)
            {
                var card = CreateLevelCard(level);
                _levelsPanel.Controls.Add(card);
            }
        }
    }

    private Label CreateSectionHeader(string text)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(10, 10, 10, 5),
            Padding = new Padding(0)
        };
    }

    private Panel CreateLevelCard(Level level)
    {
        var completion = _levelManager.GetLevelCompletion(level.Id);
        bool isCompleted = completion?.IsCompleted ?? false;

        var card = new Panel
        {
            Width = 220,
            Height = 200,
            BackColor = Color.FromArgb(60, 60, 60),
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(10),
            Padding = new Padding(10)
        };

        // 使用TableLayoutPanel来布局卡片内容
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 7,
            ColumnCount = 1,
            Padding = new Padding(0)
        };

        // 设置行高
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20)); // 序号
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25)); // 名称
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); // 描述
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20)); // 状态
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20)); // 最高分
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10)); // 间距
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // 按钮

        // 关卡序号标签
        var numberLabel = new Label
        {
            Text = level.IsCustom ? "自定义" : $"关卡 {level.LevelNumber}",
            Font = new Font("Microsoft YaHei UI", 9),
            ForeColor = Color.Gray,
            Dock = DockStyle.Fill
        };

        // 关卡名称
        var nameLabel = new Label
        {
            Text = level.Name,
            Font = new Font("Microsoft YaHei UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Fill
        };

        // 关卡描述
        var descLabel = new Label
        {
            Text = level.Description,
            Font = new Font("Microsoft YaHei UI", 8),
            ForeColor = Color.LightGray,
            Dock = DockStyle.Fill
        };

        // 完成状态
        var statusLabel = new Label
        {
            Text = isCompleted ? "✓ 已完成" : "未完成",
            Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold),
            ForeColor = isCompleted ? Color.Lime : Color.Orange,
            Dock = DockStyle.Fill
        };

        // 最高分
        var bestScoreLabel = new Label
        {
            Text = completion != null && completion.IsCompleted
                ? $"最高分: {completion.BestScore}"
                : "尚未完成",
            Font = new Font("Microsoft YaHei UI", 8),
            ForeColor = Color.Cyan,
            Dock = DockStyle.Fill
        };

        // 开始按钮
        var startButton = new Button
        {
            Text = "开始挑战",
            BackColor = Color.FromArgb(50, 150, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Microsoft YaHei UI", 10),
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Cursor = Cursors.Hand
        };
        startButton.Click += (s, e) =>
        {
            SelectedLevelId = level.Id;
            DialogResult = DialogResult.OK;
            Close();
        };

        layout.Controls.Add(numberLabel, 0, 0);
        layout.Controls.Add(nameLabel, 0, 1);
        layout.Controls.Add(descLabel, 0, 2);
        layout.Controls.Add(statusLabel, 0, 3);
        layout.Controls.Add(bestScoreLabel, 0, 4);
        layout.Controls.Add(startButton, 0, 6);

        card.Controls.Add(layout);
        return card;
    }

    private Button CreateButton(string text, Color backColor)
    {
        return new Button
        {
            Text = text,
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Microsoft YaHei UI", 10),
            Padding = new Padding(10, 5, 10, 5),
            AutoSize = true,
            Cursor = Cursors.Hand
        };
    }
}
