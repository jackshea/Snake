using System;
using System.Drawing;
using System.Windows.Forms;
using AiPlayground.Models;
using AiPlayground.Services;
using AiPlayground.Game;
using System.Text.Json;

namespace AiPlayground.Forms;

/// <summary>
/// 关卡编辑器主窗体
/// </summary>
public class LevelEditorForm : Form
{
    private Level _currentLevel;
    private LevelEditorPanel _editorPanel = null!;
    private ToolBoxPanel _toolBoxPanel = null!;
    private PropertiesPanel _propertiesPanel = null!;
    private MenuStrip _menuStrip = null!;
    private readonly LevelStorageService _storageService;
    private readonly LevelManager _levelManager;
    private bool _hasUnsavedChanges;

    public LevelEditorForm(LevelStorageService storageService, LevelManager levelManager)
    {
        _storageService = storageService;
        _levelManager = levelManager;
        _currentLevel = Level.CreateCustomLevel("新关卡");
        InitializeForm();
        InitializeMenu();
        InitializeLayout();
        SetupEventSubscriptions();
    }

    private void InitializeForm()
    {
        Text = "关卡编辑器";
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        BackColor = Color.FromArgb(45, 45, 45);
    }

    private void SetupEventSubscriptions()
    {
        _propertiesPanel.PropertyChanged += MarkAsDirty;
        _editorPanel.ObstacleAdded += MarkAsDirty;
        _editorPanel.ObstacleRemoved += MarkAsDirty;
    }

    private void MarkAsDirty()
    {
        if (!_hasUnsavedChanges)
        {
            _hasUnsavedChanges = true;
            UpdateWindowTitle();
        }
    }

    private void MarkAsClean()
    {
        if (_hasUnsavedChanges)
        {
            _hasUnsavedChanges = false;
            UpdateWindowTitle();
        }
    }

    private void UpdateWindowTitle()
    {
        string levelName = string.IsNullOrWhiteSpace(_currentLevel.Name) ? "新关卡" : _currentLevel.Name;
        Text = _hasUnsavedChanges
            ? $"关卡编辑器 - {levelName} ●"
            : $"关卡编辑器 - {levelName}";
    }

    private void InitializeMenu()
    {
        _menuStrip = new MenuStrip();

        // 文件菜单
        var fileMenu = new ToolStripMenuItem("文件(&F)");
        fileMenu.DropDownItems.Add("新建(&N)", null, (s, e) => NewLevel());
        fileMenu.DropDownItems.Add("打开(&O)", null, (s, e) => OpenLevel());
        fileMenu.DropDownItems.Add("保存(&S)", null, (s, e) => SaveLevelSync());
        fileMenu.DropDownItems.Add("另存为(&A)", null, (s, e) => SaveLevelAsSync());
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("测试关卡(&T)", null, (s, e) => TestLevel());
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("退出(&X)", null, (s, e) => Close());

        // 编辑菜单
        var editMenu = new ToolStripMenuItem("编辑(&E)");
        editMenu.DropDownItems.Add("清除所有障碍物(&C)", null, (s, e) => ClearObstacles());

        // 视图菜单
        var viewMenu = new ToolStripMenuItem("视图(&V)");
        viewMenu.DropDownItems.Add("放大(&Z+)", null, (s, e) => ZoomIn());
        viewMenu.DropDownItems.Add("缩小(&Z-)", null, (s, e) => ZoomOut());
        viewMenu.DropDownItems.Add("重置视图(&R)", null, (s, e) => ResetZoom());

        // 帮助菜单
        var helpMenu = new ToolStripMenuItem("帮助(&H)");
        helpMenu.DropDownItems.Add("编辑器帮助(&H)", null, (s, e) => ShowHelp());
        helpMenu.DropDownItems.Add("关于(&A)", null, (s, e) => ShowAbout());

        _menuStrip.Items.Add(fileMenu);
        _menuStrip.Items.Add(editMenu);
        _menuStrip.Items.Add(viewMenu);
        _menuStrip.Items.Add(helpMenu);

        Controls.Add(_menuStrip);
    }

    private void InitializeLayout()
    {
        var mainSplitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 250,
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(45, 45, 45)
        };

