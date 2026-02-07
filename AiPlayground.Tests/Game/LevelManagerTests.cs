using System.Drawing;
using AiPlayground.Game;
using AiPlayground.Models;
using AiPlayground.Services;
using AiPlayground.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Game;

public class LevelManagerTests
{
    [Fact]
    public void Constructor_ShouldInitializeManager()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var storageService = new LevelStorageService();

        // Act
        var manager = new LevelManager(storageService);

        // Assert
        manager.Should().NotBeNull();
    }

    [Fact]
    public void CustomLevels_ShouldLoadFromFileSystem()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // Act
        var customLevels = manager.CustomLevels;

        // Assert
        // Loads from actual file system, so may have levels or be empty
        customLevels.Should().NotBeNull();
    }

    [Fact]
    public void GetLevelByNumber_WithNonExistentLevel_ShouldReturnNull()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // Act
        var level = manager.GetLevelByNumber(999);

        // Assert
        level.Should().BeNull();
    }

    [Fact]
    public void CheckVictoryCondition_WithNoLevelLoaded_ShouldReturnFalse()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);
        var state = new GameState { Score = 150 };

        // Act
        var result = manager.CheckVictoryCondition(state);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetVictoryConditionDescription_WithNoLevelLoaded_ShouldReturnNone()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // Act
        var description = manager.GetVictoryConditionDescription();

        // Assert
        description.Should().Be("无");
    }

    [Fact]
    public void GetLevelProgress_WithNoLevelLoaded_ShouldReturnZero()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);
        var state = new GameState { Score = 100 };

        // Act
        var progress = manager.GetLevelProgress(state);

        // Assert
        progress.Should().Be(0);
    }

    [Fact]
    public void GetNextLockedLevelNumber_ShouldReturnNumberGreaterThanOne()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // Act
        var nextLevel = manager.GetNextLockedLevelNumber();

        // Assert
        // Level 1 is unlocked by default, so next level should be >= 2
        nextLevel.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public void CurrentLevel_Initially_ShouldBeNull()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // Act
        var currentLevel = manager.CurrentLevel;

        // Assert
        currentLevel.Should().BeNull();
    }
}

/// <summary>
/// 临时目录辅助类 - 用于测试文件系统操作
/// </summary>
public class TempDirectory : IDisposable
{
    private readonly string _path;

    public TempDirectory()
    {
        _path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
        System.IO.Directory.CreateDirectory(_path);
    }

    public string Path => _path;

    public void Dispose()
    {
        try
        {
            if (System.IO.Directory.Exists(_path))
            {
                System.IO.Directory.Delete(_path, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
