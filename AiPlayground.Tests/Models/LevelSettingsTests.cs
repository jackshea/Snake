using System.Drawing;
using AiPlayground.Models;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Models;

public class LevelSettingsTests
{
    [Fact]
    public void DefaultConstructor_ShouldSetDefaultDifficultyToEasy()
    {
        // Act
        var settings = new LevelSettings();

        // Assert
        settings.DefaultDifficulty.Should().Be(Difficulty.Easy);
    }

    [Fact]
    public void DefaultConstructor_ShouldSetInitialSpeedLevelTo5()
    {
        // Act
        var settings = new LevelSettings();

        // Assert
        settings.InitialSpeedLevel.Should().Be(5);
    }

    [Fact]
    public void DefaultConstructor_ShouldSetGridWidthTo30()
    {
        // Act
        var settings = new LevelSettings();

        // Assert
        settings.GridWidth.Should().Be(30);
    }

    [Fact]
    public void DefaultConstructor_ShouldSetGridHeightTo20()
    {
        // Act
        var settings = new LevelSettings();

        // Assert
        settings.GridHeight.Should().Be(20);
    }

    [Fact]
    public void DefaultConstructor_ShouldSetSnakeStartPosition()
    {
        // Act
        var settings = new LevelSettings();

        // Assert
        settings.SnakeStartPosition.Should().Be(new Point(5, 5));
    }

    [Fact]
    public void DefaultConstructor_ShouldSetInitialDirectionToRight()
    {
        // Act
        var settings = new LevelSettings();

        // Assert
        settings.InitialDirection.Should().Be(new Point(1, 0));
    }

    [Fact]
    public void DefaultConstructor_ShouldSetInitialSnakeLengthTo3()
    {
        // Act
        var settings = new LevelSettings();

        // Assert
        settings.InitialSnakeLength.Should().Be(3);
    }

    [Fact]
    public void DefaultConstructor_ShouldSetFoodCountTo1()
    {
        // Act
        var settings = new LevelSettings();

        // Assert
        settings.FoodCount.Should().Be(1);
    }

    [Fact]
    public void DefaultConstructor_ShouldDisableDynamicObstacles()
    {
        // Act
        var settings = new LevelSettings();

        // Assert
        settings.EnableDynamicObstacles.Should().BeFalse();
    }

    [Fact]
    public void DefaultDifficulty_CanBeModified()
    {
        // Arrange
        var settings = new LevelSettings();

        // Act
        settings.DefaultDifficulty = Difficulty.Hard;

        // Assert
        settings.DefaultDifficulty.Should().Be(Difficulty.Hard);
    }

    [Fact]
    public void InitialSpeedLevel_CanBeModified()
    {
        // Arrange
        var settings = new LevelSettings();

        // Act
        settings.InitialSpeedLevel = 8;

        // Assert
        settings.InitialSpeedLevel.Should().Be(8);
    }

    [Fact]
    public void GridDimensions_CanBeModified()
    {
        // Arrange
        var settings = new LevelSettings();

        // Act
        settings.GridWidth = 40;
        settings.GridHeight = 30;

        // Assert
        settings.GridWidth.Should().Be(40);
        settings.GridHeight.Should().Be(30);
    }

    [Fact]
    public void SnakeStartPosition_CanBeModified()
    {
        // Arrange
        var settings = new LevelSettings();
        var newPosition = new Point(10, 15);

        // Act
        settings.SnakeStartPosition = newPosition;

        // Assert
        settings.SnakeStartPosition.Should().Be(newPosition);
    }

    [Fact]
    public void InitialDirection_CanBeModified()
    {
        // Arrange
        var settings = new LevelSettings();
        var newDirection = new Point(0, -1);

        // Act
        settings.InitialDirection = newDirection;

        // Assert
        settings.InitialDirection.Should().Be(newDirection);
    }

    [Fact]
    public void InitialSnakeLength_CanBeModified()
    {
        // Arrange
        var settings = new LevelSettings();

        // Act
        settings.InitialSnakeLength = 5;

        // Assert
        settings.InitialSnakeLength.Should().Be(5);
    }

    [Fact]
    public void FoodCount_CanBeModified()
    {
        // Arrange
        var settings = new LevelSettings();

        // Act
        settings.FoodCount = 5;

        // Assert
        settings.FoodCount.Should().Be(5);
    }

    [Fact]
    public void EnableDynamicObstacles_CanBeModified()
    {
        // Arrange
        var settings = new LevelSettings();

        // Act
        settings.EnableDynamicObstacles = true;

        // Assert
        settings.EnableDynamicObstacles.Should().BeTrue();
    }
}
