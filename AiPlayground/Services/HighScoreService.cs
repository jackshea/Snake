using System.ComponentModel;
using AiPlayground.Services.Abstractions;

namespace AiPlayground.Services;

/// <summary>
/// 高分服务 - 负责最高分的保存和加载
/// </summary>
public class HighScoreService
{
    private readonly string _highScoreFilePath;
    private readonly IFileSystem _fileSystem;

    public HighScoreService(IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new PhysicalFileSystem();
        _highScoreFilePath = _fileSystem.CombinePaths(
            _fileSystem.GetSpecialFolder(Environment.SpecialFolder.ApplicationData),
            "AiPlayground",
            "snake_highscore.txt"
        );
    }

    /// <summary>
    /// 加载最高分
    /// </summary>
    public int LoadHighScore()
    {
        try
        {
            if (_fileSystem.FileExists(_highScoreFilePath))
            {
                string scoreText = _fileSystem.ReadAllText(_highScoreFilePath);
                if (int.TryParse(scoreText, out int score))
                {
                    return score;
                }
            }
        }
        catch
        {
            // 忽略错误
        }
        return 0;
    }

    /// <summary>
    /// 保存最高分
    /// </summary>
    public void SaveHighScore(int score)
    {
        try
        {
            string? directory = _fileSystem.GetDirectoryName(_highScoreFilePath);
            if (!string.IsNullOrEmpty(directory) && !_fileSystem.DirectoryExists(directory))
            {
                _fileSystem.CreateDirectory(directory);
            }
            _fileSystem.WriteAllText(_highScoreFilePath, score.ToString());
        }
        catch
        {
            // 忽略保存错误
        }
    }

    /// <summary>
    /// 检查并更新最高分
    /// </summary>
    /// <returns>如果是新纪录返回 true</returns>
    public bool TryUpdateHighScore(int currentScore, ref int highScore)
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore(highScore);
            return true;
        }
        return false;
    }
}
