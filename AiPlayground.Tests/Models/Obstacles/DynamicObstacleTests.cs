using System.Drawing;
using AiPlayground.Models.Obstacles;
using AiPlayground.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Models.Obstacles;

public class DynamicObstacleTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var obstacle = new DynamicObstacle(position);

        // Assert
        obstacle.Position.Should().Be(position);
        obstacle.Path.Should().HaveCount(1);
        obstacle.Path[0].Should().Be(position);
        obstacle.MoveIntervalMs.Should().Be(500);
        obstacle.LoopPath.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithPath_ShouldInitializeWithPath()
    {
        // Arrange
        var position = new Point(5, 5);
        var path = new List<Point> { new Point(5, 5), new Point(6, 5), new Point(7, 5) };

        // Act
        var obstacle = new DynamicObstacle(position, path);

        // Assert
        obstacle.Path.Should().HaveCount(3);
        obstacle.Path.Should().BeEquivalentTo(path);
    }

    [Fact]
    public void Constructor_WithNullPath_ShouldUseSinglePointPath()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var obstacle = new DynamicObstacle(position, null);

        // Assert
        obstacle.Path.Should().HaveCount(1);
        obstacle.Path[0].Should().Be(position);
    }

    [Fact]
    public void Update_BeforeInterval_ShouldNotMove()
    {
        // Arrange
        var mockTime = new MockTimeProvider(1000);
        var position = new Point(5, 5);
        var path = new List<Point> { new Point(5, 5), new Point(6, 5) };
        var obstacle = new DynamicObstacle(position, path, mockTime);

        // Act - 在间隔时间内调用
        mockTime.AdvanceTime(100); // 只前进100ms，小于500ms间隔
        obstacle.Update(30, 30);

        // Assert
        obstacle.Position.Should().Be(new Point(5, 5));
    }

    [Fact]
    public void Update_AfterInterval_ShouldMoveToNextPoint()
    {
        // Arrange
        var mockTime = new MockTimeProvider(1000);
        var position = new Point(5, 5);
        var path = new List<Point> { new Point(5, 5), new Point(6, 5), new Point(7, 5) };
        var obstacle = new DynamicObstacle(position, path, mockTime)
        {
            MoveIntervalMs = 500
        };

        // Act - 前进超过间隔时间
        mockTime.AdvanceTime(500);
        obstacle.Update(30, 30);

        // Assert
        obstacle.Position.Should().Be(new Point(6, 5));
    }

    [Fact]
    public void Update_WithLoopPath_ShouldLoopBackToStart()
    {
        // Arrange
        var mockTime = new MockTimeProvider(1000);
        var position = new Point(5, 5);
        var path = new List<Point> { new Point(5, 5), new Point(6, 5), new Point(7, 5) };
        var obstacle = new DynamicObstacle(position, path, mockTime)
        {
            MoveIntervalMs = 500,
            LoopPath = true
        };

        // Act - 移动超过路径长度
        mockTime.AdvanceTime(500);
        obstacle.Update(30, 30); // 移动到 (6, 5)

        mockTime.AdvanceTime(500);
        obstacle.Update(30, 30); // 移动到 (7, 5)

        mockTime.AdvanceTime(500);
        obstacle.Update(30, 30); // 应该循环回 (5, 5)

        // Assert
        obstacle.Position.Should().Be(new Point(5, 5));
    }

    [Fact]
    public void Update_WithoutLoopPath_ShouldStopAtEnd()
    {
        // Arrange
        var mockTime = new MockTimeProvider(1000);
        var position = new Point(5, 5);
        var path = new List<Point> { new Point(5, 5), new Point(6, 5), new Point(7, 5) };
        var obstacle = new DynamicObstacle(position, path, mockTime)
        {
            MoveIntervalMs = 500,
            LoopPath = false
        };

        // Act
        mockTime.AdvanceTime(500);
        obstacle.Update(30, 30); // 移动到 (6, 5)

        mockTime.AdvanceTime(500);
        obstacle.Update(30, 30); // 移动到 (7, 5)

        mockTime.AdvanceTime(500);
        obstacle.Update(30, 30); // 应该停在 (7, 5)

        mockTime.AdvanceTime(500);
        obstacle.Update(30, 30); // 仍然在 (7, 5)

        // Assert
        obstacle.Position.Should().Be(new Point(7, 5));
    }

    [Fact]
    public void Update_WithPositionOutsideGrid_ShouldNotMove()
    {
        // Arrange
        var mockTime = new MockTimeProvider(1000);
        var position = new Point(5, 5);
        // 路径包含超出网格的点
        var path = new List<Point> { new Point(5, 5), new Point(100, 100) };
        var obstacle = new DynamicObstacle(position, path, mockTime)
        {
            MoveIntervalMs = 500
        };

        // Act
        mockTime.AdvanceTime(500);
        obstacle.Update(30, 30);

        // Assert
        obstacle.Position.Should().Be(new Point(5, 5)); // 应该保持原位置
    }

    [Fact]
    public void Update_WithNegativePosition_ShouldNotMove()
    {
        // Arrange
        var mockTime = new MockTimeProvider(1000);
        var position = new Point(5, 5);
        var path = new List<Point> { new Point(5, 5), new Point(-1, -1) };
        var obstacle = new DynamicObstacle(position, path, mockTime)
        {
            MoveIntervalMs = 500
        };

        // Act
        mockTime.AdvanceTime(500);
        obstacle.Update(30, 30);

        // Assert
        obstacle.Position.Should().Be(new Point(5, 5));
    }

    [Fact]
    public void Interact_ShouldReturnDeadlyResult()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = new DynamicObstacle(position);

        // Act
        var result = obstacle.Interact(null!);

        // Assert
        result.IsDeadly.Should().BeTrue();
        result.CanPassThrough.Should().BeFalse();
    }

    [Fact]
    public void Update_WithEmptyPath_ShouldNotMove()
    {
        // Arrange
        var mockTime = new MockTimeProvider(1000);
        var position = new Point(5, 5);
        var obstacle = new DynamicObstacle(position, new List<Point>(), mockTime)
        {
            MoveIntervalMs = 500
        };

        // Act
        mockTime.AdvanceTime(500);
        obstacle.Update(30, 30);

        // Assert
        obstacle.Position.Should().Be(position);
    }

    [Fact]
    public void ToString_ShouldReturnCorrectFormat()
    {
        // Arrange
        var position = new Point(5, 10);
        var path = new List<Point> { new Point(5, 10), new Point(6, 10) };
        var obstacle = new DynamicObstacle(position, path);

        // Act
        var result = obstacle.ToString();

        // Assert
        result.Should().Contain("DynamicObstacle");
        result.Should().Contain("(5, 10)");
        result.Should().Contain("path index: 0/2");
    }

    [Fact]
    public void Path_Setter_ShouldHandleNull()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = new DynamicObstacle(position, new List<Point> { new Point(5, 5) });

        // Act - 设置为 null，setter 应该处理这种情况
        obstacle.Path = null!;

        // Assert
        obstacle.Path.Should().NotBeNull();
        obstacle.Path.Should().BeEmpty();
    }
}
