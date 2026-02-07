using AiPlayground.Models;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Models;

public class DifficultyTests
{
    [Fact]
    public void Easy_ShouldHaveValue1()
    {
        // Act & Assert
        ((int)Difficulty.Easy).Should().Be(1);
    }

    [Fact]
    public void Medium_ShouldHaveValue2()
    {
        // Act & Assert
        ((int)Difficulty.Medium).Should().Be(2);
    }

    [Fact]
    public void Hard_ShouldHaveValue3()
    {
        // Act & Assert
        ((int)Difficulty.Hard).Should().Be(3);
    }

    [Fact]
    public void DifficultyValues_ShouldBeSequential()
    {
        // Act & Assert
        ((int)Difficulty.Easy).Should().BeLessThan((int)Difficulty.Medium);
        ((int)Difficulty.Medium).Should().BeLessThan((int)Difficulty.Hard);
    }

    [Fact]
    public void DifficultyEnum_ShouldHaveExactlyThreeValues()
    {
        // Act & Assert
        var values = Enum.GetValues<Difficulty>();
        values.Should().HaveCount(3);
    }

    [Fact]
    public void DifficultyEnum_ShouldContainAllExpectedValues()
    {
        // Act & Assert
        var values = Enum.GetValues<Difficulty>();
        values.Should().Contain(Difficulty.Easy);
        values.Should().Contain(Difficulty.Medium);
        values.Should().Contain(Difficulty.Hard);
    }

    [Fact]
    public void Parse_ValidString_ShouldReturnCorrectDifficulty()
    {
        // Act & Assert
        Enum.Parse<Difficulty>("Easy").Should().Be(Difficulty.Easy);
        Enum.Parse<Difficulty>("Medium").Should().Be(Difficulty.Medium);
        Enum.Parse<Difficulty>("Hard").Should().Be(Difficulty.Hard);
    }

    [Fact]
    public void IsDefined_ForAllValues_ShouldReturnTrue()
    {
        // Act & Assert
        Enum.IsDefined(typeof(Difficulty), Difficulty.Easy).Should().BeTrue();
        Enum.IsDefined(typeof(Difficulty), Difficulty.Medium).Should().BeTrue();
        Enum.IsDefined(typeof(Difficulty), Difficulty.Hard).Should().BeTrue();
    }

    [Fact]
    public void GetName_ShouldReturnCorrectString()
    {
        // Act & Assert
        Enum.GetName(typeof(Difficulty), Difficulty.Easy).Should().Be("Easy");
        Enum.GetName(typeof(Difficulty), Difficulty.Medium).Should().Be("Medium");
        Enum.GetName(typeof(Difficulty), Difficulty.Hard).Should().Be("Hard");
    }

    [Fact]
    public void Difficulty_CanBeUsedAsSwitchCase()
    {
        // Arrange
        var difficulty = Difficulty.Hard;
        string result;

        // Act
        switch (difficulty)
        {
            case Difficulty.Easy:
                result = "Easy";
                break;
            case Difficulty.Medium:
                result = "Medium";
                break;
            case Difficulty.Hard:
                result = "Hard";
                break;
            default:
                result = "Unknown";
                break;
        }

        // Assert
        result.Should().Be("Hard");
    }

    [Fact]
    public void Difficulty_ComparisonOperators_ShouldWork()
    {
        // Act & Assert
        (Difficulty.Easy < Difficulty.Medium).Should().BeTrue();
        (Difficulty.Medium < Difficulty.Hard).Should().BeTrue();
        (Difficulty.Hard > Difficulty.Medium).Should().BeTrue();
        (Difficulty.Medium > Difficulty.Easy).Should().BeTrue();
    }

    [Fact]
    public void Difficulty_EqualityOperator_ShouldWork()
    {
        // Act & Assert
        var easy = Difficulty.Easy;
        var medium = Difficulty.Medium;
        var hard = Difficulty.Hard;
        (easy == Difficulty.Easy).Should().BeTrue();
        (easy == medium).Should().BeFalse();
        (medium == hard).Should().BeFalse();
    }
}
