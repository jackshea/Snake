using System.Drawing;
using AiPlayground.Game;
using AiPlayground.Models.Obstacles;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Models.Obstacles;

public class ObstacleTests
{
    public class StaticObstacleTests
    {
        [Fact]
        public void Constructor_ShouldSetPositionAndType()
        {
            // Arrange
            var position = new Point(5, 10);

            // Act
            var obstacle = new StaticObstacle(position);

            // Assert
            obstacle.Position.Should().Be(position);
            obstacle.Type.Should().Be(ObstacleType.Static);
        }

        [Fact]
        public void Interact_ShouldReturnDeadlyResult()
        {
            // Arrange
            var obstacle = new StaticObstacle(new Point(5, 10));
            var state = new GameState();

            // Act
            var result = obstacle.Interact(state);

            // Assert
            result.IsDeadly.Should().BeTrue();
            result.ShouldRemove.Should().BeFalse();
            result.CanPassThrough.Should().BeFalse();
        }

        [Fact]
        public void ToString_ShouldReturnCorrectFormat()
        {
            // Arrange
            var obstacle = new StaticObstacle(new Point(5, 10));

            // Act
            var result = obstacle.ToString();

            // Assert
            result.Should().Be("StaticObstacle at (5, 10)");
        }
    }

    public class DestructibleObstacleTests
    {
        [Fact]
        public void Constructor_WithDefaultMaxPasses_ShouldInitializeCorrectly()
        {
            // Arrange
            var position = new Point(5, 10);

            // Act
            var obstacle = new DestructibleObstacle(position);

            // Assert
            obstacle.Position.Should().Be(position);
            obstacle.Type.Should().Be(ObstacleType.Destructible);
            obstacle.MaxPasses.Should().Be(1);
            obstacle.RemainingPasses.Should().Be(1);
        }

        [Fact]
        public void Constructor_WithCustomMaxPasses_ShouldInitializeCorrectly()
        {
            // Arrange
            var position = new Point(5, 10);

            // Act
            var obstacle = new DestructibleObstacle(position, 3);

            // Assert
            obstacle.MaxPasses.Should().Be(3);
            obstacle.RemainingPasses.Should().Be(3);
        }

        [Fact]
        public void Interact_FirstPass_ShouldReturnPassThrough()
        {
            // Arrange
            var obstacle = new DestructibleObstacle(new Point(5, 10), 2);
            var state = new GameState();

            // Act
            var result = obstacle.Interact(state);

            // Assert
            result.IsDeadly.Should().BeFalse();
            result.CanPassThrough.Should().BeTrue();
            result.ShouldRemove.Should().BeFalse();
            obstacle.RemainingPasses.Should().Be(1);
        }

        [Fact]
        public void Interact_LastPass_ShouldReturnRemoveAfterPass()
        {
            // Arrange
            var obstacle = new DestructibleObstacle(new Point(5, 10), 1);
            var state = new GameState();

            // Act
            var result = obstacle.Interact(state);

            // Assert
            result.IsDeadly.Should().BeFalse();
            result.CanPassThrough.Should().BeTrue();
            result.ShouldRemove.Should().BeTrue();
            obstacle.RemainingPasses.Should().Be(0);
        }

        [Fact]
        public void Interact_MultiplePasses_ShouldDecreaseRemainingPasses()
        {
            // Arrange
            var obstacle = new DestructibleObstacle(new Point(5, 10), 3);
            var state = new GameState();

            // Act
            obstacle.Interact(state);
            obstacle.Interact(state);
            var result = obstacle.Interact(state);

            // Assert
            obstacle.RemainingPasses.Should().Be(0);
            result.ShouldRemove.Should().BeTrue();
        }

        [Fact]
        public void RemainingPasses_Setter_ShouldNotAllowNegative()
        {
            // Arrange
            var obstacle = new DestructibleObstacle(new Point(5, 10), 2);

            // Act
            obstacle.RemainingPasses = -1;

            // Assert
            obstacle.RemainingPasses.Should().Be(0);
        }

