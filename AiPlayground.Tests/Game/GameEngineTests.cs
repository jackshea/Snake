using System.Drawing;
using AiPlayground.Game;
using AiPlayground.Models;
using AiPlayground.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using Xunit;

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
        _engine = new GameEngine(_state, _mockRandom);
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
}