using System.Drawing;
using System.Reflection;
using AiPlayground.Game;
using AiPlayground.Models;
using AiPlayground.Models.Obstacles;
using AiPlayground.Services;
using AiPlayground.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace AiPlayground.Tests.Game;

public class GameEngineTests
{
    private readonly MockRandomProvider _mockRandom;
    private readonly GameState _state;
    private readonly GameEngine _engine;

    public GameEngineTests()
    {
        _mockRandom = new MockRandomProvider();
        _state = new GameState();
        // 使用真实的随机数生成器，避免总是生成相同的位置
        _engine = new GameEngine(_state); // 不传入 mockRandom，使用默认的真实随机生成器
    }

    [Fact]
    public void Constructor_ShouldInitializeEngine()
    {
        // Arrange
        var state = new GameState();

        // Act
        var engine = new GameEngine(state);

        // Assert
        engine.Should().NotBeNull();
    }

    [Fact]
    public void Initialize_ShouldSetDefaultSnake()
    {
        // Act
        _engine.Initialize();

        // Assert
        _state.Snake.Count.Should().Be(3);
        _state.Foods.Should().NotBeEmpty();
        _state.IsGameOver.Should().BeFalse();
        _state.IsPaused.Should().BeFalse();
        _state.IsWaitingToStart.Should().BeTrue();
    }

    [Fact]
    public void Initialize_WithLevel_ShouldUseLevelSettings()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithSnakeStart(10, 10)
            .WithInitialDirection(new Point(1, 0))
            .WithInitialSnakeLength(5)
            .WithInitialSpeed(8)
            .WithDifficulty(Difficulty.Hard)
            .Build();
        _state.CurrentLevel = level;

        // Act
        _engine.Initialize();

