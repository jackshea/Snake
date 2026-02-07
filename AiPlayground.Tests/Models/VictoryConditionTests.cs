using AiPlayground.Game;
using AiPlayground.Models;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Models;

public class VictoryConditionTests
{
    [Fact]
    public void DefaultConstructor_ShouldSetDefaults()
    {
        // Act
        var condition = new VictoryCondition();

        // Assert
        condition.Type.Should().Be(VictoryConditionType.TargetScore);
        condition.TargetScore.Should().Be(100);
        condition.TargetLength.Should().Be(10);
        condition.MustCollectAllFood.Should().BeFalse();
    }

    [Fact]
    public void CheckCondition_TargetScore_WhenScoreMet_ShouldReturnTrue()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.TargetScore,
            TargetScore = 100
        };
        var state = new GameState { Score = 150 };

        // Act
        var result = condition.CheckCondition(state, 0, 0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckCondition_TargetScore_WhenScoreNotMet_ShouldReturnFalse()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.TargetScore,
            TargetScore = 100
        };
        var state = new GameState { Score = 50 };

        // Act
        var result = condition.CheckCondition(state, 0, 0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckCondition_TargetScore_WhenScoreExactlyEqualsTarget_ShouldReturnTrue()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.TargetScore,
            TargetScore = 100
        };
        var state = new GameState { Score = 100 };

        // Act
        var result = condition.CheckCondition(state, 0, 0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckCondition_TargetLength_WhenLengthMet_ShouldReturnTrue()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.TargetLength,
            TargetLength = 10
        };
        var state = new GameState();
        for (int i = 0; i < 15; i++)
        {
            state.Snake.AddLast(new System.Drawing.Point(i, 0));
        }

        // Act
        var result = condition.CheckCondition(state, 0, 0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckCondition_TargetLength_WhenLengthNotMet_ShouldReturnFalse()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.TargetLength,
            TargetLength = 10
        };
        var state = new GameState();
        for (int i = 0; i < 5; i++)
        {
            state.Snake.AddLast(new System.Drawing.Point(i, 0));
        }

        // Act
        var result = condition.CheckCondition(state, 0, 0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckCondition_CollectAllFood_WhenAllCollected_ShouldReturnTrue()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.CollectAllFood,
            MustCollectAllFood = true
        };

        // Act
        var result = condition.CheckCondition(new GameState(), foodCollected: 5, totalFoodSpawned: 5);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckCondition_CollectAllFood_WhenNotAllCollected_ShouldReturnFalse()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.CollectAllFood,
            MustCollectAllFood = true
        };

        // Act
        var result = condition.CheckCondition(new GameState(), foodCollected: 3, totalFoodSpawned: 5);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckCondition_CollectAllFood_WhenNoFoodSpawned_ShouldReturnFalse()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.CollectAllFood,
            MustCollectAllFood = true
        };

        // Act
        var result = condition.CheckCondition(new GameState(), foodCollected: 0, totalFoodSpawned: 0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckCondition_Combined_WhenAllConditionsMet_ShouldReturnTrue()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.Combined,
            TargetScore = 100,
            TargetLength = 10,
            MustCollectAllFood = true
        };
        var state = new GameState { Score = 150 };
        for (int i = 0; i < 12; i++)
        {
            state.Snake.AddLast(new System.Drawing.Point(i, 0));
        }

        // Act
        var result = condition.CheckCondition(state, foodCollected: 5, totalFoodSpawned: 5);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckCondition_Combined_WhenScoreNotMet_ShouldReturnFalse()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.Combined,
            TargetScore = 100,
            TargetLength = 10,
            MustCollectAllFood = false
        };
        var state = new GameState { Score = 50 };
        for (int i = 0; i < 12; i++)
        {
            state.Snake.AddLast(new System.Drawing.Point(i, 0));
        }

        // Act
        var result = condition.CheckCondition(state, foodCollected: 0, totalFoodSpawned: 0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckCondition_Combined_WhenLengthNotMet_ShouldReturnFalse()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.Combined,
            TargetScore = 100,
            TargetLength = 10,
            MustCollectAllFood = false
        };
        var state = new GameState { Score = 150 };
        for (int i = 0; i < 5; i++)
        {
            state.Snake.AddLast(new System.Drawing.Point(i, 0));
        }

        // Act
        var result = condition.CheckCondition(state, foodCollected: 0, totalFoodSpawned: 0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckCondition_Combined_WhenFoodNotAllCollected_ShouldReturnFalse()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.Combined,
            TargetScore = 100,
            TargetLength = 10,
            MustCollectAllFood = true
        };
        var state = new GameState { Score = 150 };
        for (int i = 0; i < 12; i++)
        {
            state.Snake.AddLast(new System.Drawing.Point(i, 0));
        }

        // Act
        var result = condition.CheckCondition(state, foodCollected: 2, totalFoodSpawned: 5);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckCondition_Combined_WithZeroTargets_ShouldIgnoreThoseConditions()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.Combined,
            TargetScore = 0, // Ignored
            TargetLength = 0, // Ignored
            MustCollectAllFood = false
        };
        var state = new GameState { Score = 0 };
        state.Snake.AddLast(new System.Drawing.Point(0, 0));

        // Act
        var result = condition.CheckCondition(state, foodCollected: 0, totalFoodSpawned: 0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetDescription_TargetScore_ShouldReturnCorrectDescription()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.TargetScore,
            TargetScore = 250
        };

        // Act
        var description = condition.GetDescription();

        // Assert
        description.Should().Be("目标分数: 250");
    }

    [Fact]
    public void GetDescription_TargetLength_ShouldReturnCorrectDescription()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.TargetLength,
            TargetLength = 20
        };

        // Act
        var description = condition.GetDescription();

        // Assert
        description.Should().Be("目标长度: 20");
    }

    [Fact]
    public void GetDescription_CollectAllFood_ShouldReturnCorrectDescription()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.CollectAllFood,
            MustCollectAllFood = true
        };

        // Act
        var description = condition.GetDescription();

        // Assert
        description.Should().Be("收集所有食物");
    }

    [Fact]
    public void GetDescription_Combined_WithScoreOnly_ShouldReturnCorrectDescription()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.Combined,
            TargetScore = 150,
            TargetLength = 0,
            MustCollectAllFood = false
        };

        // Act
        var description = condition.GetDescription();

        // Assert
        description.Should().Be("分数 150");
    }

    [Fact]
    public void GetDescription_Combined_WithLengthOnly_ShouldReturnCorrectDescription()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.Combined,
            TargetScore = 0,
            TargetLength = 15,
            MustCollectAllFood = false
        };

        // Act
        var description = condition.GetDescription();

        // Assert
        description.Should().Be("长度 15");
    }

    [Fact]
    public void GetDescription_Combined_WithAllConditions_ShouldReturnCorrectDescription()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.Combined,
            TargetScore = 100,
            TargetLength = 10,
            MustCollectAllFood = true
        };

        // Act
        var description = condition.GetDescription();

        // Assert
        description.Should().Be("分数 100, 长度 10, 收集所有食物");
    }

    [Fact]
    public void GetDescription_Combined_WithNoConditions_ShouldReturnDefaultDescription()
    {
        // Arrange
        var condition = new VictoryCondition
        {
            Type = VictoryConditionType.Combined,
            TargetScore = 0,
            TargetLength = 0,
            MustCollectAllFood = false
        };

        // Act
        var description = condition.GetDescription();

        // Assert
        description.Should().Be("完成目标");
    }

    [Fact]
    public void FoodSpawnCount_ShouldBeSettable()
    {
        // Arrange
        var condition = new VictoryCondition();

        // Act
        condition.FoodSpawnCount = 10;

        // Assert
        condition.FoodSpawnCount.Should().Be(10);
    }

    [Fact]
    public void FoodSpawnCount_WhenNull_ShouldBeNull()
    {
        // Arrange
        var condition = new VictoryCondition();

        // Assert
        condition.FoodSpawnCount.Should().BeNull();
    }
}
