using AiPlayground.Models;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Models;

public class LevelProgressionTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeEmptyCompletedLevels()
    {
        // Act
        var progression = new LevelProgression();

        // Assert
        progression.CompletedLevels.Should().BeEmpty();
    }

    [Fact]
    public void DefaultConstructor_ShouldSetHighestUnlockedLevelTo1()
    {
        // Act
        var progression = new LevelProgression();

        // Assert
        progression.HighestUnlockedLevel.Should().Be(1);
    }

    [Fact]
    public void DefaultConstructor_ShouldSetLastPlayedTime()
    {
        // Act
        var progression = new LevelProgression();
        var before = DateTime.Now.AddSeconds(-1);

        // Assert
        progression.LastPlayedTime.Should().BeAfter(before);
        progression.LastPlayedTime.Should().BeBefore(DateTime.Now.AddSeconds(1));
    }

    [Fact]
    public void GetCompletion_NonExistentLevel_ShouldReturnNull()
    {
        // Arrange
        var progression = new LevelProgression();

        // Act
        var completion = progression.GetCompletion("non_existent_level");

        // Assert
        completion.Should().BeNull();
    }

    [Fact]
    public void GetCompletion_ExistingLevel_ShouldReturnCompletion()
    {
        // Arrange
        var progression = new LevelProgression();
        var levelId = "level_1";
        progression.UpdateCompletion(levelId, 100, 30);

        // Act
        var completion = progression.GetCompletion(levelId);

        // Assert
        completion.Should().NotBeNull();
        completion!.LevelId.Should().Be(levelId);
    }

    [Fact]
    public void UpdateCompletion_NewLevel_ShouldCreateCompletionRecord()
    {
        // Arrange
        var progression = new LevelProgression();
        var levelId = "level_1";

        // Act
        progression.UpdateCompletion(levelId, 150, 45);

        // Assert
        progression.CompletedLevels.Should().ContainKey(levelId);
        var completion = progression.CompletedLevels[levelId];
        completion.LevelId.Should().Be(levelId);
    }

    [Fact]
    public void UpdateCompletion_ShouldSetIsCompletedToTrue()
    {
        // Arrange
        var progression = new LevelProgression();
        var levelId = "level_1";

        // Act
        progression.UpdateCompletion(levelId, 100, 30);

        // Assert
        progression.CompletedLevels[levelId].IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void UpdateCompletion_ShouldIncrementCompletionCount()
    {
        // Arrange
        var progression = new LevelProgression();
        var levelId = "level_1";

        // Act
        progression.UpdateCompletion(levelId, 100, 30);
        progression.UpdateCompletion(levelId, 120, 25);
        progression.UpdateCompletion(levelId, 90, 35);

        // Assert
        progression.CompletedLevels[levelId].CompletionCount.Should().Be(3);
    }

    [Fact]
    public void UpdateCompletion_ShouldUpdateLastCompletedTime()
    {
        // Arrange
        var progression = new LevelProgression();
        var levelId = "level_1";
        var before = DateTime.Now.AddSeconds(-1);

        // Act
        progression.UpdateCompletion(levelId, 100, 30);

        // Assert
        progression.CompletedLevels[levelId].LastCompletedTime.Should().BeAfter(before);
    }

    [Fact]
    public void UpdateCompletion_HigherScore_ShouldUpdateBestScore()
    {
        // Arrange
        var progression = new LevelProgression();
        var levelId = "level_1";
        progression.UpdateCompletion(levelId, 100, 30);

        // Act
        progression.UpdateCompletion(levelId, 150, 25);

        // Assert
        progression.CompletedLevels[levelId].BestScore.Should().Be(150);
    }

    [Fact]
    public void UpdateCompletion_LowerScore_ShouldKeepBestScore()
    {
        // Arrange
        var progression = new LevelProgression();
        var levelId = "level_1";
        progression.UpdateCompletion(levelId, 150, 30);

        // Act
        progression.UpdateCompletion(levelId, 100, 25);

        // Assert
        progression.CompletedLevels[levelId].BestScore.Should().Be(150);
    }

    [Fact]
    public void UpdateCompletion_FasterTime_ShouldUpdateBestTime()
    {
        // Arrange
        var progression = new LevelProgression();
        var levelId = "level_1";
        progression.UpdateCompletion(levelId, 100, 30);

        // Act
        progression.UpdateCompletion(levelId, 100, 20);

        // Assert
        progression.CompletedLevels[levelId].BestTime.Should().Be(20);
    }

    [Fact]
    public void UpdateCompletion_SlowerTime_ShouldKeepBestTime()
    {
        // Arrange
        var progression = new LevelProgression();
        var levelId = "level_1";
        progression.UpdateCompletion(levelId, 100, 20);

        // Act
        progression.UpdateCompletion(levelId, 100, 30);

        // Assert
        progression.CompletedLevels[levelId].BestTime.Should().Be(20);
    }

    [Fact]
    public void UpdateCompletion_FirstCompletion_ShouldSetBestTime()
    {
        // Arrange
        var progression = new LevelProgression();
        var levelId = "level_1";

        // Act
        progression.UpdateCompletion(levelId, 100, 30);

        // Assert
        progression.CompletedLevels[levelId].BestTime.Should().Be(30);
    }

    [Fact]
    public void IsLevelUnlocked_Level1_ShouldReturnTrue()
    {
        // Arrange
        var progression = new LevelProgression();

        // Act
        var isUnlocked = progression.IsLevelUnlocked(1);

        // Assert
        isUnlocked.Should().BeTrue();
    }

    [Fact]
    public void IsLevelUnlocked_LevelBelowHighestUnlocked_ShouldReturnTrue()
    {
        // Arrange
        var progression = new LevelProgression { HighestUnlockedLevel = 5 };

        // Act
        var isUnlocked = progression.IsLevelUnlocked(3);

        // Assert
        isUnlocked.Should().BeTrue();
    }

    [Fact]
    public void IsLevelUnlocked_LevelEqualToHighestUnlocked_ShouldReturnTrue()
    {
        // Arrange
        var progression = new LevelProgression { HighestUnlockedLevel = 5 };

        // Act
        var isUnlocked = progression.IsLevelUnlocked(5);

        // Assert
        isUnlocked.Should().BeTrue();
    }

    [Fact]
    public void IsLevelUnlocked_LevelAboveHighestUnlocked_ShouldReturnFalse()
    {
        // Arrange
        var progression = new LevelProgression { HighestUnlockedLevel = 3 };

        // Act
        var isUnlocked = progression.IsLevelUnlocked(5);

        // Assert
        isUnlocked.Should().BeFalse();
    }

    [Fact]
    public void LevelCompletion_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var completion = new LevelCompletion();

        // Assert
        completion.LevelId.Should().BeEmpty();
        completion.IsCompleted.Should().BeFalse();
        completion.BestScore.Should().Be(0);
        completion.BestTime.Should().Be(0);
        completion.CompletionCount.Should().Be(0);
    }

    [Fact]
    public void HighestUnlockedLevel_CanBeModified()
    {
        // Arrange
        var progression = new LevelProgression();

        // Act
        progression.HighestUnlockedLevel = 10;

        // Assert
        progression.HighestUnlockedLevel.Should().Be(10);
    }

    [Fact]
    public void LastPlayedTime_CanBeModified()
    {
        // Arrange
        var progression = new LevelProgression();
        var newTime = new DateTime(2024, 1, 1, 12, 0, 0);

        // Act
        progression.LastPlayedTime = newTime;

        // Assert
        progression.LastPlayedTime.Should().Be(newTime);
    }
}
