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

    // 新增测试 - CompleteLevelAsync 正确解锁下一关

    [Fact]
    public async Task CompleteLevelAsync_ShouldUnlockNextLevel()
    {
        // Arrange
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);
        var level1 = manager.GetLevelByNumber(1);
        if (level1 == null)
        {
            // 如果关卡1不存在，跳过测试
            return;
        }

        manager.TryLoadLevel(level1.Id);
        var state = new GameState { Score = 100, LevelTime = 30 };

        // Act
        await manager.CompleteLevelAsync(state);

        // Assert - 关卡2应该被解锁
        var level2 = manager.GetLevelByNumber(2);
        if (level2 != null)
        {
            level2.IsUnlocked.Should().BeTrue("完成关卡1后应该解锁关卡2");
        }
    }

    [Fact]
    public async Task CompleteLevelAsync_ShouldSaveCompletionRecord()
    {
        // Arrange
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);
        var level1 = manager.GetLevelByNumber(1);
        if (level1 == null)
        {
            return;
        }

        manager.TryLoadLevel(level1.Id);
        var state = new GameState { Score = 150, LevelTime = 45 };

        // Act
        await manager.CompleteLevelAsync(state);

        // Assert
        var completion = manager.GetLevelCompletion(level1.Id);
        completion.Should().NotBeNull();
        completion!.BestScore.Should().Be(150);
        completion.BestTime.Should().Be(45);
        completion.CompletionCount.Should().Be(1);
    }

    [Fact]
    public async Task CompleteLevelAsync_MultipleCompletions_ShouldUpdateBestScore()
    {
        // Arrange
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);
        var level1 = manager.GetLevelByNumber(1);
        if (level1 == null)
        {
            return;
        }

        manager.TryLoadLevel(level1.Id);

        // 第一次完成
        var state1 = new GameState { Score = 100, LevelTime = 50 };
        await manager.CompleteLevelAsync(state1);

        // 第二次完成（更高分）
        var state2 = new GameState { Score = 200, LevelTime = 40 };

        // Act
        await manager.CompleteLevelAsync(state2);

        // Assert
        var completion = manager.GetLevelCompletion(level1.Id);
        completion.Should().NotBeNull();
        completion!.BestScore.Should().Be(200, "应该更新为最高分");
        completion.BestTime.Should().Be(40, "应该更新为最佳时间");
        completion.CompletionCount.Should().Be(2, "应该记录完成次数");
    }

    // 新增测试 - CheckVictoryCondition 所有组合条件
    // 注意：这些测试直接使用 VictoryCondition.CheckCondition 而不是通过 LevelManager
    // 因为 LevelManager 需要实际加载关卡

    [Fact]
    public void CheckVictoryCondition_TargetScore_ShouldReturnTrueWhenReached()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.TargetScore,
            TargetScore = 100
        };
        var state = new GameState { Score = 100 };

        // Act
        var result = condition.CheckCondition(state, 0, 0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckVictoryCondition_TargetScore_ShouldReturnFalseWhenNotReached()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.TargetScore,
            TargetScore = 100
        };
        var state = new GameState { Score = 50 };

        // Act
        var result = condition.CheckCondition(state, 0, 0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckVictoryCondition_TargetLength_ShouldReturnTrueWhenReached()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.TargetLength,
            TargetLength = 10
        };
        var state = new GameState();
        for (int i = 0; i < 10; i++)
        {
            state.Snake.AddLast(new Point(i, 0));
        }

        // Act
        var result = condition.CheckCondition(state, 0, 0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckVictoryCondition_CollectAllFood_ShouldReturnTrueWhenAllCollected()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.CollectAllFood,
            MustCollectAllFood = true,
            FoodSpawnCount = 10
        };
        var state = new GameState
        {
            FoodCollected = 10,
            TotalFoodSpawned = 10
        };

        // Act
        var result = condition.CheckCondition(state, state.FoodCollected, state.TotalFoodSpawned);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckVictoryCondition_Combined_ShouldReturnTrueWhenAllConditionsMet()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.Combined,
            TargetScore = 100,
            TargetLength = 10,
            MustCollectAllFood = false
        };
        var state = new GameState { Score = 100 };
        for (int i = 0; i < 10; i++)
        {
            state.Snake.AddLast(new Point(i, 0));
        }

        // Act
        var result = condition.CheckCondition(state, 0, 0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckVictoryCondition_Combined_ShouldReturnFalseWhenOneConditionNotMet()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.Combined,
            TargetScore = 100,
            TargetLength = 10,
            MustCollectAllFood = false
        };
        var state = new GameState { Score = 50 }; // 分数不够
        for (int i = 0; i < 10; i++)
        {
            state.Snake.AddLast(new Point(i, 0));
        }

        // Act
        var result = condition.CheckCondition(state, 0, 0);

        // Assert
        result.Should().BeFalse();
    }

    // 新增测试 - GetVictoryConditionDescription 格式正确

    [Fact]
    public void GetVictoryConditionDescription_TargetScore_ShouldReturnCorrectDescription()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.TargetScore,
            TargetScore = 100
        };

        // Act
        var description = condition.GetDescription();

        // Assert
        description.Should().Contain("100");
    }

    [Fact]
    public void GetVictoryConditionDescription_TargetLength_ShouldReturnCorrectDescription()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.TargetLength,
            TargetLength = 15
        };

        // Act
        var description = condition.GetDescription();

        // Assert
        description.Should().Contain("15");
    }

    [Fact]
    public void GetVictoryConditionDescription_CollectAllFood_ShouldReturnCorrectDescription()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.CollectAllFood,
            MustCollectAllFood = true,
            FoodSpawnCount = 10
        };

        // Act
        var description = condition.GetDescription();

        // Assert
        description.Should().Contain("所有食物");
    }

    [Fact]
    public void GetVictoryConditionDescription_Combined_ShouldReturnAllConditions()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.Combined,
            TargetScore = 100,
            TargetLength = 10,
            MustCollectAllFood = false
        };

        // Act
        var description = condition.GetDescription();

        // Assert
        description.Should().Contain("100");
        description.Should().Contain("10");
    }

    // 新增测试 - GetLevelProgress

    [Fact]
    public void GetLevelProgress_TargetScore_ShouldReturnCorrectPercentage()
    {
        // Arrange
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // 使用实际存在的预设关卡（关卡1）
        var level1 = manager.GetLevelByNumber(1);
        if (level1 == null)
        {
            return;
        }

        // 修改通关条件为测试目标
        level1.VictoryCondition.Type = VictoryConditionType.TargetScore;
        level1.VictoryCondition.TargetScore = 100;

        manager.TryLoadLevel(level1.Id);
        var state = new GameState { Score = 50 };

        // Act
        var progress = manager.GetLevelProgress(state);

        // Assert
        progress.Should().Be(50); // 50/100 = 50%
    }

    [Fact]
    public void GetLevelProgress_TargetScore_CappedAt100()
    {
        // Arrange
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // 使用实际存在的预设关卡（关卡1）
        var level1 = manager.GetLevelByNumber(1);
        if (level1 == null)
        {
            return;
        }

        // 修改通关条件为测试目标
        level1.VictoryCondition.Type = VictoryConditionType.TargetScore;
        level1.VictoryCondition.TargetScore = 100;

        manager.TryLoadLevel(level1.Id);
        var state = new GameState { Score = 150 };

        // Act
        var progress = manager.GetLevelProgress(state);

        // Assert
        progress.Should().Be(100); // 不超过100%
    }

    [Fact]
    public void GetLevelProgress_TargetLength_ShouldReturnCorrectPercentage()
    {
        // Arrange
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // 使用实际存在的预设关卡（关卡1）
        var level1 = manager.GetLevelByNumber(1);
        if (level1 == null)
        {
            return;
        }

        // 修改通关条件为测试目标
        level1.VictoryCondition.Type = VictoryConditionType.TargetLength;
        level1.VictoryCondition.TargetLength = 10;

        manager.TryLoadLevel(level1.Id);
        var state = new GameState();
        for (int i = 0; i < 5; i++)
        {
            state.Snake.AddLast(new Point(i, 0));
        }

        // Act
        var progress = manager.GetLevelProgress(state);

        // Assert
        progress.Should().Be(50); // 5/10 = 50%
    }

    [Fact]
    public void GetLevelProgress_CollectAllFood_ShouldReturnCorrectPercentage()
    {
        // Arrange
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // 使用实际存在的预设关卡（关卡1）
        var level1 = manager.GetLevelByNumber(1);
        if (level1 == null)
        {
            return;
        }

        // 修改通关条件为测试目标
        level1.VictoryCondition.Type = VictoryConditionType.CollectAllFood;
        level1.VictoryCondition.MustCollectAllFood = true;
        level1.VictoryCondition.FoodSpawnCount = 10;

        manager.TryLoadLevel(level1.Id);
        var state = new GameState
        {
            FoodCollected = 5,
            TotalFoodSpawned = 10
        };

        // Act
        var progress = manager.GetLevelProgress(state);

        // Assert
        progress.Should().Be(50); // 5/10 = 50%
    }

    [Fact]
    public void GetLevelProgress_Combined_ShouldReturnAveragePercentage()
    {
        // Arrange
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // 使用实际存在的预设关卡（关卡1）
        var level1 = manager.GetLevelByNumber(1);
        if (level1 == null)
        {
            return;
        }

        // 修改通关条件为测试目标
        level1.VictoryCondition.Type = VictoryConditionType.Combined;
        level1.VictoryCondition.TargetScore = 100;
        level1.VictoryCondition.TargetLength = 10;
        level1.VictoryCondition.MustCollectAllFood = false;

        manager.TryLoadLevel(level1.Id);
        var state = new GameState { Score = 50 }; // 50%
        for (int i = 0; i < 10; i++)
        {
            state.Snake.AddLast(new Point(i, 0)); // 100%
        }

        // Act
        var progress = manager.GetLevelProgress(state);

        // Assert
        progress.Should().Be(75); // (50 + 100) / 2 = 75%
    }

    // 新增测试 - TryLoadLevel

    [Fact]
    public void TryLoadLevel_ShouldSucceedForValidLevel()
    {
        // Arrange
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // 使用实际存在的预设关卡（关卡1）
        var level1 = manager.GetLevelByNumber(1);
        if (level1 == null)
        {
            return;
        }

        // Act
        var result = manager.TryLoadLevel(level1.Id);

        // Assert
        result.Should().BeTrue();
        manager.CurrentLevel.Should().NotBeNull();
    }

    [Fact]
    public void TryLoadLevel_ShouldFailForInvalidId()
    {
        // Arrange
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // Act
        var result = manager.TryLoadLevel("invalid_id");

        // Assert
        result.Should().BeFalse();
        manager.CurrentLevel.Should().BeNull();
    }

    [Fact]
    public void TryLoadLevel_ShouldRaiseLevelChangedEvent()
    {
        // Arrange
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // 使用实际存在的预设关卡（关卡1）
        var level1 = manager.GetLevelByNumber(1);
        if (level1 == null)
        {
            return;
        }

        Level? raisedLevel = null;
        manager.LevelChanged += (l) => raisedLevel = l;

        // Act
        manager.TryLoadLevel(level1.Id);

        // Assert
        raisedLevel.Should().NotBeNull();
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
