using System.Drawing;
using AiPlayground.Game;
using AiPlayground.Models;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Models;

public class GameStateTests
{
    [Fact]
    public void DefaultConstructor_ShouldSetDefaults()
    {
        // Act
        var state = new GameState();

        // Assert
        state.Snake.Should().NotBeNull();
        state.Snake.Count.Should().Be(0);
        state.Foods.Should().BeEmpty();
        state.Direction.Should().Be(GameConfig.DirectionRight);
        state.Score.Should().Be(0);
        state.IsGameOver.Should().BeFalse();
        state.IsPaused.Should().BeFalse();
        state.IsWaitingToStart.Should().BeTrue();
        state.IsNewHighScore.Should().BeFalse();
        state.Difficulty.Should().Be(Difficulty.Medium);
        state.SpeedLevel.Should().Be(5);
        state.IsLevelCompleted.Should().BeFalse();
        state.LevelTime.Should().Be(0);
        state.FoodCollected.Should().Be(0);
        state.TotalFoodSpawned.Should().Be(0);
    }

    [Fact]
    public void Clone_ShouldCreateDeepCopy()
    {
        // Arrange
        var original = new GameState
        {
            Score = 150,
            Direction = new Point(1, 0),
            IsGameOver = false,
            IsPaused = false,
            IsWaitingToStart = false,
            IsNewHighScore = true,
            Difficulty = Difficulty.Hard,
            SpeedLevel = 8,
            IsLevelCompleted = true,
            LevelTime = 45,
            FoodCollected = 10,
            TotalFoodSpawned = 15
        };
        original.Snake.AddLast(new Point(10, 10));
        original.Snake.AddLast(new Point(9, 10));
        original.Foods.Add(new Point(15, 15));
        original.Foods.Add(new Point(20, 20));

        // Act
        var cloned = original.Clone();

        // Assert
        cloned.Should().NotBeSameAs(original);
        cloned.Score.Should().Be(original.Score);
        cloned.Direction.Should().Be(original.Direction);
        cloned.IsGameOver.Should().Be(original.IsGameOver);
        cloned.IsPaused.Should().Be(original.IsPaused);
        cloned.IsWaitingToStart.Should().Be(original.IsWaitingToStart);
        cloned.IsNewHighScore.Should().Be(original.IsNewHighScore);
        cloned.Difficulty.Should().Be(original.Difficulty);
        cloned.SpeedLevel.Should().Be(original.SpeedLevel);
        cloned.IsLevelCompleted.Should().Be(original.IsLevelCompleted);
        cloned.LevelTime.Should().Be(original.LevelTime);
        cloned.FoodCollected.Should().Be(original.FoodCollected);
        cloned.TotalFoodSpawned.Should().Be(original.TotalFoodSpawned);
    }

    [Fact]
    public void Clone_SnakeShouldBeIndependent()
    {
        // Arrange
        var original = new GameState();
        original.Snake.AddLast(new Point(10, 10));
        original.Snake.AddLast(new Point(9, 10));

        // Act
        var cloned = original.Clone();
        cloned.Snake.AddLast(new Point(8, 10));

        // Assert
        original.Snake.Count.Should().Be(2);
        cloned.Snake.Count.Should().Be(3);
    }

    [Fact]
    public void Clone_FoodsShouldBeIndependent()
    {
        // Arrange
        var original = new GameState();
        original.Foods.Add(new Point(15, 15));

        // Act
        var cloned = original.Clone();
        cloned.Foods.Add(new Point(20, 20));

        // Assert
        original.Foods.Should().HaveCount(1);
        cloned.Foods.Should().HaveCount(2);
    }

    [Fact]
    public void IsWaitingToStart_CanBeModified()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.IsWaitingToStart = false;

