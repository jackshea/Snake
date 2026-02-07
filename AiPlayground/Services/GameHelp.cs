using System.Windows.Forms;

namespace AiPlayground.Services;

/// <summary>
/// 游戏帮助服务
/// </summary>
public static class GameHelp
{
    /// <summary>
    /// 显示游戏帮助
    /// </summary>
    public static void ShowHelp()
    {
        string helpText = @"贪吃蛇游戏 - 帮助

=== 游戏目标 ===
控制蛇吃掉食物，使蛇变长并获得分数。

=== 操作方法 ===
方向键 ↑↓←→  - 控制蛇的移动方向
空格键         - 暂停/继续游戏
开始按钮       - 开始新游戏
F2            - 重置游戏
F1            - 显示此帮助

=== 游戏规则 ===
• 吃到食物可以得分并使蛇变长
• 撞到墙壁或自己的身体游戏结束
• 在简单模式下有3个食物，中等和困难模式只有1个食物
• 困难模式下速度更快，但得分也更多

=== 难度级别 ===
• 简单：3个食物，每个食物 5 + 速度等级 分
• 中等：1个食物，每个食物 10 + 速度等级 分
• 困难：1个食物，速度快，每个食物 20 + 速度等级 分

=== 速度等级 ===
按数字键 0-9 可以快速调整速度等级
等级 1 = 最慢，等级 10 = 最快

=== 提示 ===
• 游戏会自动保存您的最高分
• 菜单中有更多选项可以调整游戏设置
• 暂停时可以调整速度和难度
• 点击开始按钮或按空格键开始游戏

祝您游戏愉快！";

        MessageBox.Show(helpText, "游戏帮助", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// 显示关于对话框
    /// </summary>
    public static void ShowAbout()
    {
        MessageBox.Show(
            "贪吃蛇 v1.0\n\n" +
            "使用 C# 和 WinForms 开发\n\n" +
            "功能特点：\n" +
            "• 3 种难度级别\n" +
            "• 10 档速度调节\n" +
            "• 最高分自动保存\n" +
            "• 暂停/继续功能\n" +
            "• 完整的菜单系统\n" +
            "• 无闪烁双缓冲渲染\n\n" +
            "© 2025 AiPlayground",
            "关于贪吃蛇",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    /// <summary>
    /// 显示分数记录
    /// </summary>
    public static void ShowHighScores(int currentScore, int highScore)
    {
        MessageBox.Show(
            $"当前分数：{currentScore}\n\n" +
            $"历史最高：{highScore}\n\n" +
            $"提示：打破最高分会自动保存！",
            "分数记录",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    /// <summary>
    /// 显示难度设置说明
    /// </summary>
    public static void ShowDifficultyInfo(string difficultyName)
    {
        MessageBox.Show(
            $"难度已设置为：{difficultyName}\n\n" +
            "• 简单：3个食物，速度较慢\n" +
            "• 中等：1个食物，标准速度\n" +
            "• 困难：1个食物，速度较快",
            "难度设置",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}
