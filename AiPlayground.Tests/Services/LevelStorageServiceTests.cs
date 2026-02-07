using AiPlayground.Services;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Services;

public class LevelStorageServiceTests
{
    [Fact]
    public void Constructor_ShouldInitializeService()
    {
        // Arrange & Act
        var service = new LevelStorageService();

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void LoadPresetLevels_ShouldReturnLevels()
    {
        // Arrange
        var service = new LevelStorageService();

        // Act
        var levels = service.LoadPresetLevels();

        // Assert
        levels.Should().NotBeNull();
        // May have levels if preset levels exist in the project
    }

    [Fact]
    public void LoadCustomLevels_ShouldReturnList()
    {
        // Arrange
        var service = new LevelStorageService();

        // Act
        var levels = service.LoadCustomLevels();

        // Assert
        levels.Should().NotBeNull();
    }

    [Fact]
    public void LoadProgression_ShouldReturnProgression()
    {
        // Arrange
        var service = new LevelStorageService();

        // Act
        var progression = service.LoadProgression();

        // Assert
        progression.Should().NotBeNull();
        progression.HighestUnlockedLevel.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void GetJsonOptions_ShouldReturnOptions()
    {
        // Arrange
        var service = new LevelStorageService();

        // Act
        var options = service.GetJsonOptions();

        // Assert
        options.Should().NotBeNull();
        options.WriteIndented.Should().BeTrue();
    }

    [Fact]
    public async Task SaveProgressionAsync_ShouldSucceed()
    {
        // Arrange
        var service = new LevelStorageService();
        var progression = service.LoadProgression();

        // Act
        var result = await service.SaveProgressionAsync(progression);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ResetProgression_ShouldReturnTrue()
    {
        // Arrange
        var service = new LevelStorageService();

        // Act
        var result = service.ResetProgression();

        // Assert
        result.Should().BeTrue();
    }
}
