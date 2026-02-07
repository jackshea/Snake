using AiPlayground.Services;
using FluentAssertions;
using Xunit;

namespace AiPlayground.Tests.Services;

public class DefaultTimeProviderTests
{
    [Fact]
    public void GetCurrentUnixTimeMilliseconds_ShouldReturnPositiveValue()
    {
        // Arrange
        var provider = new DefaultTimeProvider();

        // Act
        var timestamp = provider.GetCurrentUnixTimeMilliseconds();

        // Assert
        timestamp.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetCurrentUnixTimeMilliseconds_ShouldReturnRecentTimestamp()
    {
        // Arrange
        var provider = new DefaultTimeProvider();
        var expectedMin = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeMilliseconds();
        var expectedMax = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeMilliseconds();

        // Act
        var timestamp = provider.GetCurrentUnixTimeMilliseconds();

        // Assert
        timestamp.Should().BeGreaterOrEqualTo(expectedMin);
        timestamp.Should().BeLessOrEqualTo(expectedMax);
    }

    [Fact]
    public void GetCurrentUnixTimeMilliseconds_MultipleCalls_ShouldReturnIncreasingValues()
    {
        // Arrange
        var provider = new DefaultTimeProvider();

        // Act
        var timestamp1 = provider.GetCurrentUnixTimeMilliseconds();
        System.Threading.Thread.Sleep(10);
        var timestamp2 = provider.GetCurrentUnixTimeMilliseconds();

        // Assert
        timestamp2.Should().BeGreaterThan(timestamp1);
    }

    [Fact]
    public void Now_ShouldReturnCurrentDateTime()
    {
        // Arrange
        var provider = new DefaultTimeProvider();
        var before = DateTime.Now.AddSeconds(-1);

        // Act
        var now = provider.Now;

        // Assert
        now.Should().BeAfter(before);
        now.Should().BeBefore(DateTime.Now.AddSeconds(1));
    }

    [Fact]
    public void Now_ShouldReturnDifferentValuesOverTime()
    {
        // Arrange
        var provider = new DefaultTimeProvider();

        // Act
        var now1 = provider.Now;
        System.Threading.Thread.Sleep(10);
        var now2 = provider.Now;

        // Assert
        now2.Should().BeAfter(now1);
    }

    [Fact]
    public void GetCurrentUnixTimeMilliseconds_ShouldMatchNowWithinTolerance()
    {
        // Arrange
        var provider = new DefaultTimeProvider();

        // Act
        var unixTime = provider.GetCurrentUnixTimeMilliseconds();
        var now = provider.Now;

        // Assert - Convert DateTime to Unix time and compare
        var expectedUnixTime = new DateTimeOffset(now).ToUnixTimeMilliseconds();
        var difference = Math.Abs(unixTime - expectedUnixTime);

        // Allow up to 1 second difference due to execution time
        difference.Should().BeLessThan(1000);
    }
}