        [Fact]
        public void ToString_ShouldReturnCorrectFormat()
        {
            // Arrange
            var obstacle = new DestructibleObstacle(new Point(5, 10), 3);

            // Act
            var result = obstacle.ToString();

            // Assert
            result.Should().Be("DestructibleObstacle at (5, 10), remaining: 3/3");
        }

        [Fact]
        public void ToString_AfterPasses_ShouldShowUpdatedRemaining()
        {
            // Arrange
            var obstacle = new DestructibleObstacle(new Point(5, 10), 3);
            var state = new GameState();

            // Act
            obstacle.Interact(state);
            var result = obstacle.ToString();

            // Assert
            result.Should().Be("DestructibleObstacle at (5, 10), remaining: 2/3");
        }
    }

    public class DynamicObstacleTests
    {
        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange
            var position = new Point(5, 10);
            var path = new List<Point> { new Point(0, 0), new Point(5, 10), new Point(10, 10) };

            // Act
            var obstacle = new DynamicObstacle(position, path);

            // Assert
            obstacle.Position.Should().Be(position);
            obstacle.Type.Should().Be(ObstacleType.Dynamic);
            obstacle.Path.Should().BeEquivalentTo(path);
            obstacle.MoveIntervalMs.Should().Be(500);
            obstacle.LoopPath.Should().BeTrue();
        }

        [Fact]
        public void Constructor_WithNullPath_ShouldUseDefaultPath()
        {
            // Arrange
            var position = new Point(5, 10);

            // Act
            var obstacle = new DynamicObstacle(position, null);

            // Assert
            obstacle.Path.Should().HaveCount(1);
            obstacle.Path[0].Should().Be(position);
        }

        [Fact]
        public void Interact_ShouldReturnDeadlyResult()
        {
            // Arrange
            var obstacle = new DynamicObstacle(new Point(5, 10));
            var state = new GameState();

            // Act
            var result = obstacle.Interact(state);

            // Assert
            result.IsDeadly.Should().BeTrue();
        }

        [Fact]
        public void Update_WithEmptyPath_ShouldNotMove()
        {
            // Arrange
            var obstacle = new DynamicObstacle(new Point(5, 10), new List<Point>());
            var originalPosition = obstacle.Position;

            // Act
            obstacle.Update(30, 30);

            // Assert
            obstacle.Position.Should().Be(originalPosition);
        }

        [Fact]
        public void Update_BeforeInterval_ShouldNotMove()
        {
            // Arrange
            var obstacle = new DynamicObstacle(new Point(5, 10), new List<Point>
            {
                new Point(5, 10),
                new Point(6, 10)
            });

            // Act
            obstacle.Update(30, 30);

            // Assert
            obstacle.Position.Should().Be(new Point(5, 10));
        }

