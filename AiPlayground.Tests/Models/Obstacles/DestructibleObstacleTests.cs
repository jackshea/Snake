using System.Drawing;
using AiPlayground.Game;
using AiPlayground.Models.Obstacles;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Models.Obstacles;

public class DestructibleObstacleTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var obstacle = new DestructibleObstacle(position);

        // Assert
        obstacle.Position.Should().Be(position);
        obstacle.MaxPasses.Should().Be(1);
        obstacle.RemainingPasses.Should().Be(1);
    }

    [Fact]
    public void Constructor_WithCustomMaxPasses_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var obstacle = new DestructibleObstacle(position, 3);

        // Assert
        obstacle.MaxPasses.Should().Be(3);
        obstacle.RemainingPasses.Should().Be(3);
    }

    [Fact]
    public void Interact_FirstPass_ShouldReturnPassThrough()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = new DestructibleObstacle(position, 2);
        var state = new GameState();

        // Act
        var result = obstacle.Interact(state);

        // Assert
        result.CanPassThrough.Should().BeTrue();
        result.ShouldRemove.Should().BeFalse();
        result.IsDeadly.Should().BeFalse();
        obstacle.RemainingPasses.Should().Be(1);
    }

    [Fact]
    public void Interact_LastPass_ShouldReturnRemoveAfterPass()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = new DestructibleObstacle(position, 1);
        var state = new GameState();

        // Act
        var result = obstacle.Interact(state);

        // Assert
        result.CanPassThrough.Should().BeTrue();
        result.ShouldRemove.Should().BeTrue();
        result.IsDeadly.Should().BeFalse();
        obstacle.RemainingPasses.Should().Be(0);
    }

    [Fact]
    public void Interact_MultiplePasses_ShouldDestructCorrectly()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = new DestructibleObstacle(position, 3);
        var state = new GameState();

        // Act - 第一次通过
        var result1 = obstacle.Interact(state);
        obstacle.RemainingPasses.Should().Be(2);
        result1.ShouldRemove.Should().BeFalse();

        // Act - 第二次通过
        var result2 = obstacle.Interact(state);
        obstacle.RemainingPasses.Should().Be(1);
        result2.ShouldRemove.Should().BeFalse();

        // Act - 第三次通过（最后一次）
        var result3 = obstacle.Interact(state);
        obstacle.RemainingPasses.Should().Be(0);
        result3.ShouldRemove.Should().BeTrue();
    }

    [Fact]
    public void RemainingPasses_Setter_ShouldNotAllowNegative()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = new DestructibleObstacle(position, 3);

        // Act
        obstacle.RemainingPasses = -5;

        // Assert
        obstacle.RemainingPasses.Should().Be(0);
    }

    [Fact]
    public void RemainingPasses_Setter_ShouldAcceptZero()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = new DestructibleObstacle(position, 3);

        // Act
        obstacle.RemainingPasses = 0;

        // Assert
        obstacle.RemainingPasses.Should().Be(0);
    }

    [Fact]
    public void ToString_ShouldReturnCorrectFormat()
    {
        // Arrange
        var position = new Point(5, 10);
        var obstacle = new DestructibleObstacle(position, 3);

        // Act
        var result = obstacle.ToString();

        // Assert
        result.Should().Contain("DestructibleObstacle");
        result.Should().Contain("(5, 10)");
        result.Should().Contain("remaining: 3/3");
    }

    [Fact]
    public void ToString_AfterSomePasses_ShouldShowCorrectRemaining()
    {
        // Arrange
        var position = new Point(5, 10);
        var obstacle = new DestructibleObstacle(position, 3);
        var state = new GameState();

        // Act
        obstacle.Interact(state);
        obstacle.Interact(state);

        var result = obstacle.ToString();

        // Assert
        result.Should().Contain("remaining: 1/3");
    }

    [Fact]
    public void Interact_SinglePassObstacle_ShouldRemoveImmediately()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = new DestructibleObstacle(position, 1);
        var state = new GameState();

        // Act
        var result = obstacle.Interact(state);

        // Assert
        result.CanPassThrough.Should().BeTrue();
        result.ShouldRemove.Should().BeTrue();
        obstacle.RemainingPasses.Should().Be(0);
    }

    [Fact]
    public void MaxPasses_CanBeZero()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var obstacle = new DestructibleObstacle(position, 0);

        // Assert
        obstacle.MaxPasses.Should().Be(0);
        obstacle.RemainingPasses.Should().Be(0);
    }

    [Fact]
    public void ObstacleType_ShouldBeDestructible()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var obstacle = new DestructibleObstacle(position);

        // Assert
        obstacle.Type.Should().Be(ObstacleType.Destructible);
    }
}