        // 左侧面板
        var leftPanel = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 200,
            BorderStyle = BorderStyle.None
        };

        // 工具箱
        _toolBoxPanel = new ToolBoxPanel(_currentLevel)
        {
            Dock = DockStyle.Fill
        };
        _toolBoxPanel.ToolSelected += (toolType) => _editorPanel.SelectedTool = toolType;

        // 属性面板
        _propertiesPanel = new PropertiesPanel(_currentLevel)
        {
            Dock = DockStyle.Fill
        };
        _propertiesPanel.PropertyChanged += () => _editorPanel.Invalidate();

        leftPanel.Panel1.Controls.Add(_toolBoxPanel);
        leftPanel.Panel2.Controls.Add(_propertiesPanel);

        // 右侧编辑器
        _editorPanel = new LevelEditorPanel(_currentLevel)
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Black
        };
        _editorPanel.SelectionChanged += (obj) => _propertiesPanel.SetSelectedObject(obj);

        mainSplitContainer.Panel1.Controls.Add(leftPanel);
        mainSplitContainer.Panel2.Controls.Add(_editorPanel);

        Controls.Add(mainSplitContainer);
    }

    private void NewLevel()
    {
        if (!ConfirmSaveChanges()) return;

        _currentLevel = Level.CreateCustomLevel("新关卡");
        _toolBoxPanel.SetLevel(_currentLevel);
        _propertiesPanel.SetLevel(_currentLevel);
        _editorPanel.SetLevel(_currentLevel);
        MarkAsClean();
    }

    private void OpenLevel()
    {
        if (!ConfirmSaveChanges()) return;

        // 重新加载自定义关卡列表
        _levelManager.ReloadLevels();
        var customLevels = _levelManager.CustomLevels.ToList();

        using var dialog = new EditorLevelSelectionForm(customLevels);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            if (dialog.SelectedLevel != null)
            {
                LoadLevelIntoEditor(dialog.SelectedLevel);
            }

            // 处理删除
            if (!string.IsNullOrEmpty(dialog.LevelIdToDelete))
            {
                _levelManager.DeleteCustomLevel(dialog.LevelIdToDelete);
            }
        }
        else if (!string.IsNullOrEmpty(dialog.LevelIdToDelete))
        {
            // 用户取消了加载，但可能删除了关卡
            _levelManager.DeleteCustomLevel(dialog.LevelIdToDelete);
        }
    }

    private void LoadLevelIntoEditor(Level level)
    {
        _currentLevel = CloneLevel(level);
        _propertiesPanel.SetLevel(_currentLevel);
        _editorPanel.SetLevel(_currentLevel);
        MarkAsClean();
    }

    private void TestLevel()
    {
        // 验证关卡
        var (isValid, errorMessage) = ValidateLevelForTest();

        if (!isValid)
        {
            MessageBox.Show(
                $"关卡无法测试：\n\n{errorMessage}",
                "验证失败",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        // 提示保存
        if (_hasUnsavedChanges)
        {
            var result = MessageBox.Show(
                "当前关卡有未保存的更改，是否保存后再测试？\n\n" +
                "点击「是」保存并测试\n" +
                "点击「否」直接测试当前版本\n" +
                "点击「取消」返回编辑器",
                "未保存的更改",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Cancel) return;

            if (result == DialogResult.Yes)
            {
                if (string.IsNullOrWhiteSpace(_currentLevel.Name))
                {
                    using var dialog = new SaveLevelDialog(_currentLevel.Name, _currentLevel.Description);
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        _currentLevel.Name = dialog.LevelName;
                        _currentLevel.Description = dialog.LevelDescription;
                    }
                    else
                    {
                        return; // 用户取消
                    }
                }

                // 同步保存
                var saved = SaveLevelInternalSync();
                if (!saved) return;
            }
        }

        // 启动测试
        try
        {
            var testForm = new TestGameForm(_currentLevel, _levelManager);
            testForm.Show(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"启动测试失败：\n\n{ex.Message}",
                "错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void SaveLevelSync()
    {
        if (string.IsNullOrWhiteSpace(_currentLevel.Name))
        {
            // 需要输入名称
            SaveLevelAsSync();
            return;
        }

        SaveLevelInternalSync();
    }

    private void SaveLevelAsSync()
    {
        using var dialog = new SaveLevelDialog(_currentLevel.Name, _currentLevel.Description);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _currentLevel.Name = dialog.LevelName;
            _currentLevel.Description = dialog.LevelDescription;

            // 如果是另存为，生成新的 ID
            if (!string.IsNullOrEmpty(_currentLevel.Id))
            {
                _currentLevel.Id = Guid.NewGuid().ToString();
            }

            SaveLevelInternalSync();
        }
    }

    private bool SaveLevelInternalSync()
    {
        if (string.IsNullOrWhiteSpace(_currentLevel.Name))
        {
            MessageBox.Show("请输入关卡名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        // 使用同步方式等待异步方法
        var task = _levelManager.SaveCustomLevelAsync(_currentLevel);
        task.Wait(); // 简单的同步等待

        bool success = task.Result;
        if (success)
        {
            MarkAsClean();
            MessageBox.Show($"关卡 \"{_currentLevel.Name}\" 已保存！", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
        else
        {
            MessageBox.Show("保存关卡失败，请重试", "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private (bool IsValid, string? ErrorMessage) ValidateLevelForTest()
    {
        // 检查起点位置
        var startPos = _currentLevel.Settings.SnakeStartPosition;
        if (startPos.X < 0 || startPos.X >= _currentLevel.GridWidth ||
            startPos.Y < 0 || startPos.Y >= _currentLevel.GridHeight)
        {
            return (false, "起点位置超出网格范围");
        }

        // 检查起点是否有障碍物
        if (_currentLevel.Obstacles.Any(o => o.Position == startPos))
        {
            return (false, "起点位置有障碍物，蛇无法开始游戏");
        }

        // 检查通关条件
        var condition = _currentLevel.VictoryCondition;
        if (condition.Type == VictoryConditionType.TargetScore && condition.TargetScore <= 0)
        {
            return (false, "目标分数必须大于 0");
        }

        if (condition.Type == VictoryConditionType.TargetLength && condition.TargetLength <= 0)
        {
            return (false, "目标长度必须大于 0");
        }

        if (condition.Type == VictoryConditionType.Combined &&
            condition.TargetScore <= 0 &&
            condition.TargetLength <= 0 &&
            !condition.MustCollectAllFood)
        {
            return (false, "组合通关条件至少需要设置一个目标");
        }

        // 检查网格尺寸
        if (_currentLevel.GridWidth < 10 || _currentLevel.GridWidth > 50 ||
            _currentLevel.GridHeight < 10 || _currentLevel.GridHeight > 50)
        {
            return (false, "网格尺寸应在 10x10 到 50x50 之间");
        }

        return (true, null);
    }

    private void ClearObstacles()
    {
        if (MessageBox.Show("确定要清除所有障碍物吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            _currentLevel.Obstacles.Clear();
            _editorPanel.Invalidate();
            MarkAsDirty();
        }
    }

    private void ZoomIn()
    {
        _editorPanel.Zoom *= 1.2f;
        _editorPanel.Invalidate();
    }

    private void ZoomOut()
    {
        _editorPanel.Zoom /= 1.2f;
        _editorPanel.Invalidate();
    }

    private void ResetZoom()
    {
        _editorPanel.Zoom = 1.0f;
        _editorPanel.Invalidate();
    }

    private void ShowHelp()
    {
        MessageBox.Show(
            "关卡编辑器使用说明：\n\n" +
            "1. 从工具箱选择工具（障碍物、起点、食物等）\n" +
            "2. 在网格上点击放置，右键删除\n" +
            "3. 在属性面板修改关卡属性\n" +
            "4. 保存后可在游戏中测试",
            "编辑器帮助",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void ShowAbout()
    {
        MessageBox.Show(
            "贪吃蛇关卡编辑器 v1.0\n\n" +
            "用于创建自定义关卡",
            "关于",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private bool ConfirmSaveChanges()
    {
        if (!_hasUnsavedChanges) return true;

        var result = MessageBox.Show(
            "当前关卡有未保存的更改，是否保存？",
            "未保存的更改",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Cancel) return false;
        if (result == DialogResult.No) return true;

        // Yes - try to save
        return SaveLevelInternalSync();
    }

    private Level CloneLevel(Level source)
    {
        var json = JsonSerializer.Serialize(source, _storageService.GetJsonOptions());
        return JsonSerializer.Deserialize<Level>(json, _storageService.GetJsonOptions())!;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        if (e.CloseReason == CloseReason.UserClosing && !ConfirmSaveChanges())
        {
            e.Cancel = true;
        }
    }
}
