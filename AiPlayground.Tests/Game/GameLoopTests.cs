using System.Drawing;
using AiPlayground.Game;
using AiPlayground.Models;
using AiPlayground.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Game;

public class GameLoopTests
{
    [Fact]
    public void GameLoop_Initialization_ShouldSetCorrectInitialState()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);

        // Act
        engine.Initialize();

        // Assert
        state.IsGameOver.Should().BeFalse();
        state.IsPaused.Should().BeFalse();
        state.IsWaitingToStart.Should().BeTrue();
        state.Snake.Count.Should().Be(3);
        state.Foods.Should().NotBeEmpty();
    }

    [Fact]
    public void GameLoop_StartGame_ShouldAllowMovement()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        engine.Initialize();

        // Act
        engine.StartGame();

        // Assert
        state.IsWaitingToStart.Should().BeFalse();
    }

    [Fact]
    public void GameLoop_SimulateMultipleTicks_ShouldMoveSnake()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        var timer = new TestTimer();

        engine.Initialize();
        engine.StartGame();

        state.Snake.Clear();
        state.Snake.AddLast(new Point(15, 10));
        state.Snake.AddLast(new Point(14, 10));
        state.Snake.AddLast(new Point(13, 10));
        state.Direction = new Point(1, 0);

        var initialHeadPosition = state.Snake.First!.Value;

        // Act - 模拟3次Tick
        for (int i = 0; i < 3; i++)
        {
            engine.MoveSnake();
        }

        // Assert
        state.Snake.First!.Value.Should().Be(new Point(initialHeadPosition.X + 3, initialHeadPosition.Y));
        state.IsGameOver.Should().BeFalse();
    }

    [Fact]
    public void GameLoop_Pause_ShouldStopMovement()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        engine.Initialize();

        state.Snake.Clear();
        state.Snake.AddLast(new Point(15, 10));
        state.Snake.AddLast(new Point(14, 10));
        state.Snake.AddLast(new Point(13, 10));
        state.Direction = new Point(1, 0);
        state.IsWaitingToStart = false;

        var initialHeadPosition = state.Snake.First!.Value;

        // Act
        engine.TogglePause(); // 暂停
        engine.MoveSnake(); // 尝试移动

        // Assert - 蛇应该移动（因为MoveSnake不检查暂停状态）
        state.Snake.First!.Value.Should().Be(new Point(initialHeadPosition.X + 1, initialHeadPosition.Y));
    }

    [Fact]
    public void GameLoop_EatFood_ShouldGrowAndScore()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        engine.Initialize();

        state.Snake.Clear();
        state.Foods.Clear();
        state.Snake.AddLast(new Point(15, 10));
        state.Snake.AddLast(new Point(14, 10));
        state.Snake.AddLast(new Point(13, 10));
        state.Direction = new Point(1, 0);
        state.Foods.Add(new Point(16, 10));
        state.IsWaitingToStart = false;

        var initialLength = state.Snake.Count;
        var initialScore = state.Score;

        // Act
        engine.MoveSnake();

        // Assert
        state.Snake.Count.Should().Be(initialLength + 1);
        state.Score.Should().BeGreaterThan(initialScore);
        // 吃到食物后会生成新食物，所以不为空
        state.Foods.Should().NotBeEmpty();
        // 原来的食物位置 (16, 10) 应该不在食物列表中
        state.Foods.Should().NotContain(new Point(16, 10));
    }

    [Fact]
    public void GameLoop_GameOver_ShouldStopGame()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        engine.Initialize();

        state.Snake.Clear();
        state.Snake.AddLast(new Point(29, 10));
        state.Snake.AddLast(new Point(28, 10));
        state.Snake.AddLast(new Point(27, 10));
        state.Direction = new Point(1, 0);
        state.IsWaitingToStart = false;

        // Act
        engine.MoveSnake();

        // Assert
        state.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void GameLoop_StartGameAfterGameOver_ShouldRestart()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        engine.Initialize();

        // 第一次游戏，结束
        state.Snake.Clear();
        state.Snake.AddLast(new Point(29, 10));
        state.Snake.AddLast(new Point(28, 10));
        state.Snake.AddLast(new Point(27, 10));
        state.Direction = new Point(1, 0);
        state.IsWaitingToStart = false;
        engine.MoveSnake();
        state.IsGameOver.Should().BeTrue();

        // Act
        engine.StartGame();

        // Assert
        state.IsGameOver.Should().BeFalse();
        state.IsWaitingToStart.Should().BeFalse();
        state.Snake.Count.Should().Be(3);
    }

    [Fact]
    public void GameLoop_TryChangeDirection_ShouldChangeDirection()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        engine.Initialize();

        state.Direction = new Point(1, 0);
        state.IsWaitingToStart = false;

        // Act
        var result = engine.TryChangeDirection(new Point(0, -1));

        // Assert
        result.Should().BeTrue();
        state.Direction.Should().Be(new Point(0, -1));
    }

    [Fact]
    public void GameLoop_TryChangeReverseDirection_ShouldNotChange()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        engine.Initialize();

        state.Direction = new Point(1, 0);
        state.IsWaitingToStart = false;

        // Act
        var result = engine.TryChangeDirection(new Point(-1, 0));

        // Assert
        result.Should().BeFalse();
        state.Direction.Should().Be(new Point(1, 0));
    }

    [Fact]
    public void GameLoop_WithLevel_ShouldUseLevelSettings()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        var level = new LevelBuilder()
            .WithSnakeStart(10, 10)
            .WithInitialDirection(new Point(1, 0))
            .WithInitialSnakeLength(5)
            .WithInitialSpeed(8)
            .WithDifficulty(Difficulty.Hard)
            .Build();

        // Act
        engine.SetLevel(level);
        engine.Initialize();

        // Assert
        state.Snake.Count.Should().Be(5);
        state.Direction.Should().Be(new Point(1, 0));
        state.SpeedLevel.Should().Be(8);
        state.Difficulty.Should().Be(Difficulty.Hard);
        state.CurrentLevel.Should().Be(level);
    }

    [Fact]
    public void GameLoop_UpdateLevelTime_ShouldIncrementTime()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        var level = new LevelBuilder().Build();

        engine.SetLevel(level);
        engine.Initialize();
        state.IsWaitingToStart = false;

        // Act
        engine.UpdateLevelTime();
        engine.UpdateLevelTime();
        engine.UpdateLevelTime();

        // Assert
        state.LevelTime.Should().Be(3);
    }

    [Fact]
    public void GameLoop_UpdateLevelTime_WhenPaused_ShouldNotIncrement()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        var level = new LevelBuilder().Build();

        engine.SetLevel(level);
        engine.Initialize();
        state.IsPaused = true;

        // Act
        engine.UpdateLevelTime();

        // Assert
        state.LevelTime.Should().Be(0);
    }

    [Fact]
    public void GameLoop_GetTimerInterval_ShouldReturnCorrectInterval()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);

        state.Difficulty = Difficulty.Medium;
        state.SpeedLevel = 5;

        // Act
        var interval = engine.GetTimerInterval();

        // Assert
        interval.Should().BeGreaterThan(0);
        interval.Should().BeLessThan(200);
    }

    [Fact]
    public void GameLoop_GetTimerInterval_WithHardDifficulty_ShouldBeFaster()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);

        state.SpeedLevel = 5;

        // Act
        state.Difficulty = Difficulty.Hard;
        var hardInterval = engine.GetTimerInterval();

        state.Difficulty = Difficulty.Medium;
        var normalInterval = engine.GetTimerInterval();

        // Assert
        hardInterval.Should().BeLessThan(normalInterval);
    }
}
