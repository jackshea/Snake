using FluentAssertions;
using Xunit;
using AiLinkedListNode = AiPlayground.Models.Collections.LinkedListNode<int>;
using AiStringLinkedListNode = AiPlayground.Models.Collections.LinkedListNode<string>;

namespace AiPlayground.Tests.Models.Collections;

public class LinkedListNodeTests
{
    [Fact]
    public void Constructor_ShouldSetValue()
    {
        // Arrange & Act
        var node = new AiLinkedListNode(42);

        // Assert
        node.Value.Should().Be(42);
    }

    [Fact]
    public void Value_ShouldBeModifiable()
    {
        // Arrange
        var node = new AiLinkedListNode(42);

        // Act
        node.Value = 100;

        // Assert
        node.Value.Should().Be(100);
    }

    [Fact]
    public void Previous_Initially_ShouldBeNull()
    {
        // Arrange & Act
        var node = new AiLinkedListNode(42);

        // Assert
        node.Previous.Should().BeNull();
    }

    [Fact]
    public void Next_Initially_ShouldBeNull()
    {
        // Arrange & Act
        var node = new AiLinkedListNode(42);

        // Assert
        node.Next.Should().BeNull();
    }

    [Fact]
    public void List_Initially_ShouldBeNull()
    {
        // Arrange & Act
        var node = new AiLinkedListNode(42);

        // Assert
        node.List.Should().BeNull();
    }

    [Fact]
    public void ToString_ShouldReturnValueAsString()
    {
        // Arrange
        var node = new AiLinkedListNode(42);

        // Act
        var result = node.ToString();

        // Assert
        result.Should().Be("42");
    }

    [Fact]
    public void ToString_WithStringValue_ShouldReturnValue()
    {
        // Arrange
        var node = new AiStringLinkedListNode("test");

        // Act
        var result = node.ToString();

        // Assert
        result.Should().Be("test");
    }

    [Fact]
    public void ToString_WithNullValue_ShouldReturnNull()
    {
        // Arrange
        var node = new AiStringLinkedListNode(null!);

        // Act
        var result = node.ToString();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void WithObjectType_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var node = new AiPlayground.Models.Collections.LinkedListNode<object>(new { Name = "Test", Value = 123 });

        // Assert
        node.Value.Should().NotBeNull();
    }
}