        // Assert
        state.IsWaitingToStart.Should().BeFalse();
    }

    [Fact]
    public void IsGameOver_CanBeModified()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.IsGameOver = true;

        // Assert
        state.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void IsPaused_CanBeModified()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.IsPaused = true;

        // Assert
        state.IsPaused.Should().BeTrue();
    }

    [Fact]
    public void Score_CanBeModified()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.Score = 250;

        // Assert
        state.Score.Should().Be(250);
    }

    [Fact]
    public void Difficulty_CanBeModified()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.Difficulty = Difficulty.Easy;

        // Assert
        state.Difficulty.Should().Be(Difficulty.Easy);
    }

    [Fact]
    public void SpeedLevel_CanBeModified()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.SpeedLevel = 10;

        // Assert
        state.SpeedLevel.Should().Be(10);
    }

    [Fact]
    public void Direction_CanBeModified()
    {
        // Arrange
        var state = new GameState();
        var newDirection = new Point(0, -1);

        // Act
        state.Direction = newDirection;

        // Assert
        state.Direction.Should().Be(newDirection);
    }

    [Fact]
    public void CurrentLevel_CanBeSet()
    {
        // Arrange
        var state = new GameState();
        var level = new Level();

        // Act
        state.CurrentLevel = level;

        // Assert
        state.CurrentLevel.Should().Be(level);
    }

    [Fact]
    public void CurrentLevel_CanBeNull()
    {
        // Arrange
        var state = new GameState
        {
            CurrentLevel = new Level()
        };

        // Act
        state.CurrentLevel = null;

        // Assert
        state.CurrentLevel.Should().BeNull();
    }

    [Fact]
    public void IsLevelCompleted_CanBeModified()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.IsLevelCompleted = true;

        // Assert
        state.IsLevelCompleted.Should().BeTrue();
    }

    [Fact]
    public void LevelTime_CanBeIncremented()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.LevelTime = 30;

        // Assert
        state.LevelTime.Should().Be(30);
    }

    [Fact]
    public void FoodCollected_CanBeIncremented()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.FoodCollected = 5;

        // Assert
        state.FoodCollected.Should().Be(5);
    }

    [Fact]
    public void TotalFoodSpawned_CanBeIncremented()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.TotalFoodSpawned = 20;

        // Assert
        state.TotalFoodSpawned.Should().Be(20);
    }

    [Fact]
    public void Snake_AddMultipleSegments_ShouldMaintainOrder()
    {
        // Arrange
        var state = new GameState();
        var positions = new[] { new Point(10, 10), new Point(9, 10), new Point(8, 10) };

        // Act
        foreach (var pos in positions)
        {
            state.Snake.AddLast(pos);
        }

        // Assert
        state.Snake.Count.Should().Be(3);
        state.Snake.First!.Value.Should().Be(new Point(10, 10));
        state.Snake.Last!.Value.Should().Be(new Point(8, 10));
    }

    [Fact]
    public void Foods_AddMultipleItems_ShouldMaintainOrder()
    {
        // Arrange
        var state = new GameState();
        var foodPositions = new[] { new Point(5, 5), new Point(10, 10), new Point(15, 15) };

        // Act
        foreach (var pos in foodPositions)
        {
            state.Foods.Add(pos);
        }

        // Assert
        state.Foods.Should().HaveCount(3);
        state.Foods[0].Should().Be(new Point(5, 5));
        state.Foods[2].Should().Be(new Point(15, 15));
    }

    [Fact]
    public void Clone_CurrentLevelReference_ShouldBeSameReference()
    {
        // Arrange
        var level = new Level();
        var original = new GameState { CurrentLevel = level };

        // Act
        var cloned = original.Clone();

        // Assert
        cloned.CurrentLevel.Should().BeSameAs(level);
    }

    [Fact]
    public void IsNewHighScore_CanBeModified()
    {
        // Arrange
        var state = new GameState();

        // Act
        state.IsNewHighScore = true;

        // Assert
        state.IsNewHighScore.Should().BeTrue();
    }

    [Fact]
    public void Foods_Clear_ShouldEmptyList()
    {
        // Arrange
        var state = new GameState();
        state.Foods.Add(new Point(10, 10));
        state.Foods.Add(new Point(15, 15));

        // Act
        state.Foods.Clear();

        // Assert
        state.Foods.Should().BeEmpty();
    }

    [Fact]
    public void Foods_RemoveAt_ShouldRemoveItem()
    {
        // Arrange
        var state = new GameState();
        state.Foods.Add(new Point(10, 10));
        state.Foods.Add(new Point(15, 15));
        state.Foods.Add(new Point(20, 20));

        // Act
        state.Foods.RemoveAt(1);

        // Assert
        state.Foods.Should().HaveCount(2);
        state.Foods.Should().NotContain(new Point(15, 15));
    }

    [Fact]
    public void Foods_Indexer_CanSetItem()
    {
        // Arrange
        var state = new GameState();
        state.Foods.Add(new Point(10, 10));
        state.Foods.Add(new Point(15, 15));

        // Act
        state.Foods[0] = new Point(5, 5);

        // Assert
        state.Foods[0].Should().Be(new Point(5, 5));
    }

    [Fact]
    public void Snake_Clear_ShouldEmptyList()
    {
        // Arrange
        var state = new GameState();
        state.Snake.AddLast(new Point(10, 10));
        state.Snake.AddLast(new Point(15, 15));

        // Act
        state.Snake.Clear();

        // Assert
        state.Snake.Count.Should().Be(0);
        state.Snake.First.Should().BeNull();
        state.Snake.Last.Should().BeNull();
    }

    [Fact]
    public void Snake_RemoveFirst_ShouldRemoveHead()
    {
        // Arrange
        var state = new GameState();
        state.Snake.AddLast(new Point(10, 10));
        state.Snake.AddLast(new Point(15, 15));
        state.Snake.AddLast(new Point(20, 20));

        // Act
        state.Snake.RemoveFirst();

        // Assert
        state.Snake.Count.Should().Be(2);
        state.Snake.First!.Value.Should().Be(new Point(15, 15));
    }

    [Fact]
    public void Snake_RemoveFirst_OnEmptyList_ShouldReturnFalse()
    {
        // Arrange
        var state = new GameState();

        // Act
        var result = state.Snake.RemoveFirst();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Snake_AddBefore_ShouldInsertNode()
    {
        // Arrange
        var state = new GameState();
        var first = state.Snake.AddLast(new Point(10, 10));
        var last = state.Snake.AddLast(new Point(20, 20));

        // Act
        var newNode = state.Snake.AddBefore(last, new Point(15, 15));

        // Assert
        state.Snake.Count.Should().Be(3);
        first.Next.Should().Be(newNode);
        newNode.Next.Should().Be(last);
    }

    [Fact]
    public void Snake_AddAfter_ShouldInsertNode()
    {
        // Arrange
        var state = new GameState();
        var first = state.Snake.AddLast(new Point(10, 10));
        var last = state.Snake.AddLast(new Point(20, 20));

        // Act
        var newNode = state.Snake.AddAfter(first, new Point(15, 15));

        // Assert
        state.Snake.Count.Should().Be(3);
        first.Next.Should().Be(newNode);
        newNode.Next.Should().Be(last);
    }

    [Fact]
    public void Snake_Contains_ShouldReturnTrueWhenExists()
    {
        // Arrange
        var state = new GameState();
        state.Snake.AddLast(new Point(10, 10));
        state.Snake.AddLast(new Point(15, 15));

        // Act
        var result = state.Snake.Contains(new Point(15, 15));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Snake_Contains_ShouldReturnFalseWhenNotExists()
    {
        // Arrange
        var state = new GameState();
        state.Snake.AddLast(new Point(10, 10));

        // Act
        var result = state.Snake.Contains(new Point(15, 15));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Snake_Find_ShouldReturnCorrectNode()
    {
        // Arrange
        var state = new GameState();
        state.Snake.AddLast(new Point(10, 10));
        var expectedNode = state.Snake.AddLast(new Point(15, 15));
        state.Snake.AddLast(new Point(20, 20));

        // Act
        var foundNode = state.Snake.Find(new Point(15, 15));

        // Assert
        foundNode.Should().Be(expectedNode);
    }

    [Fact]
    public void Snake_FindLast_ShouldReturnLastOccurrence()
    {
        // Arrange
        var state = new GameState();
        state.Snake.AddLast(new Point(10, 10));
        state.Snake.AddLast(new Point(15, 15));
        var lastNode = state.Snake.AddLast(new Point(10, 10));

        // Act
        var foundNode = state.Snake.FindLast(new Point(10, 10));

        // Assert
        foundNode.Should().Be(lastNode);
    }

    [Fact]
    public void Snake_RemoveByValue_ShouldReturnTrueWhenRemoved()
    {
        // Arrange
        var state = new GameState();
        state.Snake.AddLast(new Point(10, 10));
        state.Snake.AddLast(new Point(15, 15));
        state.Snake.AddLast(new Point(20, 20));

        // Act
        var result = state.Snake.Remove(new Point(15, 15));

        // Assert
        result.Should().BeTrue();
        state.Snake.Count.Should().Be(2);
        state.Snake.Contains(new Point(15, 15)).Should().BeFalse();
    }

    [Fact]
    public void Snake_RemoveByValue_ShouldReturnFalseWhenNotFound()
    {
        // Arrange
        var state = new GameState();
        state.Snake.AddLast(new Point(10, 10));

        // Act
        var result = state.Snake.Remove(new Point(15, 15));

        // Assert
        result.Should().BeFalse();
        state.Snake.Count.Should().Be(1);
    }
}
