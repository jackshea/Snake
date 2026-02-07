using System.Drawing;
using FluentAssertions;
using Xunit;
using AiLinkedList = AiPlayground.Models.Collections.LinkedList<int>;
using AiPointLinkedList = AiPlayground.Models.Collections.LinkedList<System.Drawing.Point>;

namespace AiPlayground.Tests.Models.Collections;

public class LinkedListTests
{
    [Fact]
    public void NewList_ShouldBeEmpty()
    {
        // Arrange & Act
        var list = new AiLinkedList();

        // Assert
        list.Count.Should().Be(0);
        list.First.Should().BeNull();
        list.Last.Should().BeNull();
    }

    [Fact]
    public void AddFirst_ToEmptyList_ShouldSetHeadAndTail()
    {
        // Arrange
        var list = new AiLinkedList();

        // Act
        var node = list.AddFirst(10);

        // Assert
        list.Count.Should().Be(1);
        list.First.Should().Be(node);
        list.Last.Should().Be(node);
        list.First!.Value.Should().Be(10);
    }

    [Fact]
    public void AddFirst_ToNonEmptyList_ShouldUpdateHead()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddFirst(10);
        list.AddFirst(20);

        // Act
        var node = list.AddFirst(30);

        // Assert
        list.Count.Should().Be(3);
        list.First!.Value.Should().Be(30);
        list.Last!.Value.Should().Be(10);
    }

    [Fact]
    public void AddLast_ToEmptyList_ShouldSetHeadAndTail()
    {
        // Arrange
        var list = new AiLinkedList();

        // Act
        var node = list.AddLast(10);

        // Assert
        list.Count.Should().Be(1);
        list.First.Should().Be(node);
        list.Last.Should().Be(node);
        list.Last!.Value.Should().Be(10);
    }

    [Fact]
    public void AddLast_ToNonEmptyList_ShouldUpdateTail()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);
        list.AddLast(20);

        // Act
        var node = list.AddLast(30);

        // Assert
        list.Count.Should().Be(3);
        list.First!.Value.Should().Be(10);
        list.Last!.Value.Should().Be(30);
    }

    [Fact]
    public void RemoveLast_FromListWithMultipleNodes_ShouldUpdateTail()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);
        list.AddLast(20);
        list.AddLast(30);

        // Act
        list.RemoveLast();

        // Assert
        list.Count.Should().Be(2);
        list.First!.Value.Should().Be(10);
        list.Last!.Value.Should().Be(20);
    }

    [Fact]
    public void RemoveLast_FromSingleElementList_ShouldClearList()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);

        // Act
        list.RemoveLast();

        // Assert
        list.Count.Should().Be(0);
        list.First.Should().BeNull();
        list.Last.Should().BeNull();
    }

    [Fact]
    public void RemoveLast_FromEmptyList_ShouldReturnFalse()
    {
        // Arrange
        var list = new AiLinkedList();

        // Act
        var result = list.RemoveLast();

        // Assert
        result.Should().BeFalse();
        list.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveFirst_FromListWithMultipleNodes_ShouldUpdateHead()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);
        list.AddLast(20);
        list.AddLast(30);

        // Act
        list.RemoveFirst();

        // Assert
        list.Count.Should().Be(2);
        list.First!.Value.Should().Be(20);
        list.Last!.Value.Should().Be(30);
    }

    [Fact]
    public void Find_ShouldReturnCorrectNode()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);
        list.AddLast(20);
        list.AddLast(30);

        // Act
        var node = list.Find(20);

        // Assert
        node.Should().NotBeNull();
        node!.Value.Should().Be(20);
    }

    [Fact]
    public void Find_WhenValueNotExists_ShouldReturnNull()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);
        list.AddLast(20);

        // Act
        var node = list.Find(30);

        // Assert
        node.Should().BeNull();
    }

    [Fact]
    public void FindLast_ShouldReturnLastMatchingNode()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);
        list.AddLast(20);
        list.AddLast(20); // Duplicate
        list.AddLast(30);

        // Act
        var node = list.FindLast(20);

        // Assert
        node.Should().NotBeNull();
        node!.Value.Should().Be(20);
        // Should be the second 20 (not the first)
        node.Next!.Value.Should().Be(30);
    }

    [Fact]
    public void Contains_ShouldReturnTrueWhenValueExists()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);
        list.AddLast(20);

        // Act
        var result = list.Contains(20);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_ShouldReturnFalseWhenValueNotExists()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);

        // Act
        var result = list.Contains(20);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Clear_ShouldRemoveAllNodes()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);
        list.AddLast(20);
        list.AddLast(30);

        // Act
        list.Clear();

        // Assert
        list.Count.Should().Be(0);
        list.First.Should().BeNull();
        list.Last.Should().BeNull();
    }

    [Fact]
    public void Indexer_ShouldReturnCorrectElement()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);
        list.AddLast(20);
        list.AddLast(30);

        // Act
        var value = list[1];

        // Assert
        value.Should().Be(20);
    }

    [Fact]
    public void Indexer_WhenIndexOutOfRange_ShouldThrow()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);

        // Act
        Action action = () => _ = list[5];

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Enumerator_ShouldEnumerateAllElements()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);
        list.AddLast(20);
        list.AddLast(30);

        // Act
        var result = list.ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(new[] { 10, 20, 30 });
    }

    [Fact]
    public void AddBefore_ShouldInsertNodeBeforeTarget()
    {
        // Arrange
        var list = new AiLinkedList();
        var first = list.AddLast(10);
        var second = list.AddLast(30);

        // Act
        var newNode = list.AddBefore(second, 20);

        // Assert
        list.Count.Should().Be(3);
        list.First.Should().Be(first);
        first.Next.Should().Be(newNode);
        newNode.Next.Should().Be(second);
    }

    [Fact]
    public void AddAfter_ShouldInsertNodeAfterTarget()
    {
        // Arrange
        var list = new AiLinkedList();
        var first = list.AddLast(10);
        var second = list.AddLast(20);

        // Act
        var newNode = list.AddAfter(first, 15);

        // Assert
        list.Count.Should().Be(3);
        list.First.Should().Be(first);
        first.Next.Should().Be(newNode);
        newNode.Next.Should().Be(second);
    }

    [Fact]
    public void WithPointTypes_ShouldWorkCorrectly()
    {
        // Arrange
        var list = new AiPointLinkedList();

        // Act
        list.AddLast(new Point(1, 2));
        list.AddLast(new Point(3, 4));
        list.AddLast(new Point(5, 6));

        // Assert
        list.Count.Should().Be(3);
        list.First!.Value.Should().Be(new Point(1, 2));
        list.Last!.Value.Should().Be(new Point(5, 6));
    }

    [Fact]
    public void NodePreviousAndNext_ShouldBeCorrectlyLinked()
    {
        // Arrange
        var list = new AiLinkedList();
        var first = list.AddLast(10);
        var second = list.AddLast(20);
        var third = list.AddLast(30);

        // Assert
        first.Previous.Should().BeNull();
        first.Next.Should().Be(second);

        second.Previous.Should().Be(first);
        second.Next.Should().Be(third);

        third.Previous.Should().Be(second);
        third.Next.Should().BeNull();
    }

    [Fact]
    public void RemoveNode_ShouldCorrectlyUnlinkNode()
    {
        // Arrange
        var list = new AiLinkedList();
        var first = list.AddLast(10);
        var middle = list.AddLast(20);
        var last = list.AddLast(30);

        // Act
        list.Remove(middle);

        // Assert
        list.Count.Should().Be(2);
        first.Next.Should().Be(last);
        last.Previous.Should().Be(first);
        middle.List.Should().BeNull();
    }

    [Fact]
    public void RemoveByValue_ShouldReturnTrueWhenRemoved()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);
        list.AddLast(20);
        list.AddLast(30);

        // Act
        var result = list.Remove(20);

        // Assert
        result.Should().BeTrue();
        list.Count.Should().Be(2);
        list.Contains(20).Should().BeFalse();
    }

    [Fact]
    public void RemoveByValue_ShouldReturnFalseWhenValueNotFound()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);
        list.AddLast(30);

        // Act
        var result = list.Remove(20);

        // Assert
        result.Should().BeFalse();
        list.Count.Should().Be(2);
    }

    [Fact]
    public void CopyTo_ShouldCopyElementsToArray()
    {
        // Arrange
        var list = new AiLinkedList();
        list.AddLast(10);
        list.AddLast(20);
        list.AddLast(30);
        var array = new int[5];

        // Act
        list.CopyTo(array, 1);

        // Assert
        array.Should().BeEquivalentTo(new[] { 0, 10, 20, 30, 0 });
    }
}