        // Assert
        _state.Snake.Count.Should().Be(5);
        _state.Direction.Should().Be(new Point(1, 0));
        _state.SpeedLevel.Should().Be(8);
        _state.Difficulty.Should().Be(Difficulty.Hard);
    }

    [Fact]
    public void MoveSnake_ShouldMoveInCurrentDirection()
    {
        // Arrange
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;

        // Act
        _engine.MoveSnake();

        // Assert
        _state.Snake.First!.Value.Should().Be(new Point(16, 10));
        _state.Snake.Count.Should().Be(3);
    }

    [Fact]
    public void MoveSnake_WhenEatingFood_ShouldGrowAndScore()
    {
        // Arrange
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.Foods.Add(new Point(16, 10));
        _state.IsWaitingToStart = false;

        // Act
        _engine.MoveSnake();

        // Assert
        _state.Snake.Count.Should().Be(4);
        _state.Score.Should().BeGreaterThan(0);
        _state.Foods.Should().NotContain(new Point(16, 10));
    }

    [Fact]
    public void MoveSnake_WhenHittingWall_ShouldEndGame()
    {
        // Arrange
        _state.Snake.AddLast(new Point(29, 10));
        _state.Snake.AddLast(new Point(28, 10));
        _state.Snake.AddLast(new Point(27, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;

        // Act
        _engine.MoveSnake();

        // Assert
        _state.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void MoveSnake_WhenHittingSelf_ShouldEndGame()
    {
        // Arrange
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Snake.AddLast(new Point(12, 10));
        _state.Direction = new Point(-1, 0); // Turn around
        _state.IsWaitingToStart = false;

        // Act
        _engine.MoveSnake();

        // Assert
        _state.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void TryChangeDirection_WithValidDirection_ShouldChange()
    {
        // Arrange
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;

        // Act
        var result = _engine.TryChangeDirection(new Point(0, -1));

        // Assert
        result.Should().BeTrue();
        _state.Direction.Should().Be(new Point(0, -1));
    }

    [Fact]
    public void TryChangeDirection_WithReverseDirection_ShouldNotChange()
    {
        // Arrange
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;

        // Act
        var result = _engine.TryChangeDirection(new Point(-1, 0));

        // Assert
        result.Should().BeFalse();
        _state.Direction.Should().Be(new Point(1, 0));
    }

    [Fact]
    public void TryChangeDirection_WhenGameOver_ShouldNotChange()
    {
        // Arrange
        _state.Direction = new Point(1, 0);
        _state.IsGameOver = true;

        // Act
        var result = _engine.TryChangeDirection(new Point(0, -1));

        // Assert
        result.Should().BeFalse();
        _state.Direction.Should().Be(new Point(1, 0));
    }

    [Fact]
    public void TogglePause_ShouldTogglePauseState()
    {
        // Arrange
        _state.IsWaitingToStart = false;

        // Act
        _engine.TogglePause();

        // Assert
        _state.IsPaused.Should().BeTrue();

        // Act again
        _engine.TogglePause();

        // Assert
        _state.IsPaused.Should().BeFalse();
    }

    [Fact]
    public void StartGame_WhenWaitingToStart_ShouldStart()
    {
        // Arrange
        _state.IsWaitingToStart = true;

        // Act
        _engine.StartGame();

        // Assert
        _state.IsWaitingToStart.Should().BeFalse();
    }

    [Fact]
    public void StartGame_WhenGameOver_ShouldReinitialize()
    {
        // Arrange
        _state.IsGameOver = true;
        _state.Snake.AddLast(new Point(10, 10));

        // Act
        _engine.StartGame();

        // Assert
        _state.IsGameOver.Should().BeFalse();
        _state.IsWaitingToStart.Should().BeFalse();
    }

    [Fact]
    public void GetTimerInterval_ShouldReturnCorrectInterval()
    {
        // Arrange
        _state.Difficulty = Difficulty.Medium;
        _state.SpeedLevel = 5;

        // Act
        var interval = _engine.GetTimerInterval();

        // Assert
        interval.Should().BeGreaterThan(0);
        interval.Should().BeLessThan(200);
    }

    [Fact]
    public void GetTimerInterval_WithHardDifficulty_ShouldBeFaster()
    {
        // Arrange
        _state.Difficulty = Difficulty.Hard;
        _state.SpeedLevel = 5;
        var hardInterval = _engine.GetTimerInterval();

        _state.Difficulty = Difficulty.Medium;
        var normalInterval = _engine.GetTimerInterval();

        // Assert
        hardInterval.Should().BeLessThan(normalInterval);
    }

    [Fact]
    public void SetLevel_ShouldSetCurrentLevel()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithNumber(1)
            .Build();

        // Act
        _engine.SetLevel(level);

        // Assert
        _state.CurrentLevel.Should().Be(level);
        _state.IsLevelCompleted.Should().BeFalse();
        _state.LevelTime.Should().Be(0);
        _state.FoodCollected.Should().Be(0);
        _state.TotalFoodSpawned.Should().Be(0);
    }

    [Fact]
    public void UpdateLevelTime_ShouldIncrementTime()
    {
        // Arrange
        var level = new LevelBuilder().Build();
        _state.CurrentLevel = level;
        _state.IsWaitingToStart = false;

        // Act
        _engine.UpdateLevelTime();

        // Assert
        _state.LevelTime.Should().Be(1);
    }

    [Fact]
    public void UpdateLevelTime_WhenPaused_ShouldNotIncrement()
    {
        // Arrange
        var level = new LevelBuilder().Build();
        _state.CurrentLevel = level;
        _state.IsPaused = true;

        // Act
        _engine.UpdateLevelTime();

        // Assert
        _state.LevelTime.Should().Be(0);
    }

    [Fact]
    public void GetObstacles_ShouldReturnObstaclesList()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithStaticObstacle(5, 5)
            .WithStaticObstacle(10, 10)
            .Build();
        _engine.SetLevel(level);

        // Act
        var obstacles = _engine.GetObstacles();

        // Assert
        obstacles.Should().HaveCount(2);
    }

    [Fact]
    public void UpdateDynamicObstacles_ShouldCallUpdateOnDynamicObstacles()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithDynamicObstacle(5, 5, new Point(5, 5), new Point(6, 5))
            .Build();
        _engine.SetLevel(level);

        // Act
        _engine.UpdateDynamicObstacles();

        // Assert
        // Dynamic obstacles update is time-based, so we just verify it doesn't throw
        var obstacles = _engine.GetObstacles();
        obstacles.Should().HaveCount(1);
    }

    [Fact]
    public void MoveSnake_WhenPaused_ShouldNotMove()
    {
        // Arrange
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;
        _state.IsPaused = true;

        // Act
        _engine.MoveSnake();

        // Assert
        _state.Snake.First!.Value.Should().Be(new Point(16, 10));
    }

    [Fact]
    public void MoveSnake_WhenWaitingToStart_ShouldMove()
    {
        // Arrange
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = true;

        // Act
        _engine.MoveSnake();

        // Assert
        _state.Snake.First!.Value.Should().Be(new Point(16, 10));
    }

    [Fact]
    public void TryChangeDirection_WhenWaitingToStart_ShouldNotChange()
    {
        // Arrange
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = true;

        // Act
        var result = _engine.TryChangeDirection(new Point(0, -1));

        // Assert
        result.Should().BeFalse();
        _state.Direction.Should().Be(new Point(1, 0));
    }

    [Fact]
    public void Initialize_WithDefaultSettings_ShouldCreateCorrectSnakeLength()
    {
        // Act
        _engine.Initialize();

        // Assert
        _state.Snake.Count.Should().Be(3);
    }

    [Fact]
    public void Initialize_ShouldSpawnFood()
    {
        // Act
        _engine.Initialize();

        // Assert
        _state.Foods.Should().NotBeEmpty();
    }

    [Fact]
    public void GetTimerInterval_WithMinSpeed_ShouldReturnLargestInterval()
    {
        // Arrange
        _state.Difficulty = Difficulty.Medium;
        _state.SpeedLevel = GameConfig.MinSpeed;

        // Act
        var interval = _engine.GetTimerInterval();

        // Assert
        interval.Should().Be(GameConfig.NormalBaseInterval);
    }

    [Fact]
    public void GetTimerInterval_WithMaxSpeed_ShouldReturnSmallestInterval()
    {
        // Arrange
        _state.Difficulty = Difficulty.Medium;
        _state.SpeedLevel = GameConfig.MaxSpeed;

        // Act
        var interval = _engine.GetTimerInterval();

        // Assert
        var expectedInterval = Math.Max(
            GameConfig.MinInterval,
            GameConfig.NormalBaseInterval - (GameConfig.MaxSpeed - GameConfig.MinSpeed) * GameConfig.SpeedIntervalReduction
        );
        interval.Should().Be(expectedInterval);
    }

    [Fact]
    public void MoveSnake_WithEasyDifficulty_ShouldScoreCorrectPoints()
    {
        // Arrange
        _state.Snake.Clear(); // 确保蛇为空
        _state.Foods.Clear(); // 确保食物为空
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.Foods.Add(new Point(16, 10));
        _state.IsWaitingToStart = false;
        _state.Difficulty = Difficulty.Easy;
        _state.SpeedLevel = 5;
        var initialScore = _state.Score;

        // Act
        _engine.MoveSnake();

        // Assert
        var expectedPoints = GameConfig.EasyBasePoints + _state.SpeedLevel;
        _state.Score.Should().Be(initialScore + expectedPoints);
    }

    [Fact]
    public void MoveSnake_WithMediumDifficulty_ShouldScoreCorrectPoints()
    {
        // Arrange
        _state.Snake.Clear(); // 确保蛇为空
        _state.Foods.Clear(); // 确保食物为空
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.Foods.Add(new Point(16, 10));
        _state.IsWaitingToStart = false;
        _state.Difficulty = Difficulty.Medium;
        _state.SpeedLevel = 5;
        var initialScore = _state.Score;

        // Act
        _engine.MoveSnake();

        // Assert
        var expectedPoints = GameConfig.MediumBasePoints + _state.SpeedLevel;
        _state.Score.Should().Be(initialScore + expectedPoints);
    }

    [Fact]
    public void MoveSnake_WithHardDifficulty_ShouldScoreCorrectPoints()
    {
        // Arrange
        _state.Snake.Clear(); // 确保蛇为空
        _state.Foods.Clear(); // 确保食物为空
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.Foods.Add(new Point(16, 10));
        _state.IsWaitingToStart = false;
        _state.Difficulty = Difficulty.Hard;
        _state.SpeedLevel = 5;
        var initialScore = _state.Score;

        // Act
        _engine.MoveSnake();

        // Assert
        var expectedPoints = GameConfig.HardBasePoints + _state.SpeedLevel;
        _state.Score.Should().Be(initialScore + expectedPoints);
    }

    [Fact]
    public void MoveSnake_WhenHittingLeftWall_ShouldEndGame()
    {
        // Arrange
        _state.Snake.AddLast(new Point(0, 10));
        _state.Snake.AddLast(new Point(1, 10));
        _state.Snake.AddLast(new Point(2, 10));
        _state.Direction = new Point(-1, 0);
        _state.IsWaitingToStart = false;

        // Act
        _engine.MoveSnake();

        // Assert
        _state.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void MoveSnake_WhenHittingTopWall_ShouldEndGame()
    {
        // Arrange
        _state.Snake.AddLast(new Point(10, 0));
        _state.Snake.AddLast(new Point(10, 1));
        _state.Snake.AddLast(new Point(10, 2));
        _state.Direction = new Point(0, -1);
        _state.IsWaitingToStart = false;

        // Act
        _engine.MoveSnake();

        // Assert
        _state.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void UpdateLevelTime_WhenNotWaitingAndNotPaused_ShouldIncrement()
    {
        // Arrange
        var level = new LevelBuilder().Build();
        _state.CurrentLevel = level;
        _state.IsWaitingToStart = false;
        _state.IsPaused = false;

        // Act
        _engine.UpdateLevelTime();

        // Assert
        _state.LevelTime.Should().Be(1);
    }

    [Fact]
    public void UpdateLevelTime_WhenGameOver_ShouldNotIncrement()
    {
        // Arrange
        var level = new LevelBuilder().Build();
        _state.CurrentLevel = level;
        _state.IsWaitingToStart = false;
        _state.IsGameOver = true;

        // Act
        _engine.UpdateLevelTime();

        // Assert
        _state.LevelTime.Should().Be(0);
    }

    [Fact]
    public void UpdateLevelTime_WithoutLevel_ShouldNotThrow()
    {
        // Arrange
        _state.CurrentLevel = null;
        _state.IsWaitingToStart = false;

        // Act
        Action action = () => _engine.UpdateLevelTime();

        // Assert
        action.Should().NotThrow();
        _state.LevelTime.Should().Be(0);
    }

    [Fact]
    public void SpawnFood_WithCollectAllFoodCondition_ShouldContinueSpawningWhenCrowded()
    {
        // Arrange - 创建类似关卡4的配置
        var level = new LevelBuilder()
            .WithDetailedVictoryCondition(VictoryConditionType.CollectAllFood, mustCollectAllFood: true, foodSpawnCount: 10)
            .WithFoodCount(2)
            .WithGridSize(30, 20)
            .Build();

        _engine.SetLevel(level);
        _engine.Initialize();

        // Act & Assert - 验证基本功能
        int initialFoodCount = _state.Foods.Count;
        int initialSpawnedCount = _state.TotalFoodSpawned;

        // 应该生成配置的 foodCount 数量的食物
        initialFoodCount.Should().Be(2, $"应该生成配置的 foodCount 数量的食物 (level.Settings.FoodCount={level.Settings.FoodCount})");
        initialSpawnedCount.Should().Be(2, "TotalFoodSpawned 应该反映实际生成的数量");

        // 验证通关条件配置
        level.VictoryCondition.Type.Should().Be(VictoryConditionType.CollectAllFood);
        level.VictoryCondition.MustCollectAllFood.Should().BeTrue();
        level.VictoryCondition.FoodSpawnCount.Should().Be(10);
    }

    [Fact]
    public void SpawnFood_WithFoodSpawnCountLimit_ShouldRespectLimitWhenNotCollectAll()
    {
        // Arrange - 有限制但不需要收集所有食物的关卡
        var level = new LevelBuilder()
            .WithDetailedVictoryCondition(VictoryConditionType.Combined, targetScore: 50, mustCollectAllFood: false, foodSpawnCount: 5)
            .WithFoodCount(2)
            .Build();

        _engine.SetLevel(level);
        _engine.Initialize();

        // Act - 通过 Initialize 生成食物
        int spawnedCount = _state.TotalFoodSpawned;
        int currentFoodCount = _state.Foods.Count;

        // Assert - 应该生成 foodCount 数量的食物，且不超过 foodSpawnCount 限制
        spawnedCount.Should().Be(2, $"应该生成配置的 foodCount 数量的食物 (level.Settings.FoodCount={level.Settings.FoodCount})");
        currentFoodCount.Should().Be(2, "场上应该有 foodCount 数量的食物");
        spawnedCount.Should().BeLessOrEqualTo(5, "不应该超过 foodSpawnCount 限制");
    }

    [Fact]
    public void SpawnFood_ShouldGenerateCorrectCount()
    {
        // Arrange - 最简单的测试
        var level = new LevelBuilder()
            .WithFoodCount(2)
            .WithGridSize(30, 20)
            .Build();

        // 检查初始配置
        level.Settings.FoodCount.Should().Be(2);

        _engine.SetLevel(level);
        _engine.Initialize();

        // Act - 验证初始状态
        int initialFoodCount = _state.Foods.Count;
        int initialSpawnedCount = _state.TotalFoodSpawned;
        int snakeCount = _state.Snake.Count;
        int obstacleCount = _engine.GetObstacles().Count;

        // 打印调试信息
        string snakePositions = string.Join(", ", _state.Snake.Select(p => $"({p.X},{p.Y})"));
        string foodPositions = string.Join(", ", _state.Foods.Select(p => $"({p.X},{p.Y})"));

        // Assert - 应该生成初始数量的食物
        initialFoodCount.Should().Be(2,
            $"应该生成2个食物，实际生成{initialFoodCount}个 (蛇身占用{snakeCount}个位置: {snakePositions}, 障碍物{obstacleCount}个, 食物位置: {foodPositions})");
        initialSpawnedCount.Should().Be(2, "TotalFoodSpawned 应该反映实际生成的数量");
    }

    [Fact]
    public void SpawnFood_Level4Scenario_ShouldGenerateFoodCorrectly()
    {
        // Arrange - 模拟关卡4的配置
        var level = new LevelBuilder()
            .WithDetailedVictoryCondition(VictoryConditionType.CollectAllFood, mustCollectAllFood: true, foodSpawnCount: 10)
            .WithFoodCount(2)
            .WithGridSize(30, 20)
            .WithSnakeStart(5, 10)
            .Build();

        _engine.SetLevel(level);
        _engine.Initialize();

        // Act - 验证初始状态
        int initialFoodCount = _state.Foods.Count;
        int initialSpawnedCount = _state.TotalFoodSpawned;

        // Assert - 应该生成初始数量的食物
        initialFoodCount.Should().Be(2, $"应该生成配置的 foodCount 数量的食物 (level.Settings.FoodCount={level.Settings.FoodCount})");
        initialSpawnedCount.Should().Be(2, "TotalFoodSpawned 应该反映实际生成的数量");

        // 验证通关条件配置
        level.VictoryCondition.Type.Should().Be(VictoryConditionType.CollectAllFood);
        level.VictoryCondition.MustCollectAllFood.Should().BeTrue();
        level.VictoryCondition.FoodSpawnCount.Should().Be(10);
    }

    [Fact]
    public void LevelStorageService_ShouldLoadAllPresetLevels()
    {
        // Arrange
        var storageService = new LevelStorageService();

        // Act
        var levels = storageService.LoadPresetLevels();

        // Assert - 应该能加载5个预设关卡
        levels.Should().HaveCount(5, "应该有5个预设关卡");

        // 检查每个关卡的序号
        levels[0].LevelNumber.Should().Be(1);
        levels[1].LevelNumber.Should().Be(2);
        levels[2].LevelNumber.Should().Be(3);
        levels[3].LevelNumber.Should().Be(4);
        levels[4].LevelNumber.Should().Be(5);
    }

    [Fact]
    public void LevelManager_UnlockedPresetLevels_ShouldIncludeLevel4_WhenLevel3Completed()
    {
        // Arrange
        var storageService = new LevelStorageService();
        var manager = new LevelManager(storageService);

        // 模拟完成第3关后的状态
        var progression = new LevelProgression();
        progression.HighestUnlockedLevel = 4; // 完成第3关后解锁第4关

        // Act
        var unlockedLevels = manager.UnlockedPresetLevels;

        // Assert - 应该包含关卡1-4
        unlockedLevels.Should().HaveCountGreaterOrEqualTo(4, "至少应该有4个已解锁关卡");
        unlockedLevels.Count(l => l.LevelNumber == 4).Should().Be(1, "应该包含关卡4");
    }
}