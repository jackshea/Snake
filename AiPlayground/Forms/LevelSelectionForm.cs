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
    private TableLayoutPanel _mainPanel = null!;

    public string? SelectedLevelId { get; private set; }

    public LevelSelectionForm(LevelManager levelManager)
    {
        _levelManager = levelManager;

        Text = "选择关卡";
        Size = new Size(780, 500);
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
        // 顶部标题栏 - 使用Panel
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            Padding = new Padding(10, 5, 10, 5),
            BackColor = Color.FromArgb(35, 35, 35)
        };

        var titleLabel = new Label
        {
            Text = "选择关卡",
            Font = new Font("Microsoft YaHei UI", 16, FontStyle.Bold),
            ForeColor = Color.Gold,
            Location = new Point(10, 8),
            AutoSize = true
        };

        var closeButton = new Button
        {
            Text = "×",
            BackColor = Color.FromArgb(200, 50, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold),
            Size = new Size(35, 25),
            UseVisualStyleBackColor = false,
            Cursor = Cursors.Hand
        };
        closeButton.Click += (s, e) => Close();

        headerPanel.Controls.Add(titleLabel);
        headerPanel.Controls.Add(closeButton);
        closeButton.Location = new Point(headerPanel.ClientSize.Width - closeButton.Width - 5, 7);

        // 主面板
        _mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 1,
            ColumnCount = 1,
            Padding = new Padding(10),
            AutoScroll = true
        };
        _mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Controls.Add(_mainPanel);
        Controls.Add(headerPanel);
    }

    private void LoadLevels()
    {
        _mainPanel.Controls.Clear();
        _mainPanel.RowCount = 0;
        _mainPanel.ColumnStyles.Clear();
        _mainPanel.RowStyles.Clear();

        // 加载预设关卡
        var presetLevels = _levelManager.UnlockedPresetLevels;
        if (presetLevels.Count > 0)
        {
            AddSectionHeader("预设关卡");

            // 创建关卡行（每行4个关卡）
            for (int i = 0; i < presetLevels.Count; i += 4)
            {
                AddLevelRow(presetLevels, i, Math.Min(i + 4, presetLevels.Count));
            }
        }

        // 加载自定义关卡
        var customLevels = _levelManager.CustomLevels;
        if (customLevels.Count > 0)
        {
            AddSectionHeader("自定义关卡");

            for (int i = 0; i < customLevels.Count; i += 4)
            {
                AddLevelRow(customLevels, i, Math.Min(i + 4, customLevels.Count));
            }
        }
    }

    private void AddSectionHeader(string text)
    {
        _mainPanel.RowCount++;
        _mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

        var header = new Label
        {
            Text = text,
            Font = new Font("Microsoft YaHei UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 3, 0, 0)
        };
        _mainPanel.Controls.Add(header, 0, _mainPanel.RowCount - 1);
    }

    private void AddLevelRow(System.Collections.Generic.IReadOnlyList<Level> levels, int start, int end)
    {
        _mainPanel.RowCount++;
        _mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 155));

        // 使用TableLayoutPanel直接布局这一行的关卡
        var rowLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 1,
            ColumnCount = end - start,
            Padding = new Padding(3)
        };

        for (int i = 0; i < end - start; i++)
        {
            rowLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        }

        // 添加关卡卡片
        for (int i = start; i < end; i++)
        {
            var card = CreateLevelCard(levels[i]);
            rowLayout.Controls.Add(card, i - start, 0);
        }

        _mainPanel.Controls.Add(rowLayout, 0, _mainPanel.RowCount - 1);
    }

    private Panel CreateLevelCard(Level level)
    {
        var completion = _levelManager.GetLevelCompletion(level.Id);
        bool isCompleted = completion?.IsCompleted ?? false;

        var card = new Panel
        {
            BackColor = Color.FromArgb(60, 60, 60),
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(3),
            Padding = new Padding(6)
        };

        // 使用TableLayoutPanel来布局卡片内容
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 6,
            ColumnCount = 1,
            Padding = new Padding(0)
        };

        // 设置行高
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18)); // 序号
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22)); // 名称
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // 描述
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18)); // 状态
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18)); // 最高分
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // 按钮

        // 关卡序号标签
        var numberLabel = new Label
        {
            Text = level.IsCustom ? "自定义" : $"关卡 {level.LevelNumber}",
            Font = new Font("Microsoft YaHei UI", 8),
            ForeColor = Color.Gray,
            Dock = DockStyle.Fill
        };

        // 关卡名称
        var nameLabel = new Label
        {
            Text = level.Name,
            Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Fill
        };

        // 关卡描述
        var descLabel = new Label
        {
            Text = level.Description,
            Font = new Font("Microsoft YaHei UI", 7),
            ForeColor = Color.LightGray,
            Dock = DockStyle.Fill
        };

        // 完成状态
        var statusLabel = new Label
        {
            Text = isCompleted ? "✓ 已完成" : "未完成",
            Font = new Font("Microsoft YaHei UI", 8, FontStyle.Bold),
            ForeColor = isCompleted ? Color.Lime : Color.Orange,
            Dock = DockStyle.Fill
        };

        // 最高分
        var bestScoreLabel = new Label
        {
            Text = completion != null && completion.IsCompleted
                ? $"最高分: {completion.BestScore}"
                : "尚未完成",
            Font = new Font("Microsoft YaHei UI", 7),
            ForeColor = Color.Cyan,
            Dock = DockStyle.Fill
        };

        // 开始按钮
        var startButton = new Button
        {
            Text = "开始",
            BackColor = Color.FromArgb(50, 150, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Microsoft YaHei UI", 9),
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
        layout.Controls.Add(startButton, 0, 5);

        card.Controls.Add(layout);
        return card;
    }
}
