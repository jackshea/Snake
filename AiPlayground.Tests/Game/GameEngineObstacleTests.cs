using System.Drawing;
using AiPlayground.Game;
using AiPlayground.Models;
using AiPlayground.Models.Obstacles;
using AiPlayground.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Game;

public class GameEngineObstacleTests
{
    private readonly GameState _state;
    private readonly GameEngine _engine;

    public GameEngineObstacleTests()
    {
        _state = new GameState();
        _engine = new GameEngine(_state);
    }

    [Fact]
    public void MoveSnake_IntoStaticObstacle_ShouldEndGame()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithStaticObstacle(16, 10)
            .Build();
        _engine.SetLevel(level);
        _engine.Initialize();

        _state.Snake.Clear();
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;

        // Act
        _engine.MoveSnake();

        // Assert
        _state.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void MoveSnake_IntoDestructibleObstacle_ShouldPassAndRemove()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithDestructibleObstacle(16, 10, 1)
            .Build();
        _engine.SetLevel(level);
        _engine.Initialize();

        _state.Snake.Clear();
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;

        var initialObstacleCount = _engine.GetObstacles().Count;

        // Act
        _engine.MoveSnake();
        _engine.CheckCollisions(); // 需要调用 CheckCollisions 来处理障碍物交互

