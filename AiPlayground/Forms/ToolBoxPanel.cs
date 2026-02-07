using System;
using System.Drawing;
using System.Windows.Forms;
using AiPlayground.Controls;
using AiPlayground.Models;
using AiPlayground.Models.Obstacles;

namespace AiPlayground.Forms;

/// <summary>
/// 工具箱面板 - 选择障碍物类型
/// </summary>
public class ToolBoxPanel : DoubleBufferPanel
{
    private Level _level;
    private Button _selectedButton = null!;

    public event Action<ObstacleType>? ToolSelected;

    public ToolBoxPanel(Level level)
    {
        _level = level;
        BackColor = Color.FromArgb(35, 35, 35);
        BorderStyle = BorderStyle.FixedSingle;
        InitializeControls();
    }

    public void SetLevel(Level level)
    {
        _level = level;
    }

    private void InitializeControls()
    {
        var titleLabel = new Label
        {
            Text = "工具箱",
            Font = new Font("Microsoft YaHei UI", 12, FontStyle.Bold),
            ForeColor = Color.Gold,
            Location = new Point(10, 10),
            AutoSize = true
        };

        var startY = 50;
        int buttonHeight = 35;
        int spacing = 5;

        // 静态障碍物
        var staticButton = CreateToolButton("🟥 静态障碍物", ObstacleType.Static);
        staticButton.Location = new Point(10, startY);
        staticButton.Click += (s, e) => SelectTool(staticButton, ObstacleType.Static);
        _selectedButton = staticButton;

        // 可破坏障碍物
        var destructibleButton = CreateToolButton("🟧 可破坏障碍物", ObstacleType.Destructible);
        destructibleButton.Location = new Point(10, startY + (buttonHeight + spacing) * 1);
        destructibleButton.Click += (s, e) => SelectTool(destructibleButton, ObstacleType.Destructible);

        // 动态障碍物
        var dynamicButton = CreateToolButton("🟪 动态障碍物", ObstacleType.Dynamic);
        dynamicButton.Location = new Point(10, startY + (buttonHeight + spacing) * 2);
        dynamicButton.Click += (s, e) => SelectTool(dynamicButton, ObstacleType.Dynamic);

        // 加速道具
        var speedUpButton = CreateToolButton("⚡ 加速道具", ObstacleType.SpeedUp);
        speedUpButton.Location = new Point(10, startY + (buttonHeight + spacing) * 3);
        speedUpButton.Click += (s, e) => SelectTool(speedUpButton, ObstacleType.SpeedUp);

        // 减速道具
        var speedDownButton = CreateToolButton("🐢 减速道具", ObstacleType.SpeedDown);
        speedDownButton.Location = new Point(10, startY + (buttonHeight + spacing) * 4);
        speedDownButton.Click += (s, e) => SelectTool(speedDownButton, ObstacleType.SpeedDown);

        // 分数倍增
        var scoreMultButton = CreateToolButton("✨ 分数倍增", ObstacleType.ScoreMultiplier);
        scoreMultButton.Location = new Point(10, startY + (buttonHeight + spacing) * 5);
        scoreMultButton.Click += (s, e) => SelectTool(scoreMultButton, ObstacleType.ScoreMultiplier);

        Controls.Add(titleLabel);
        Controls.Add(staticButton);
        Controls.Add(destructibleButton);
        Controls.Add(dynamicButton);
        Controls.Add(speedUpButton);
        Controls.Add(speedDownButton);
        Controls.Add(scoreMultButton);
    }

    private Button CreateToolButton(string text, ObstacleType toolType)
    {
        return new Button
        {
            Text = text,
            Size = new Size(220, 35),
            Font = new Font("Microsoft YaHei UI", 10),
            BackColor = Color.FromArgb(60, 60, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Tag = toolType
        };
    }

    private void SelectTool(Button button, ObstacleType toolType)
    {
        _selectedButton.BackColor = Color.FromArgb(60, 60, 60);
        _selectedButton = button;
        button.BackColor = Color.FromArgb(80, 120, 80);
        ToolSelected?.Invoke(toolType);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // 绘制提示文本
        using var font = new Font("Microsoft YaHei UI", 8);
        using var brush = new SolidBrush(Color.LightGray);
        e.Graphics.DrawString(
            "左键点击放置\n右键点击删除",
            font,
            brush,
            new RectangleF(10, Height - 50, Width - 20, 50)
        );
    }
}
