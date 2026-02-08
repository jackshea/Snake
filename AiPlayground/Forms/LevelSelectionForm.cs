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
        Size = new Size(750, 480);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(45, 45, 45);

        CreateControls();
        LoadLevels();
    }

    private void CreateControls()
    {
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
    }

    private void LoadLevels()
    {
        _mainPanel.Controls.Clear();
        _mainPanel.RowCount = 0;
        _mainPanel.ColumnStyles.Clear();
        _mainPanel.RowStyles.Clear();

        // 加载预设关卡
        var presetLevels = _levelManager.UnlockedPresetLevels;

        // 调试：显示实际加载的关卡数量
        var levelNumbers = string.Join(", ", presetLevels.Select(l => l.LevelNumber));
        MessageBox.Show($"已加载 {presetLevels.Count} 个预设关卡: {levelNumbers}", "调试信息");

        if (presetLevels.Count > 0)
        {
            AddSectionHeader("预设关卡");

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = false,
                Padding = new Padding(0)
            };

            foreach (var level in presetLevels)
            {
                flowPanel.Controls.Add(CreateLevelCard(level));
            }

            _mainPanel.RowCount++;
            _mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _mainPanel.Controls.Add(flowPanel, 0, _mainPanel.RowCount - 1);
        }

        // 加载自定义关卡
        var customLevels = _levelManager.CustomLevels;
        if (customLevels.Count > 0)
        {
            AddSectionHeader("自定义关卡");

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = false,
                Padding = new Padding(0)
            };

            foreach (var level in customLevels)
            {
                flowPanel.Controls.Add(CreateLevelCard(level));
            }

            _mainPanel.RowCount++;
            _mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _mainPanel.Controls.Add(flowPanel, 0, _mainPanel.RowCount - 1);
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

    private Control CreateLevelCard(Level level)
    {
        var completion = _levelManager.GetLevelCompletion(level.Id);
        bool isCompleted = completion?.IsCompleted ?? false;

        // 卡片主容器 - 使用Button作为整个卡片
        var cardButton = new Button
        {
            BackColor = Color.FromArgb(60, 60, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 1 },
            Size = new Size(170, 130),
            Margin = new Padding(4),
            Padding = new Padding(6),
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.TopLeft,
            UseVisualStyleBackColor = false
        };
        cardButton.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);

        // 在按钮上绘制内容
        cardButton.Paint += (s, e) =>
        {
            var g = e.Graphics;
            var fontNumber = new Font("Microsoft YaHei UI", 8);
            var fontName = new Font("Microsoft YaHei UI", 10, FontStyle.Bold);
            var fontDesc = new Font("Microsoft YaHei UI", 7);
            var fontStatus = new Font("Microsoft YaHei UI", 8, FontStyle.Bold);
            var fontScore = new Font("Microsoft YaHei UI", 7);

            var y = 6;
            // 序号
            g.DrawString(level.IsCustom ? "自定义" : $"关卡 {level.LevelNumber}",
                fontNumber, Brushes.Gray, 6, y);
            y += 18;

            // 名称
            g.DrawString(level.Name, fontName, Brushes.White, 6, y);
            y += 22;

            // 描述
            var descFormat = new StringFormat
            {
                Trimming = StringTrimming.EllipsisWord,
                FormatFlags = StringFormatFlags.LineLimit
            };
            g.DrawString(level.Description, fontDesc, Brushes.LightGray,
                new RectangleF(6, y, 158, 30), descFormat);
            y += 32;

            // 状态
            var statusText = isCompleted ? "✓ 已完成" : "未完成";
            var statusColor = isCompleted ? Brushes.Lime : Brushes.Orange;
            g.DrawString(statusText, fontStatus, statusColor, 6, y);
            y += 18;

            // 最高分
            var scoreText = completion != null && completion.IsCompleted
                ? $"最高分: {completion.BestScore}"
                : "尚未完成";
            g.DrawString(scoreText, fontScore, Brushes.Cyan, 6, y);
        };

        cardButton.Click += (s, e) =>
        {
            SelectedLevelId = level.Id;
            DialogResult = DialogResult.OK;
            Close();
        };

        return cardButton;
    }
}
