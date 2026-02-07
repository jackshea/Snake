using System;
using System.Drawing;
using System.Windows.Forms;
using AiPlayground.Controls;
using AiPlayground.Models;
using AiPlayground.Models.Obstacles;

namespace AiPlayground.Forms;

/// <summary>
/// 属性面板 - 显示和编辑选中对象的属性
/// </summary>
public class PropertiesPanel : DoubleBufferPanel
{
    private Level _level;
    private object? _selectedObject;
    private TableLayoutPanel _layoutPanel = null!;

    public event Action? PropertyChanged;

    public PropertiesPanel(Level level)
    {
        _level = level;
        BackColor = Color.FromArgb(35, 35, 35);
        BorderStyle = BorderStyle.FixedSingle;
        InitializeControls();
    }

    public void SetLevel(Level level)
    {
        _level = level;
        RefreshPanel();
    }

    public void SetSelectedObject(object? obj)
    {
        _selectedObject = obj;
        RefreshPanel();
    }

    private void InitializeControls()
    {
        var titleLabel = new Label
        {
            Text = "属性",
            Font = new Font("Microsoft YaHei UI", 12, FontStyle.Bold),
            ForeColor = Color.Gold,
            Dock = DockStyle.Top,
            Padding = new Padding(10, 5, 10, 5)
        };

        _layoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(10)
        };
        _layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        _layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        Controls.Add(_layoutPanel);
        Controls.Add(titleLabel);
    }

    private void RefreshPanel()
    {
        _layoutPanel.Controls.Clear();
        _layoutPanel.RowCount = 0;

        if (_selectedObject == null)
        {
            ShowLevelProperties();
        }
        else if (_selectedObject is Obstacle obstacle)
        {
            ShowObstacleProperties(obstacle);
        }
    }

    private void ShowLevelProperties()
    {
        AddPropertyRow("关卡名称", CreateTextBox(_level.Name, (v) => _level.Name = v));
        AddPropertyRow("描述", CreateTextBox(_level.Description, (v) => _level.Description = v));
        AddPropertyRow("宽度", CreateNumericUpDown(_level.GridWidth, 10, 50, (v) => _level.Settings.GridWidth = (int)v));
        AddPropertyRow("高度", CreateNumericUpDown(_level.GridHeight, 10, 50, (v) => _level.Settings.GridHeight = (int)v));
    }

    private void ShowObstacleProperties(Obstacle obstacle)
    {
        AddPropertyRow("类型", CreateReadOnlyLabel(obstacle.Type.ToString()));
        AddPropertyRow("X 坐标", CreateNumericUpDown(obstacle.Position.X, 0, 50, (v) => obstacle.Position = new Point((int)v, obstacle.Position.Y)));
        AddPropertyRow("Y 坐标", CreateNumericUpDown(obstacle.Position.Y, 0, 50, (v) => obstacle.Position = new Point(obstacle.Position.X, (int)v)));

        if (obstacle is DestructibleObstacle destructible)
        {
            AddPropertyRow("最大通过次数", CreateNumericUpDown(destructible.MaxPasses, 1, 10, (v) => destructible.MaxPasses = (int)v));
        }

        if (obstacle is DynamicObstacle dynamic)
        {
            AddPropertyRow("移动间隔(ms)", CreateNumericUpDown(dynamic.MoveIntervalMs, 100, 2000, (v) => dynamic.MoveIntervalMs = (int)v));
            AddPropertyRow("循环路径", CreateCheckBox(dynamic.LoopPath, (v) => dynamic.LoopPath = v));
        }

        if (obstacle is SpecialEffectObstacle special)
        {
            if (special.Type == ObstacleType.ScoreMultiplier)
            {
                AddPropertyRow("倍数", CreateNumericUpDown(special.ScoreMultiplierValue, 1, 10, (v) => special.ScoreMultiplierValue = (int)v));
            }
            if (special.Type == ObstacleType.SpeedUp || special.Type == ObstacleType.SpeedDown)
            {
                AddPropertyRow("速度变化", CreateNumericUpDown(special.SpeedChangeAmount, -5, 5, (v) => special.SpeedChangeAmount = (int)v));
            }
        }

        // 删除按钮
        var deleteButton = new Button
        {
            Text = "删除障碍物",
            BackColor = Color.FromArgb(200, 50, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 10, 0, 0)
        };
        deleteButton.Click += (s, e) =>
        {
            _level.Obstacles.Remove(obstacle);
            _selectedObject = null;
            RefreshPanel();
            PropertyChanged?.Invoke();
        };

        _layoutPanel.RowCount++;
        _layoutPanel.Controls.Add(new Label { Text = "" }, 0, _layoutPanel.RowCount - 1);
        _layoutPanel.Controls.Add(deleteButton, 1, _layoutPanel.RowCount - 1);
    }

    private void AddPropertyRow(string label, Control control)
    {
        _layoutPanel.RowCount++;
        var labelControl = new Label
        {
            Text = label,
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei UI", 9),
            Anchor = AnchorStyles.Left
        };
        _layoutPanel.Controls.Add(labelControl, 0, _layoutPanel.RowCount - 1);
        _layoutPanel.Controls.Add(control, 1, _layoutPanel.RowCount - 1);
    }

    private TextBox CreateTextBox(string value, Action<string> onChanged)
    {
        var textBox = new TextBox
        {
            Text = value,
            BackColor = Color.FromArgb(60, 60, 60),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        textBox.TextChanged += (s, e) =>
        {
            onChanged(textBox.Text);
            PropertyChanged?.Invoke();
        };
        return textBox;
    }

    private NumericUpDown CreateNumericUpDown(int value, int min, int max, Action<decimal> onChanged)
    {
        var numeric = new NumericUpDown
        {
            Value = value,
            Minimum = min,
            Maximum = max,
            BackColor = Color.FromArgb(60, 60, 60),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        numeric.ValueChanged += (s, e) =>
        {
            onChanged(numeric.Value);
            PropertyChanged?.Invoke();
        };
        return numeric;
    }

    private CheckBox CreateCheckBox(bool checkedState, Action<bool> onChanged)
    {
        var checkBox = new CheckBox
        {
            Checked = checkedState,
            BackColor = Color.Transparent,
            ForeColor = Color.White
        };
        checkBox.CheckedChanged += (s, e) =>
        {
            onChanged(checkBox.Checked);
            PropertyChanged?.Invoke();
        };
        return checkBox;
    }

    private Label CreateReadOnlyLabel(string text)
    {
        return new Label
        {
            Text = text,
            ForeColor = Color.LightGray,
            Font = new Font("Microsoft YaHei UI", 9),
            Dock = DockStyle.Fill
        };
    }
}
