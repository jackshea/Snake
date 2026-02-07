using System;
using System.Drawing;
using System.Windows.Forms;
using AiPlayground.Models;

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

    public LevelEditorForm()
    {
        _currentLevel = Level.CreateCustomLevel("新关卡");
        InitializeForm();
        InitializeMenu();
        InitializeLayout();
    }

    private void InitializeForm()
    {
        Text = "关卡编辑器";
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        BackColor = Color.FromArgb(45, 45, 45);
    }

    private void InitializeMenu()
    {
        _menuStrip = new MenuStrip();

        // 文件菜单
        var fileMenu = new ToolStripMenuItem("文件(&F)");
        fileMenu.DropDownItems.Add("新建(&N)", null, (s, e) => NewLevel());
        fileMenu.DropDownItems.Add("打开(&O)", null, (s, e) => OpenLevel());
        fileMenu.DropDownItems.Add("保存(&S)", null, (s, e) => SaveLevel());
        fileMenu.DropDownItems.Add("另存为(&A)", null, (s, e) => SaveLevelAs());
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
    }

    private void OpenLevel()
    {
        if (!ConfirmSaveChanges()) return;

        using var dialog = new OpenFileDialog
        {
            Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            Title = "打开关卡",
            InitialDirectory = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AiPlayground", "Levels", "Custom"
            )
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            // TODO: 实现加载关卡逻辑
            MessageBox.Show("加载功能即将推出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void SaveLevel()
    {
        // TODO: 实现保存关卡逻辑
        MessageBox.Show("保存功能即将推出\n\n提示：您可以手动复制关卡数据并保存为 JSON 文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void SaveLevelAs()
    {
        SaveLevel();
    }

    private void TestLevel()
    {
        // TODO: 实现测试关卡逻辑
        MessageBox.Show("测试功能即将推出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ClearObstacles()
    {
        if (MessageBox.Show("确定要清除所有障碍物吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            _currentLevel.Obstacles.Clear();
            _editorPanel.Invalidate();
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
        // TODO: 检查是否有未保存的更改
        return true;
    }
}
