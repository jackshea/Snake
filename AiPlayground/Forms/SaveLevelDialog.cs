using System;
using System.Drawing;
using System.Windows.Forms;

namespace AiPlayground.Forms;

/// <summary>
/// 保存关卡对话框
/// </summary>
public partial class SaveLevelDialog : Form
{
    private TextBox _nameTextBox = null!;
    private TextBox _descriptionTextBox = null!;
    private Button _okButton = null!;
    private Button _cancelButton = null!;

    public string LevelName => _nameTextBox.Text.Trim();
    public string LevelDescription => _descriptionTextBox.Text.Trim();

    public SaveLevelDialog(string defaultName = "", string defaultDescription = "")
    {
        InitializeDialog(defaultName, defaultDescription);
    }

    private void InitializeDialog(string defaultName, string defaultDescription)
    {
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(400, 200);
        Text = "保存关卡";

        var nameLabel = new Label
        {
            Text = "关卡名称:",
            Location = new Point(20, 20),
            Size = new Size(100, 23),
            ForeColor = Color.White
        };

        _nameTextBox = new TextBox
        {
            Location = new Point(120, 18),
            Size = new Size(260, 23),
            Text = defaultName
        };

        var descLabel = new Label
        {
            Text = "描述:",
            Location = new Point(20, 60),
            Size = new Size(100, 23),
            ForeColor = Color.White
        };

        _descriptionTextBox = new TextBox
        {
            Location = new Point(120, 58),
            Size = new Size(260, 60),
            Multiline = true,
            Text = defaultDescription
        };

        _okButton = new Button
        {
            Text = "保存",
            Location = new Point(220, 130),
            Size = new Size(80, 30),
            DialogResult = DialogResult.OK
        };
        _okButton.Click += (s, e) => ValidateAndClose();

        _cancelButton = new Button
        {
            Text = "取消",
            Location = new Point(310, 130),
            Size = new Size(80, 30),
            DialogResult = DialogResult.Cancel
        };

        Controls.Add(nameLabel);
        Controls.Add(_nameTextBox);
        Controls.Add(descLabel);
        Controls.Add(_descriptionTextBox);
        Controls.Add(_okButton);
        Controls.Add(_cancelButton);

        _nameTextBox.Focus();
        _nameTextBox.SelectAll();

        AcceptButton = _okButton;
        CancelButton = _cancelButton;
    }

    private void ValidateAndClose()
    {
        if (string.IsNullOrWhiteSpace(LevelName))
        {
            MessageBox.Show("请输入关卡名称", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _nameTextBox.Focus();
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
