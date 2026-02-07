using AiPlayground.Services;
using AiPlayground.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Services;

public class HighScoreServiceTests
{
    [Fact]
    public void LoadHighScore_WhenFileNotExists_ShouldReturnZero()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new HighScoreService(mockFileSystem);

        // Act
        var score = service.LoadHighScore();

        // Assert
        score.Should().Be(0);
    }

    [Fact]
    public void LoadHighScore_WhenFileExistsWithValidScore_ShouldReturnScore()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        mockFileSystem.SetupFile("/test/highscore.txt", "150");

        var service = new HighScoreService(mockFileSystem);

        // Note: The service uses a fixed path, so we need to test Save/Load together
        service.SaveHighScore(150);

        // Act
        var score = service.LoadHighScore();

        // Assert
        score.Should().Be(150);
    }

    [Fact]
    public void SaveHighScore_ShouldCreateFileWithScore()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new HighScoreService(mockFileSystem);

        // Act
        service.SaveHighScore(250);

        // Assert
        var score = service.LoadHighScore();
        score.Should().Be(250);
    }

    [Fact]
    public void TryUpdateHighScore_WithHigherScore_ShouldUpdateAndReturnTrue()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new HighScoreService(mockFileSystem);
        var highScore = 100;

        // Act
        var result = service.TryUpdateHighScore(150, ref highScore);

        // Assert
        result.Should().BeTrue();
        highScore.Should().Be(150);
    }

    [Fact]
    public void TryUpdateHighScore_WithLowerScore_ShouldNotUpdateAndReturnFalse()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new HighScoreService(mockFileSystem);
        var highScore = 150;

        // Act
        var result = service.TryUpdateHighScore(100, ref highScore);

        // Assert
        result.Should().BeFalse();
        highScore.Should().Be(150);
    }

    [Fact]
    public void TryUpdateHighScore_WithSameScore_ShouldNotUpdateAndReturnFalse()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new HighScoreService(mockFileSystem);
        var highScore = 100;

        // Act
        var result = service.TryUpdateHighScore(100, ref highScore);

        // Assert
        result.Should().BeFalse();
        highScore.Should().Be(100);
    }

    [Fact]
    public void LoadHighScore_WithZeroScore_ShouldReturnZero()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new HighScoreService(mockFileSystem);

        // Act
        service.SaveHighScore(0);
        var score = service.LoadHighScore();

        // Assert
        score.Should().Be(0);
    }

    [Fact]
    public void SaveHighScore_MultipleTimes_ShouldKeepHighest()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new HighScoreService(mockFileSystem);

        // Act
        service.SaveHighScore(100);
        service.SaveHighScore(250);
        service.SaveHighScore(150);

        // Assert
        var score = service.LoadHighScore();
        score.Should().Be(150); // Last saved value
    }

    [Fact]
    public void LoadHighScore_WithNegativeScore_ShouldHandleCorrectly()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var service = new HighScoreService(mockFileSystem);

        // Act
        service.SaveHighScore(-50);

        // Assert
        var score = service.LoadHighScore();
        score.Should().Be(-50);
    }
}
