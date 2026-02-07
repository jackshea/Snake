using System.Drawing;
using AiPlayground.Models;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Models;

public class GameConfigTests
{
    [Fact]
    public void DirectionUp_ShouldHaveCorrectValue()
    {
        // Act & Assert
        GameConfig.DirectionUp.Should().Be(new Point(0, -1));
    }

    [Fact]
    public void DirectionDown_ShouldHaveCorrectValue()
    {
        // Act & Assert
        GameConfig.DirectionDown.Should().Be(new Point(0, 1));
    }

    [Fact]
    public void DirectionLeft_ShouldHaveCorrectValue()
    {
        // Act & Assert
        GameConfig.DirectionLeft.Should().Be(new Point(-1, 0));
    }

    [Fact]
    public void DirectionRight_ShouldHaveCorrectValue()
    {
        // Act & Assert
        GameConfig.DirectionRight.Should().Be(new Point(1, 0));
    }

    [Fact]
    public void DirectionConstants_ShouldBeDifferent()
    {
        // Act & Assert
        GameConfig.DirectionUp.Should().NotBe(GameConfig.DirectionDown);
        GameConfig.DirectionUp.Should().NotBe(GameConfig.DirectionLeft);
        GameConfig.DirectionUp.Should().NotBe(GameConfig.DirectionRight);
        GameConfig.DirectionDown.Should().NotBe(GameConfig.DirectionLeft);
        GameConfig.DirectionDown.Should().NotBe(GameConfig.DirectionRight);
        GameConfig.DirectionLeft.Should().NotBe(GameConfig.DirectionRight);
    }

    [Fact]
    public void DirectionUp_AddedToDirectionDown_ShouldBeZero()
    {
        // Act
        var sum = new Point(
            GameConfig.DirectionUp.X + GameConfig.DirectionDown.X,
            GameConfig.DirectionUp.Y + GameConfig.DirectionDown.Y
        );

        // Assert - Opposite directions should cancel out
        sum.Should().Be(new Point(0, 0));
    }

    [Fact]
    public void DirectionLeft_AddedToDirectionRight_ShouldBeZero()
    {
        // Act
        var sum = new Point(
            GameConfig.DirectionLeft.X + GameConfig.DirectionRight.X,
            GameConfig.DirectionLeft.Y + GameConfig.DirectionRight.Y
        );

        // Assert - Opposite directions should cancel out
        sum.Should().Be(new Point(0, 0));
    }

    [Fact]
    public void GridSize_ShouldBe20()
    {
        // Act & Assert
        GameConfig.GridSize.Should().Be(20);
    }

    [Fact]
    public void CellSize_ShouldBe20()
    {
        // Act & Assert
        GameConfig.CellSize.Should().Be(20);
    }

    [Fact]
    public void InfoPanelWidth_ShouldBe200()
    {
        // Act & Assert
        GameConfig.InfoPanelWidth.Should().Be(200);
    }

    [Fact]
    public void MinSpeed_ShouldBe1()
    {
        // Act & Assert
        GameConfig.MinSpeed.Should().Be(1);
    }

    [Fact]
    public void MaxSpeed_ShouldBe10()
    {
        // Act & Assert
        GameConfig.MaxSpeed.Should().Be(10);
    }

    [Fact]
    public void EasyFoodCount_ShouldBe3()
    {
        // Act & Assert
        GameConfig.EasyFoodCount.Should().Be(3);
    }

    [Fact]
    public void NormalFoodCount_ShouldBe1()
    {
        // Act & Assert
        GameConfig.NormalFoodCount.Should().Be(1);
    }

    [Fact]
    public void EasyBasePoints_ShouldBe5()
    {
        // Act & Assert
        GameConfig.EasyBasePoints.Should().Be(5);
    }

    [Fact]
    public void MediumBasePoints_ShouldBe10()
    {
        // Act & Assert
        GameConfig.MediumBasePoints.Should().Be(10);
    }

    [Fact]
    public void HardBasePoints_ShouldBe20()
    {
        // Act & Assert
        GameConfig.HardBasePoints.Should().Be(20);
    }

    [Fact]
    public void HardBaseInterval_ShouldBe80()
    {
        // Act & Assert
        GameConfig.HardBaseInterval.Should().Be(80);
    }

    [Fact]
    public void NormalBaseInterval_ShouldBe150()
    {
        // Act & Assert
        GameConfig.NormalBaseInterval.Should().Be(150);
    }

    [Fact]
    public void SpeedIntervalReduction_ShouldBe15()
    {
        // Act & Assert
        GameConfig.SpeedIntervalReduction.Should().Be(15);
    }

    [Fact]
    public void MinInterval_ShouldBe40()
    {
        // Act & Assert
        GameConfig.MinInterval.Should().Be(40);
    }

    [Fact]
    public void BasePoints_ShouldIncreaseWithDifficulty()
    {
        // Act & Assert
        GameConfig.EasyBasePoints.Should().BeLessThan(GameConfig.MediumBasePoints);
        GameConfig.MediumBasePoints.Should().BeLessThan(GameConfig.HardBasePoints);
    }

    [Fact]
    public void BaseIntervals_HardShouldBeFasterThanNormal()
    {
        // Act & Assert
        GameConfig.HardBaseInterval.Should().BeLessThan(GameConfig.NormalBaseInterval);
    }

    [Fact]
    public void MinInterval_ShouldBeLessThanHardBaseInterval()
    {
        // Act & Assert
        GameConfig.MinInterval.Should().BeLessThan(GameConfig.HardBaseInterval);
    }
}
