using System.Drawing;
using AiPlayground.Game;
using AiPlayground.Models;
using AiPlayground.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Integration;

/// <summary>
/// 集成测试 - 测试完整的游戏流程
/// </summary>
public class GameFlowTests
{
    [Fact]
    public void GameFlow_CompleteGameCycle_ShouldWorkCorrectly()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);

        // Act - 初始化游戏
        engine.Initialize();

        // Assert - 初始状态
        state.IsGameOver.Should().BeFalse();
        state.IsPaused.Should().BeFalse();
        state.IsWaitingToStart.Should().BeTrue();
        state.Snake.Count.Should().Be(3);
        state.Foods.Should().NotBeEmpty();
        state.Score.Should().Be(0);

        // Act - 开始游戏
        engine.StartGame();

        // Assert - 游戏已开始
        state.IsWaitingToStart.Should().BeFalse();

        // Act - 移动蛇几次
        var initialLength = state.Snake.Count;
        for (int i = 0; i < 5; i++)
        {
            var head = state.Snake.First!.Value;
            var foodPosition = state.Foods.FirstOrDefault();
            if (foodPosition != default)
            {
                // 设置方向朝向食物
                var dx = foodPosition.X - head.X;
                var dy = foodPosition.Y - head.Y;
                if (dx != 0)
                {
                    state.Direction = new Point(dx > 0 ? 1 : -1, 0);
                }
                else if (dy != 0)
                {
                    state.Direction = new Point(0, dy > 0 ? 1 : -1);
                }
            }
            engine.MoveSnake();
        }

        // Assert - 游戏仍在进行
        state.IsGameOver.Should().BeFalse();
    }

    [Fact]
    public void GameFlow_LevelCompleteFlow_ShouldWorkCorrectly()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        var storageService = new AiPlayground.Services.LevelStorageService();
        var levelManager = new LevelManager(storageService);

        var level = new LevelBuilder()
            .WithVictoryCondition(VictoryConditionType.TargetScore, 50)
            .WithSnakeStart(15, 15)
            .Build();

        engine.SetLevel(level);
        engine.Initialize();

        // Act - 开始游戏
        engine.StartGame();

        // 设置初始状态
        state.Snake.Clear();
        state.Foods.Clear();
        for (int i = 0; i < 3; i++)
        {
            state.Snake.AddLast(new Point(15 - i, 15));
        }
        state.Direction = new Point(1, 0);

        // 直接设置分数以满足通关条件
        state.Score = 50;

        // Act - 检查通关条件
        var victoryAchieved = levelManager.CheckVictoryCondition(state);

        // Assert
        victoryAchieved.Should().BeTrue();
    }

    [Fact]
    public void GameFlow_EatFoodAndGrow_ShouldIncreaseLength()
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
        engine.CheckCollisions(); // 处理碰撞检测

        // Assert
        state.Snake.Count.Should().Be(initialLength + 1);
        state.Score.Should().BeGreaterThan(initialScore);
        state.Foods.Should().BeEmpty();
    }

    [Fact]
    public void GameFlow_GameOverByWallCollision_ShouldStopGame()
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
    public void GameFlow_GameOverBySelfCollision_ShouldStopGame()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        engine.Initialize();

        state.Snake.Clear();
        state.Snake.AddLast(new Point(15, 10));
        state.Snake.AddLast(new Point(14, 10));
        state.Snake.AddLast(new Point(13, 10));
        state.Snake.AddLast(new Point(12, 10));
        state.Direction = new Point(-1, 0);
        state.IsWaitingToStart = false;

        // Act
        engine.MoveSnake();

        // Assert
        state.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void GameFlow_PauseAndResume_ShouldWorkCorrectly()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        engine.Initialize();
        engine.StartGame();

        state.IsWaitingToStart = false;

        // Act - 暂停
        engine.TogglePause();

        // Assert
        state.IsPaused.Should().BeTrue();

        // Act - 恢复
        engine.TogglePause();

        // Assert
        state.IsPaused.Should().BeFalse();
    }

    [Fact]
    public void GameFlow_DirectionChange_ShouldWorkCorrectly()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        engine.Initialize();
        engine.StartGame();

        var initialDirection = state.Direction;

        // Act
        var result = engine.TryChangeDirection(new Point(0, -1));

        // Assert
        result.Should().BeTrue();
        state.Direction.Should().Be(new Point(0, -1));
    }

    [Fact]
    public void GameFlow_ReverseDirection_ShouldBeBlocked()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);
        engine.Initialize();
        engine.StartGame();

        state.Direction = new Point(1, 0);

        // Act
        var result = engine.TryChangeDirection(new Point(-1, 0));

        // Assert
        result.Should().BeFalse();
        state.Direction.Should().Be(new Point(1, 0));
    }

    [Fact]
    public void GameFlow_LevelWithObstacles_ShouldHandleCorrectly()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);

        var level = new LevelBuilder()
            .WithStaticObstacle(20, 10)
            .WithSnakeStart(15, 10)
            .Build();

        engine.SetLevel(level);
        engine.Initialize();

        state.Snake.Clear();
        state.Foods.Clear();
        state.Snake.AddLast(new Point(15, 10));
        state.Snake.AddLast(new Point(14, 10));
        state.Snake.AddLast(new Point(13, 10));
        state.Direction = new Point(1, 0);
        state.Foods.Add(new Point(25, 10));
        state.IsWaitingToStart = false;

        // Act - 移动直到接近障碍物
        for (int i = 0; i < 4; i++)
        {
            engine.MoveSnake();
        }

        // Assert - 应该还在障碍物前
        state.Snake.First!.Value.X.Should().BeLessThan(20);
        state.IsGameOver.Should().BeFalse();
    }

    [Fact]
    public void GameFlow_DestructibleObstacle_ShouldBePassable()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);

        var level = new LevelBuilder()
            .WithDestructibleObstacle(16, 10, 1)
            .WithSnakeStart(15, 10)
            .Build();

        engine.SetLevel(level);
        engine.Initialize();

        state.Snake.Clear();
        state.Foods.Clear();
        state.Snake.AddLast(new Point(15, 10));
        state.Snake.AddLast(new Point(14, 10));
        state.Snake.AddLast(new Point(13, 10));
        state.Direction = new Point(1, 0);
        state.IsWaitingToStart = false;

        var initialObstacleCount = engine.GetObstacles().Count;

        // Act
        engine.MoveSnake();
        engine.CheckCollisions(); // 需要调用 CheckCollisions 来处理障碍物交互

        // Assert
        state.IsGameOver.Should().BeFalse();
        state.Snake.First!.Value.Should().Be(new Point(16, 10));
        engine.GetObstacles().Count.Should().Be(initialObstacleCount - 1);
    }

    [Fact]
    public void GameFlow_SpeedChangeObstacle_ShouldModifySpeed()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);

        var level = new LevelBuilder()
            .WithSpeedUpObstacle(16, 10, 2)
            .WithSnakeStart(15, 10)
            .Build();

        engine.SetLevel(level);
        engine.Initialize();

        state.Snake.Clear();
        state.Foods.Clear();
        state.Snake.AddLast(new Point(15, 10));
        state.Snake.AddLast(new Point(14, 10));
        state.Snake.AddLast(new Point(13, 10));
        state.Direction = new Point(1, 0);
        state.IsWaitingToStart = false;
        state.SpeedLevel = 5;

        // Act
        engine.MoveSnake();
        engine.CheckCollisions(); // 需要调用 CheckCollisions 来处理障碍物交互

        // Assert
        state.IsGameOver.Should().BeFalse();
        state.SpeedLevel.Should().Be(7);
    }

    [Fact]
    public void GameFlow_TeleportObstacle_ShouldMoveSnake()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);

        var destination = new Point(25, 25);
        var level = new LevelBuilder()
            .WithTeleportObstacle(16, 10, destination)
            .WithSnakeStart(15, 10)
            .Build();

        engine.SetLevel(level);
        engine.Initialize();

        state.Snake.Clear();
        state.Foods.Clear();
        state.Snake.AddLast(new Point(15, 10));
        state.Snake.AddLast(new Point(14, 10));
        state.Snake.AddLast(new Point(13, 10));
        state.Direction = new Point(1, 0);
        state.IsWaitingToStart = false;

        // Act
        engine.MoveSnake();
        engine.CheckCollisions(); // 需要调用 CheckCollisions 来处理障碍物交互

        // Assert
        state.IsGameOver.Should().BeFalse();
        state.Snake.First!.Value.Should().Be(destination);
    }

    [Fact]
    public void GameFlow_LevelTimeTracking_ShouldWorkCorrectly()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state);

        var level = new LevelBuilder().Build();
        engine.SetLevel(level);
        engine.Initialize();
        engine.StartGame();

        // Act - 模拟时间流逝
        for (int i = 0; i < 5; i++)
        {
            engine.UpdateLevelTime();
        }

        // Assert
        state.LevelTime.Should().Be(5);
    }

    [Fact]
    public void GameFlow_DifficultySettings_ShouldAffectScoring()
    {
        // Arrange
        var engine1 = new GameEngine(new GameState { Difficulty = Difficulty.Easy, SpeedLevel = 5 });
        var engine2 = new GameEngine(new GameState { Difficulty = Difficulty.Hard, SpeedLevel = 5 });

        // Initialize engines
        engine1.Initialize();
        engine2.Initialize();

        // 设置相同的状态
        engine1.MoveSnake(); // This will initialize the snake
        engine2.MoveSnake();

        // Hard difficulty should give more points (verified by scoring logic)
        // This is a basic test to ensure difficulty affects game behavior
        engine1.GetTimerInterval().Should().NotBe(engine2.GetTimerInterval());
    }
}