        [Fact]
        public void ToString_ShouldReturnCorrectFormat()
        {
            // Arrange
            var path = new List<Point> { new Point(0, 0), new Point(5, 10) };
            var obstacle = new DynamicObstacle(new Point(5, 10), path);

            // Act
            var result = obstacle.ToString();

            // Assert
            result.Should().Be("DynamicObstacle at (5, 10), path index: 0/2");
        }
    }

    public class SpecialEffectObstacleTests
    {
        [Fact]
        public void CreateSpeedUp_ShouldCreateCorrectObstacle()
        {
            // Arrange
            var position = new Point(5, 10);

            // Act
            var obstacle = SpecialEffectObstacle.CreateSpeedUp(position, 3);

            // Assert
            obstacle.Position.Should().Be(position);
            obstacle.Type.Should().Be(ObstacleType.SpeedUp);
            obstacle.SpeedChangeAmount.Should().Be(3);
        }

        [Fact]
        public void CreateSpeedUp_DefaultAmount_ShouldUse2()
        {
            // Arrange
            var position = new Point(5, 10);

            // Act
            var obstacle = SpecialEffectObstacle.CreateSpeedUp(position);

            // Assert
            obstacle.SpeedChangeAmount.Should().Be(2);
        }

        [Fact]
        public void CreateSpeedDown_ShouldCreateCorrectObstacle()
        {
            // Arrange
            var position = new Point(5, 10);

            // Act
            var obstacle = SpecialEffectObstacle.CreateSpeedDown(position, -3);

            // Assert
            obstacle.Position.Should().Be(position);
            obstacle.Type.Should().Be(ObstacleType.SpeedDown);
            obstacle.SpeedChangeAmount.Should().Be(-3);
        }

        [Fact]
        public void CreateSpeedDown_DefaultAmount_ShouldUseNegative2()
        {
            // Arrange
            var position = new Point(5, 10);

            // Act
            var obstacle = SpecialEffectObstacle.CreateSpeedDown(position);

            // Assert
            obstacle.SpeedChangeAmount.Should().Be(-2);
        }

        [Fact]
        public void CreateScoreMultiplier_ShouldCreateCorrectObstacle()
        {
            // Arrange
            var position = new Point(5, 10);

            // Act
            var obstacle = SpecialEffectObstacle.CreateScoreMultiplier(position, 3);

            // Assert
            obstacle.Position.Should().Be(position);
            obstacle.Type.Should().Be(ObstacleType.ScoreMultiplier);
            obstacle.ScoreMultiplierValue.Should().Be(3);
        }

        [Fact]
        public void CreateScoreMultiplier_DefaultMultiplier_ShouldUse2()
        {
            // Arrange
            var position = new Point(5, 10);

            // Act
            var obstacle = SpecialEffectObstacle.CreateScoreMultiplier(position);

            // Assert
            obstacle.ScoreMultiplierValue.Should().Be(2);
        }

        [Fact]
        public void CreateTeleport_ShouldCreateCorrectObstacle()
        {
            // Arrange
            var position = new Point(5, 10);
            var destination = new Point(15, 20);

            // Act
            var obstacle = SpecialEffectObstacle.CreateTeleport(position, destination);

            // Assert
            obstacle.Position.Should().Be(position);
            obstacle.Type.Should().Be(ObstacleType.Teleport);
            obstacle.TeleportDestination.Should().Be(destination);
        }

        [Fact]
        public void Interact_SpeedUp_ShouldReturnSpeedBoostResult()
        {
            // Arrange
            var obstacle = SpecialEffectObstacle.CreateSpeedUp(new Point(5, 10), 2);
            var state = new GameState();

            // Act
            var result = obstacle.Interact(state);

            // Assert
            result.IsDeadly.Should().BeFalse();
            result.CanPassThrough.Should().BeTrue();
            result.SpeedChange.Should().Be(2);
            result.Message.Should().Be("加速!");
        }

        [Fact]
        public void Interact_SpeedDown_ShouldReturnSpeedBoostResult()
        {
            // Arrange
            var obstacle = SpecialEffectObstacle.CreateSpeedDown(new Point(5, 10), -2);
            var state = new GameState();

            // Act
            var result = obstacle.Interact(state);

            // Assert
            result.IsDeadly.Should().BeFalse();
            result.CanPassThrough.Should().BeTrue();
            result.SpeedChange.Should().Be(-2);
            result.Message.Should().Be("减速!");
        }

        [Fact]
        public void Interact_ScoreMultiplier_ShouldReturnMultiplyScoreResult()
        {
            // Arrange
            var obstacle = SpecialEffectObstacle.CreateScoreMultiplier(new Point(5, 10), 3);
            var state = new GameState();

            // Act
            var result = obstacle.Interact(state);

            // Assert
            result.IsDeadly.Should().BeFalse();
            result.CanPassThrough.Should().BeTrue();
            result.ScoreMultiplier.Should().Be(3);
            result.Message.Should().Be("3x 分数!");
        }

        [Fact]
        public void Interact_Teleport_ShouldReturnTeleportResult()
        {
            // Arrange
            var destination = new Point(15, 20);
            var obstacle = SpecialEffectObstacle.CreateTeleport(new Point(5, 10), destination);
            var state = new GameState();

            // Act
            var result = obstacle.Interact(state);

            // Assert
            result.IsDeadly.Should().BeFalse();
            result.CanPassThrough.Should().BeTrue();
            result.TeleportTarget.Should().Be(destination);
            result.Message.Should().Be("传送!");
        }

        [Fact]
        public void ToString_ShouldReturnCorrectFormat()
        {
            // Arrange
            var obstacle = SpecialEffectObstacle.CreateSpeedUp(new Point(5, 10));

            // Act
            var result = obstacle.ToString();

            // Assert
            result.Should().Be("SpecialEffectObstacle (SpeedUp) at (5, 10)");
        }

        [Fact]
        public void Constructor_ShouldInitializeDefaults()
        {
            // Arrange & Act
            var obstacle = new SpecialEffectObstacle(new Point(5, 10), ObstacleType.SpeedUp);

            // Assert
            obstacle.SpeedChangeAmount.Should().Be(0);
            obstacle.ScoreMultiplierValue.Should().Be(1);
            obstacle.TeleportDestination.Should().BeNull();
        }
    }

    public class ObstacleInteractionResultTests
    {
        [Fact]
        public void Deadly_ShouldCreateDeadlyResult()
        {
            // Act
            var result = ObstacleInteractionResult.Deadly();

            // Assert
            result.IsDeadly.Should().BeTrue();
            result.ShouldRemove.Should().BeFalse();
            result.CanPassThrough.Should().BeFalse();
        }

        [Fact]
        public void PassThrough_WithDefaultPasses_ShouldCreateCorrectResult()
        {
            // Act
            var result = ObstacleInteractionResult.PassThrough();

            // Assert
            result.IsDeadly.Should().BeFalse();
            result.CanPassThrough.Should().BeTrue();
            result.ShouldRemove.Should().BeFalse();
        }

        [Fact]
        public void RemoveAfterPass_ShouldCreateCorrectResult()
        {
            // Act
            var result = ObstacleInteractionResult.RemoveAfterPass();

            // Assert
            result.IsDeadly.Should().BeFalse();
            result.CanPassThrough.Should().BeTrue();
            result.ShouldRemove.Should().BeTrue();
        }

        [Fact]
        public void SpeedBoost_WithPositiveAmount_ShouldCreateCorrectResult()
        {
            // Act
            var result = ObstacleInteractionResult.SpeedBoost(3);

            // Assert
            result.IsDeadly.Should().BeFalse();
            result.CanPassThrough.Should().BeTrue();
            result.SpeedChange.Should().Be(3);
            result.Message.Should().Be("加速!");
        }

        [Fact]
        public void SpeedBoost_WithNegativeAmount_ShouldCreateCorrectResult()
        {
            // Act
            var result = ObstacleInteractionResult.SpeedBoost(-2);

            // Assert
            result.SpeedChange.Should().Be(-2);
            result.Message.Should().Be("减速!");
        }

        [Fact]
        public void MultiplyScore_ShouldCreateCorrectResult()
        {
            // Act
            var result = ObstacleInteractionResult.MultiplyScore(3);

            // Assert
            result.IsDeadly.Should().BeFalse();
            result.CanPassThrough.Should().BeTrue();
            result.ScoreMultiplier.Should().Be(3);
            result.Message.Should().Be("3x 分数!");
        }

        [Fact]
        public void Teleport_ShouldCreateCorrectResult()
        {
            // Arrange
            var target = new Point(15, 20);

            // Act
            var result = ObstacleInteractionResult.Teleport(target);

            // Assert
            result.IsDeadly.Should().BeFalse();
            result.CanPassThrough.Should().BeTrue();
            result.TeleportTarget.Should().Be(target);
            result.Message.Should().Be("传送!");
        }
    }
}
