using AiPlayground.Models;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Models;

public class VictoryConditionTypeTests
{
    [Fact]
    public void TargetScore_ShouldHaveValueZero()
    {
        // Act & Assert
        ((int)VictoryConditionType.TargetScore).Should().Be(0);
    }

    [Fact]
    public void TargetLength_ShouldHaveValueOne()
    {
        // Act & Assert
        ((int)VictoryConditionType.TargetLength).Should().Be(1);
    }

    [Fact]
    public void CollectAllFood_ShouldHaveValueTwo()
    {
        // Act & Assert
        ((int)VictoryConditionType.CollectAllFood).Should().Be(2);
    }

    [Fact]
    public void Combined_ShouldHaveValueThree()
    {
        // Act & Assert
        ((int)VictoryConditionType.Combined).Should().Be(3);
    }

    [Fact]
    public void Values_ShouldBeSequential()
    {
        // Act & Assert
        ((int)VictoryConditionType.TargetScore).Should().BeLessThan((int)VictoryConditionType.TargetLength);
        ((int)VictoryConditionType.TargetLength).Should().BeLessThan((int)VictoryConditionType.CollectAllFood);
        ((int)VictoryConditionType.CollectAllFood).Should().BeLessThan((int)VictoryConditionType.Combined);
    }

    [Fact]
    public void Enum_ShouldHaveExactlyFourValues()
    {
        // Act
        var values = Enum.GetValues<VictoryConditionType>();

        // Assert
        values.Should().HaveCount(4);
    }

    [Fact]
    public void Enum_ShouldContainAllExpectedValues()
    {
        // Act
        var values = Enum.GetValues<VictoryConditionType>();

        // Assert
        values.Should().Contain(VictoryConditionType.TargetScore);
        values.Should().Contain(VictoryConditionType.TargetLength);
        values.Should().Contain(VictoryConditionType.CollectAllFood);
        values.Should().Contain(VictoryConditionType.Combined);
    }

    [Fact]
    public void Parse_ValidString_ShouldReturnCorrectValue()
    {
        // Act & Assert
        Enum.Parse<VictoryConditionType>("TargetScore").Should().Be(VictoryConditionType.TargetScore);
        Enum.Parse<VictoryConditionType>("TargetLength").Should().Be(VictoryConditionType.TargetLength);
        Enum.Parse<VictoryConditionType>("CollectAllFood").Should().Be(VictoryConditionType.CollectAllFood);
        Enum.Parse<VictoryConditionType>("Combined").Should().Be(VictoryConditionType.Combined);
    }

    [Fact]
    public void GetName_ShouldReturnCorrectString()
    {
        // Act & Assert
        Enum.GetName(typeof(VictoryConditionType), VictoryConditionType.TargetScore).Should().Be("TargetScore");
        Enum.GetName(typeof(VictoryConditionType), VictoryConditionType.TargetLength).Should().Be("TargetLength");
        Enum.GetName(typeof(VictoryConditionType), VictoryConditionType.CollectAllFood).Should().Be("CollectAllFood");
        Enum.GetName(typeof(VictoryConditionType), VictoryConditionType.Combined).Should().Be("Combined");
    }

    [Fact]
    public void IsDefined_ForAllValues_ShouldReturnTrue()
    {
        // Act & Assert
        Enum.IsDefined(typeof(VictoryConditionType), VictoryConditionType.TargetScore).Should().BeTrue();
        Enum.IsDefined(typeof(VictoryConditionType), VictoryConditionType.TargetLength).Should().BeTrue();
        Enum.IsDefined(typeof(VictoryConditionType), VictoryConditionType.CollectAllFood).Should().BeTrue();
        Enum.IsDefined(typeof(VictoryConditionType), VictoryConditionType.Combined).Should().BeTrue();
    }

    [Fact]
    public void ComparisonOperators_ShouldWork()
    {
        // Act & Assert
        (VictoryConditionType.TargetScore < VictoryConditionType.TargetLength).Should().BeTrue();
        (VictoryConditionType.Combined > VictoryConditionType.CollectAllFood).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_ShouldWork()
    {
        // Arrange
        var type1 = VictoryConditionType.TargetScore;
        var type2 = VictoryConditionType.TargetScore;
        var type3 = VictoryConditionType.Combined;

        // Act & Assert
        (type1 == type2).Should().BeTrue();
        (type1 == type3).Should().BeFalse();
    }

    [Fact]
    public void CanBeUsedInSwitchStatement()
    {
        // Arrange
        var type = VictoryConditionType.Combined;
        string result;

        // Act
        switch (type)
        {
            case VictoryConditionType.TargetScore:
                result = "Score";
                break;
            case VictoryConditionType.TargetLength:
                result = "Length";
                break;
            case VictoryConditionType.CollectAllFood:
                result = "Food";
                break;
            case VictoryConditionType.Combined:
                result = "Combined";
                break;
            default:
                result = "Unknown";
                break;
        }

        // Assert
        result.Should().Be("Combined");
    }
}
