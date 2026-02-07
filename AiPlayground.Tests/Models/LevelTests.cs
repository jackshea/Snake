using System.Drawing;
using AiPlayground.Models;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Models;

public class LevelTests
{
    [Fact]
    public void DefaultConstructor_ShouldGenerateUniqueId()
    {
        // Act
        var level = new Level();

        // Assert
        level.Id.Should().NotBeEmpty();
        Guid.TryParse(level.Id, out _).Should().BeTrue();
    }

    [Fact]
    public void DefaultConstructor_ShouldSetIsUnlockedToFalse()
    {
        // Act
        var level = new Level();

        // Assert
        level.IsUnlocked.Should().BeFalse();
    }

    [Fact]
    public void DefaultConstructor_ShouldSetIsCustomToFalse()
    {
        // Act
        var level = new Level();

        // Assert
        level.IsCustom.Should().BeFalse();
    }

    [Fact]
    public void CreatePresetLevel_ShouldSetCorrectId()
    {
        // Act
        var level = Level.CreatePresetLevel(5, "Test Level", "Test Description");

        // Assert
        level.Id.Should().Be("preset_level_5");
    }

    [Fact]
    public void CreatePresetLevel_ShouldSetCorrectName()
    {
        // Act
        var level = Level.CreatePresetLevel(1, "My Level", "Description");

        // Assert
        level.Name.Should().Be("My Level");
    }

    [Fact]
    public void CreatePresetLevel_ShouldSetCorrectDescription()
    {
        // Act
        var level = Level.CreatePresetLevel(1, "Name", "My Description");

        // Assert
        level.Description.Should().Be("My Description");
    }

    [Fact]
    public void CreatePresetLevel_ShouldSetCorrectLevelNumber()
    {
        // Act
        var level = Level.CreatePresetLevel(10, "Name", "Description");

        // Assert
        level.LevelNumber.Should().Be(10);
    }

    [Fact]
    public void CreatePresetLevel_FirstLevel_ShouldBeUnlocked()
    {
        // Act
        var level = Level.CreatePresetLevel(1, "First", "Description");

        // Assert
        level.IsUnlocked.Should().BeTrue();
    }

    [Fact]
    public void CreatePresetLevel_NonFirstLevel_ShouldNotBeUnlocked()
    {
        // Act
        var level = Level.CreatePresetLevel(2, "Second", "Description");

        // Assert
        level.IsUnlocked.Should().BeFalse();
    }

    [Fact]
    public void CreatePresetLevel_ShouldSetIsCustomToFalse()
    {
        // Act
        var level = Level.CreatePresetLevel(1, "Name", "Description");

        // Assert
        level.IsCustom.Should().BeFalse();
    }

    [Fact]
    public void CreateCustomLevel_ShouldGenerateCustomId()
    {
        // Act
        var level = Level.CreateCustomLevel("My Custom Level");

        // Assert
        level.Id.Should().StartWith("custom_");
    }

    [Fact]
    public void CreateCustomLevel_ShouldSetCorrectName()
    {
        // Act
        var level = Level.CreateCustomLevel("Custom Level Name");

        // Assert
        level.Name.Should().Be("Custom Level Name");
    }

    [Fact]
    public void CreateCustomLevel_ShouldSetDefaultDescription()
    {
        // Act
        var level = Level.CreateCustomLevel("Name");

        // Assert
        level.Description.Should().Be("自定义关卡");
    }

    [Fact]
    public void CreateCustomLevel_ShouldSetLevelNumberToZero()
    {
        // Act
        var level = Level.CreateCustomLevel("Name");

        // Assert
        level.LevelNumber.Should().Be(0);
    }

    [Fact]
    public void CreateCustomLevel_ShouldBeUnlocked()
    {
        // Act
        var level = Level.CreateCustomLevel("Name");

        // Assert
        level.IsUnlocked.Should().BeTrue();
    }

    [Fact]
    public void CreateCustomLevel_ShouldSetIsCustomToTrue()
    {
        // Act
        var level = Level.CreateCustomLevel("Name");

        // Assert
        level.IsCustom.Should().BeTrue();
    }

    [Fact]
    public void GridWidth_ShouldReturnSettingsGridWidth()
    {
        // Arrange
        var level = new Level { Settings = new LevelSettings { GridWidth = 50 } };

        // Act
        var gridWidth = level.GridWidth;

        // Assert
        gridWidth.Should().Be(50);
    }

    [Fact]
    public void GridHeight_ShouldReturnSettingsGridHeight()
    {
        // Arrange
        var level = new Level { Settings = new LevelSettings { GridHeight = 40 } };

        // Act
        var gridHeight = level.GridHeight;

        // Assert
        gridHeight.Should().Be(40);
    }

    [Fact]
    public void FixedFoodPositions_ShouldBeEmptyByDefault()
    {
        // Act
        var level = new Level();

        // Assert
        level.FixedFoodPositions.Should().BeEmpty();
    }

    [Fact]
    public void FixedFoodPositions_CanAddItems()
    {
        // Arrange
        var level = new Level();

        // Act
        level.FixedFoodPositions.Add(new Point(10, 10));
        level.FixedFoodPositions.Add(new Point(15, 15));

        // Assert
        level.FixedFoodPositions.Should().HaveCount(2);
    }

    [Fact]
    public void Obstacles_ShouldBeEmptyByDefault()
    {
        // Act
        var level = new Level();

        // Assert
        level.Obstacles.Should().BeEmpty();
    }

    [Fact]
    public void VictoryCondition_ShouldHaveDefaultInstance()
    {
        // Act
        var level = new Level();

        // Assert
        level.VictoryCondition.Should().NotBeNull();
    }

    [Fact]
    public void Settings_ShouldHaveDefaultInstance()
    {
        // Act
        var level = new Level();

        // Assert
        level.Settings.Should().NotBeNull();
    }
}
