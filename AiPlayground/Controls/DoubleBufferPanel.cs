using System;
using System.Windows.Forms;
using System.Drawing;

namespace AiPlayground.Controls;

/// <summary>
/// 自定义双缓冲面板，防止闪烁
/// </summary>
public class DoubleBufferPanel : Panel
{
    public DoubleBufferPanel()
    {
        DoubleBuffered = true;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);
        UpdateStyles();
    }
}
