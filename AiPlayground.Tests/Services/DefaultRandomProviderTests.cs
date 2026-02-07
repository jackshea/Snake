using AiPlayground.Services;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Services;

public class DefaultRandomProviderTests
{
    [Fact]
    public void Next_MaxValueOnly_ShouldReturnNonNegative()
    {
        // Arrange
        var provider = new DefaultRandomProvider();

        // Act
        var result = provider.Next(100);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
        result.Should().BeLessThan(100);
    }

    [Fact]
    public void Next_MaxValueOnly_MultipleCalls_ShouldReturnVariousValues()
    {
        // Arrange
        var provider = new DefaultRandomProvider();
        var results = new HashSet<int>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            results.Add(provider.Next(10));
        }

        // Assert - With 100 calls, we should get multiple different values
        results.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public void Next_MinAndMaxValue_ShouldReturnInRange()
    {
        // Arrange
        var provider = new DefaultRandomProvider();

        // Act
        var result = provider.Next(5, 15);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(5);
        result.Should().BeLessThan(15);
    }

    [Fact]
    public void Next_SameMinAndMaxValue_ShouldReturnValue()
    {
        // Arrange
        var provider = new DefaultRandomProvider();

        // Act
        var result = provider.Next(10, 11);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public void Next_WithNegativeMinValue_ShouldHandleCorrectly()
    {
        // Arrange
        var provider = new DefaultRandomProvider();

        // Act
        var result = provider.Next(-10, 10);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(-10);
        result.Should().BeLessThan(10);
    }

    [Fact]
    public void NextDouble_ShouldReturnBetweenZeroAndOne()
    {
        // Arrange
        var provider = new DefaultRandomProvider();

        // Act
        var result = provider.NextDouble();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0.0);
        result.Should().BeLessThan(1.0);
    }

    [Fact]
    public void NextDouble_MultipleCalls_ShouldReturnVariousValues()
    {
        // Arrange
        var provider = new DefaultRandomProvider();
        var results = new HashSet<double>();

        // Act
        for (int i = 0; i < 50; i++)
        {
            results.Add(provider.NextDouble());
        }

        // Assert
        results.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public void Next_ZeroMaxValue_ShouldReturnZero()
    {
        // Arrange
        var provider = new DefaultRandomProvider();

        // Act
        var result = provider.Next(1);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Next_MultipleInstances_ShouldProduceDifferentSequences()
    {
        // Arrange
        var provider1 = new DefaultRandomProvider();
        var provider2 = new DefaultRandomProvider();

        // Act
        var value1 = provider1.Next(1000000);
        var value2 = provider2.Next(1000000);

        // Assert - Different random instances should very likely produce different values
        value1.Should().NotBe(value2);
    }
}
