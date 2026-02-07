using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AiPlayground.Models;

namespace AiPlayground.Forms;

/// <summary>
/// 编辑器关卡选择窗体 - 用于加载和删除自定义关卡
/// </summary>
public partial class EditorLevelSelectionForm : Form
{
    private ListView _levelListView = null!;
    private Button _loadButton = null!;
    private Button _deleteButton = null!;
    private Button _cancelButton = null!;
    private Level? _selectedLevel;
    private string? _levelIdToDelete;

    public Level? SelectedLevel => _selectedLevel;
    public string? LevelIdToDelete => _levelIdToDelete;

    public EditorLevelSelectionForm(System.Collections.Generic.IList<Level> customLevels)
    {
        InitializeForm();
        InitializeControls(customLevels);
    }

    private void InitializeForm()
    {
        Text = "打开自定义关卡";
        Size = new Size(600, 400);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(45, 45, 45);
    }

    private void InitializeControls(System.Collections.Generic.IList<Level> customLevels)
    {
        var titleLabel = new Label
        {
            Text = "选择要加载的关卡",
            Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold),
            ForeColor = Color.Gold,
            Location = new Point(20, 15),
            AutoSize = true
        };

        _levelListView = new ListView
        {
            Location = new Point(20, 50),
            Size = new Size(560, 250),
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            BackColor = Color.FromArgb(60, 60, 60),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        _levelListView.Columns.Add("关卡名称", 200);
        _levelListView.Columns.Add("描述", 280);
        _levelListView.Columns.Add("障碍物数", 80);

        foreach (var level in customLevels)
        {
            var item = new ListViewItem(level.Name)
            {
                Tag = level
            };
            item.SubItems.Add(level.Description ?? "");
            item.SubItems.Add(level.Obstacles.Count.ToString());
            _levelListView.Items.Add(item);
        }

        _levelListView.SelectedIndexChanged += (s, e) =>
        {
            _loadButton.Enabled = _levelListView.SelectedItems.Count > 0;
            _deleteButton.Enabled = _levelListView.SelectedItems.Count > 0;

            if (_levelListView.SelectedItems.Count > 0)
            {
                _selectedLevel = _levelListView.SelectedItems[0].Tag as Level;
            }
            else
            {
                _selectedLevel = null;
            }
        };

        _levelListView.DoubleClick += (s, e) =>
        {
            if (_levelListView.SelectedItems.Count > 0)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        };

        _loadButton = new Button
        {
            Text = "加载",
            Location = new Point(420, 320),
            Size = new Size(80, 35),
            BackColor = Color.FromArgb(80, 120, 80),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        _loadButton.Click += (s, e) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };

        _deleteButton = new Button
        {
            Text = "删除",
            Location = new Point(330, 320),
            Size = new Size(80, 35),
            BackColor = Color.FromArgb(180, 60, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        _deleteButton.Click += (s, e) => DeleteSelectedLevel();

        _cancelButton = new Button
        {
            Text = "取消",
            Location = new Point(510, 320),
            Size = new Size(80, 35),
            DialogResult = DialogResult.Cancel
        };

        Controls.Add(titleLabel);
        Controls.Add(_levelListView);
        Controls.Add(_loadButton);
        Controls.Add(_deleteButton);
        Controls.Add(_cancelButton);

        if (_levelListView.Items.Count > 0)
        {
            _levelListView.Items[0].Selected = true;
        }
    }

    private void DeleteSelectedLevel()
    {
        if (_selectedLevel == null) return;

        var result = MessageBox.Show(
            $"确定要删除关卡 \"{_selectedLevel.Name}\" 吗？\n\n此操作无法撤销。",
            "确认删除",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            _levelIdToDelete = _selectedLevel.Id;
            var item = _levelListView.SelectedItems[0];
            _levelListView.Items.Remove(item);
            _selectedLevel = null;
            _loadButton.Enabled = false;
            _deleteButton.Enabled = false;
        }
    }
}
