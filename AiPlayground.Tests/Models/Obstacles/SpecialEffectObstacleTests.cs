using System.Drawing;
using AiPlayground.Game;
using AiPlayground.Models.Obstacles;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Models.Obstacles;

public class SpecialEffectObstacleTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var obstacle = new SpecialEffectObstacle(position, ObstacleType.SpeedUp);

        // Assert
        obstacle.Position.Should().Be(position);
        obstacle.Type.Should().Be(ObstacleType.SpeedUp);
        obstacle.SpeedChangeAmount.Should().Be(0);
        obstacle.ScoreMultiplierValue.Should().Be(1);
        obstacle.TeleportDestination.Should().BeNull();
        obstacle.IsPermanent.Should().BeFalse();
    }

    [Fact]
    public void CreateSpeedUp_ShouldCreateSpeedUpObstacle()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var obstacle = SpecialEffectObstacle.CreateSpeedUp(position, 3);

        // Assert
        obstacle.Type.Should().Be(ObstacleType.SpeedUp);
        obstacle.SpeedChangeAmount.Should().Be(3);
    }

    [Fact]
    public void CreateSpeedUp_DefaultAmount_ShouldUse2()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var obstacle = SpecialEffectObstacle.CreateSpeedUp(position);

        // Assert
        obstacle.SpeedChangeAmount.Should().Be(2);
    }

    [Fact]
    public void CreateSpeedDown_ShouldCreateSpeedDownObstacle()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var obstacle = SpecialEffectObstacle.CreateSpeedDown(position, -3);

        // Assert
        obstacle.Type.Should().Be(ObstacleType.SpeedDown);
        obstacle.SpeedChangeAmount.Should().Be(-3);
    }

    [Fact]
    public void CreateSpeedDown_DefaultAmount_ShouldUseNegative2()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var obstacle = SpecialEffectObstacle.CreateSpeedDown(position);

        // Assert
        obstacle.SpeedChangeAmount.Should().Be(-2);
    }

    [Fact]
    public void CreateScoreMultiplier_ShouldCreateMultiplierObstacle()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var obstacle = SpecialEffectObstacle.CreateScoreMultiplier(position, 3);

        // Assert
        obstacle.Type.Should().Be(ObstacleType.ScoreMultiplier);
        obstacle.ScoreMultiplierValue.Should().Be(3);
    }

    [Fact]
    public void CreateScoreMultiplier_DefaultMultiplier_ShouldUse2()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var obstacle = SpecialEffectObstacle.CreateScoreMultiplier(position);

        // Assert
        obstacle.ScoreMultiplierValue.Should().Be(2);
    }

    [Fact]
    public void CreateTeleport_ShouldCreateTeleportObstacle()
    {
        // Arrange
        var position = new Point(5, 5);
        var destination = new Point(10, 10);

        // Act
        var obstacle = SpecialEffectObstacle.CreateTeleport(position, destination);

        // Assert
        obstacle.Type.Should().Be(ObstacleType.Teleport);
        obstacle.TeleportDestination.Should().Be(destination);
    }

    [Fact]
    public void Interact_SpeedUp_ShouldReturnSpeedBoostResult()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = SpecialEffectObstacle.CreateSpeedUp(position, 3);
        var state = new GameState();

        // Act
        var result = obstacle.Interact(state);

        // Assert
        result.CanPassThrough.Should().BeTrue();
        result.SpeedChange.Should().Be(3);
        result.Message.Should().Be("加速!");
    }

    [Fact]
    public void Interact_SpeedUp_ZeroAmount_ShouldUseDefault()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = new SpecialEffectObstacle(position, ObstacleType.SpeedUp)
        {
            SpeedChangeAmount = 0
        };
        var state = new GameState();

        // Act
        var result = obstacle.Interact(state);

        // Assert
        result.SpeedChange.Should().Be(2); // 默认值
    }

    [Fact]
    public void Interact_SpeedDown_ShouldReturnSpeedBoostResult()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = SpecialEffectObstacle.CreateSpeedDown(position, -2);
        var state = new GameState();

        // Act
        var result = obstacle.Interact(state);

        // Assert
        result.CanPassThrough.Should().BeTrue();
        result.SpeedChange.Should().Be(-2);
        result.Message.Should().Be("减速!");
    }

    [Fact]
    public void Interact_SpeedDown_ZeroAmount_ShouldUseDefault()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = new SpecialEffectObstacle(position, ObstacleType.SpeedDown)
        {
            SpeedChangeAmount = 0
        };
        var state = new GameState();

        // Act
        var result = obstacle.Interact(state);

        // Assert
        result.SpeedChange.Should().Be(-2); // 默认值
    }

    [Fact]
    public void Interact_ScoreMultiplier_ShouldReturnMultiplyScoreResult()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = SpecialEffectObstacle.CreateScoreMultiplier(position, 3);
        var state = new GameState();

        // Act
        var result = obstacle.Interact(state);

        // Assert
        result.CanPassThrough.Should().BeTrue();
        result.ScoreMultiplier.Should().Be(3);
        result.Message.Should().Be("3x 分数!");
    }

    [Fact]
    public void Interact_Teleport_WithDestination_ShouldReturnTeleportResult()
    {
        // Arrange
        var position = new Point(5, 5);
        var destination = new Point(10, 10);
        var obstacle = SpecialEffectObstacle.CreateTeleport(position, destination);
        var state = new GameState();

        // Act
        var result = obstacle.Interact(state);

        // Assert
        result.CanPassThrough.Should().BeTrue();
        result.TeleportTarget.Should().Be(destination);
        result.Message.Should().Be("传送!");
    }

    [Fact]
    public void Interact_Teleport_WithoutDestination_ShouldReturnPassThrough()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = new SpecialEffectObstacle(position, ObstacleType.Teleport)
        {
            TeleportDestination = null
        };
        var state = new GameState();

        // Act
        var result = obstacle.Interact(state);

        // Assert
        result.CanPassThrough.Should().BeTrue();
        result.TeleportTarget.Should().BeNull();
    }

    [Fact]
    public void Interact_UnknownType_ShouldReturnPassThrough()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = new SpecialEffectObstacle(position, ObstacleType.Static);
        var state = new GameState();

        // Act
        var result = obstacle.Interact(state);

        // Assert
        result.CanPassThrough.Should().BeTrue();
        result.IsDeadly.Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectFormat()
    {
        // Arrange
        var position = new Point(5, 10);
        var obstacle = SpecialEffectObstacle.CreateSpeedUp(position);

        // Act
        var result = obstacle.ToString();

        // Assert
        result.Should().Contain("SpecialEffectObstacle");
        result.Should().Contain("SpeedUp");
        result.Should().Contain("(5, 10)");
    }

    [Fact]
    public void IsPermanent_CanBeSetToTrue()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = SpecialEffectObstacle.CreateSpeedUp(position);

        // Act
        obstacle.IsPermanent = true;

        // Assert
        obstacle.IsPermanent.Should().BeTrue();
    }

    [Fact]
    public void ScoreMultiplierValue_CanBeCustomized()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = SpecialEffectObstacle.CreateScoreMultiplier(position, 2);

        // Act
        obstacle.ScoreMultiplierValue = 5;

        // Assert
        obstacle.ScoreMultiplierValue.Should().Be(5);

        // Interact 应该使用新值
        var state = new GameState();
        var result = obstacle.Interact(state);
        result.ScoreMultiplier.Should().Be(5);
    }

    [Fact]
    public void TeleportDestination_CanBeChanged()
    {
        // Arrange
        var position = new Point(5, 5);
        var destination1 = new Point(10, 10);
        var destination2 = new Point(15, 15);
        var obstacle = SpecialEffectObstacle.CreateTeleport(position, destination1);

        // Act
        obstacle.TeleportDestination = destination2;

        // Assert
        obstacle.TeleportDestination.Should().Be(destination2);

        // Interact 应该使用新值
        var state = new GameState();
        var result = obstacle.Interact(state);
        result.TeleportTarget.Should().Be(destination2);
    }

    [Fact]
    public void SpeedChangeAmount_CanBeCustomized()
    {
        // Arrange
        var position = new Point(5, 5);
        var obstacle = SpecialEffectObstacle.CreateSpeedUp(position, 2);

        // Act
        obstacle.SpeedChangeAmount = 5;

        // Assert
        obstacle.SpeedChangeAmount.Should().Be(5);

        // Interact 应该使用新值
        var state = new GameState();
        var result = obstacle.Interact(state);
        result.SpeedChange.Should().Be(5);
    }
}