        // Assert
        _state.IsGameOver.Should().BeFalse();
        _state.Snake.First!.Value.Should().Be(new Point(16, 10));
        _engine.GetObstacles().Count.Should().Be(initialObstacleCount - 1);
    }

    [Fact]
    public void MoveSnake_IntoDestructibleObstacleMultiplePasses_ShouldDestructCorrectly()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithDestructibleObstacle(16, 10, 2)
            .Build();
        _engine.SetLevel(level);
        _engine.Initialize();

        _state.Snake.Clear();
        _state.Foods.Clear();
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;

        var initialObstacleCount = _engine.GetObstacles().Count;

        // Act - 第一次通过
        _engine.MoveSnake();
        _engine.CheckCollisions(); // 处理障碍物交互
        _state.IsGameOver.Should().BeFalse();
        _engine.GetObstacles().Count.Should().Be(initialObstacleCount); // 障碍物还在

        // Act - 第二次通过
        _state.Snake.Clear();
        _state.Snake.AddLast(new Point(16, 10));
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));

        _engine.MoveSnake();
        _engine.CheckCollisions(); // 处理障碍物交互

        // Assert - 障碍物应该被移除
        _engine.GetObstacles().Count.Should().Be(initialObstacleCount - 1);
    }

    [Fact]
    public void MoveSnake_IntoSpeedUpObstacle_ShouldIncreaseSpeed()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithSpeedUpObstacle(16, 10, 2)
            .Build();
        _engine.SetLevel(level);
        _engine.Initialize();

        _state.Snake.Clear();
        _state.Foods.Clear();
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;
        _state.SpeedLevel = 5;

        // Act
        _engine.MoveSnake();
        _engine.CheckCollisions(); // 处理障碍物交互

        // Assert
        _state.IsGameOver.Should().BeFalse();
        _state.SpeedLevel.Should().Be(7); // 5 + 2
    }

    [Fact]
    public void MoveSnake_IntoSpeedDownObstacle_ShouldDecreaseSpeed()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithSpeedDownObstacle(16, 10, -2)
            .Build();
        _engine.SetLevel(level);
        _engine.Initialize();

        _state.Snake.Clear();
        _state.Foods.Clear();
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;
        _state.SpeedLevel = 5;

        // Act
        _engine.MoveSnake();
        _engine.CheckCollisions(); // 处理障碍物交互

        // Assert
        _state.IsGameOver.Should().BeFalse();
        _state.SpeedLevel.Should().Be(3); // 5 - 2
    }

    [Fact]
    public void MoveSnake_IntoSpeedDownObstacle_ShouldNotGoBelowMinSpeed()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithSpeedDownObstacle(16, 10, -5)
            .Build();
        _engine.SetLevel(level);
        _engine.Initialize();

        _state.Snake.Clear();
        _state.Foods.Clear();
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;
        _state.SpeedLevel = 2;

        // Act
        _engine.MoveSnake();
        _engine.CheckCollisions(); // 处理障碍物交互

        // Assert
        _state.SpeedLevel.Should().Be(1); // 最小速度为1
    }

    [Fact]
    public void MoveSnake_IntoTeleportObstacle_ShouldTeleportSnake()
    {
        // Arrange
        var destination = new Point(25, 25);
        var level = new LevelBuilder()
            .WithTeleportObstacle(16, 10, destination)
            .Build();
        _engine.SetLevel(level);
        _engine.Initialize();

        _state.Snake.Clear();
        _state.Foods.Clear();
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;

        // Act
        _engine.MoveSnake();
        _engine.CheckCollisions(); // 处理障碍物交互

        // Assert
        _state.IsGameOver.Should().BeFalse();
        _state.Snake.First!.Value.Should().Be(destination);
    }

    [Fact]
    public void UpdateDynamicObstacles_ShouldMoveDynamicObstacles()
    {
        // Arrange
        var mockTime = new MockTimeProvider(1000);
        var path = new List<Point> { new Point(5, 5), new Point(6, 5), new Point(7, 5) };
        var dynamicObstacle = new DynamicObstacle(new Point(5, 5), path, mockTime)
        {
            MoveIntervalMs = 500
        };

        var level = new LevelBuilder()
            .Build();
        level.Obstacles.Add(dynamicObstacle);

        _engine.SetLevel(level);
        _engine.Initialize();

        // Act - 前进时间
        mockTime.AdvanceTime(500);
        _engine.UpdateDynamicObstacles();

        // Assert
        var obstacles = _engine.GetObstacles();
        var updatedObstacle = obstacles.OfType<DynamicObstacle>().FirstOrDefault();
        updatedObstacle.Should().NotBeNull();
        updatedObstacle!.Position.Should().Be(new Point(6, 5));
    }

    [Fact]
    public void GetObstacles_ShouldReturnAllObstacles()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithStaticObstacle(5, 5)
            .WithStaticObstacle(10, 10)
            .WithDestructibleObstacle(15, 15, 2)
            .Build();
        _engine.SetLevel(level);

        // Act
        var obstacles = _engine.GetObstacles();

        // Assert
        obstacles.Should().HaveCount(3);
    }

    [Fact]
    public void SpawnFood_ShouldNotSpawnOnObstacles()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithStaticObstacle(10, 10)
            .WithGridSize(15, 15)
            .Build();
        _engine.SetLevel(level);
        _engine.Initialize();

        // Act
        var foods = _state.Foods;

        // Assert - 食物不应该生成在障碍物位置
        foods.Should().NotContain(f => f.X == 10 && f.Y == 10);
    }

    [Fact]
    public void WillCollide_ShouldDetectStaticObstacle()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithStaticObstacle(20, 10)
            .Build();
        _engine.SetLevel(level);
        _engine.Initialize();

        _state.Snake.Clear();
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;

        // Act
        _engine.MoveSnake();

        // Assert - 在到达 (20, 10) 之前不应该碰撞
        _state.IsGameOver.Should().BeFalse();
    }

    [Fact]
    public void Initialize_ShouldResetObstacles()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithDestructibleObstacle(16, 10, 1)
            .Build();
        _engine.SetLevel(level);
        _engine.Initialize();

        // 通过障碍物使其被破坏
        _state.Snake.Clear();
        _state.Foods.Clear();
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;

        _engine.MoveSnake();
        _engine.CheckCollisions(); // 处理障碍物交互
        _engine.GetObstacles().Count.Should().Be(0); // 障碍物被破坏

        // Act
        _engine.Initialize();

        // Assert - 障碍物应该恢复
        _engine.GetObstacles().Count.Should().Be(1);
    }

    [Fact]
    public void CheckCollisions_WithMultipleObstacles_ShouldHandleCorrectly()
    {
        // Arrange
        var level = new LevelBuilder()
            .WithStaticObstacle(16, 10)
            .WithDestructibleObstacle(17, 10, 1)
            .Build();
        _engine.SetLevel(level);
        _engine.Initialize();

        _state.Snake.Clear();
        _state.Foods.Clear();
        _state.Snake.AddLast(new Point(15, 10));
        _state.Snake.AddLast(new Point(14, 10));
        _state.Snake.AddLast(new Point(13, 10));
        _state.Direction = new Point(1, 0);
        _state.IsWaitingToStart = false;

        // Act - 移动到静态障碍物
        _engine.MoveSnake();

        // Assert
        _state.IsGameOver.Should().BeTrue();
    }
}
