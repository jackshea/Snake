using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AiPlayground.Models.Collections;

/// <summary>
/// 双向链表实现
/// </summary>
/// <typeparam name="T">元素类型</typeparam>
public class LinkedList<T> : ICollection<T>, IEnumerable<T>
{
    /// <summary>
    /// 链表头节点
    /// </summary>
    private LinkedListNode<T>? _head;

    /// <summary>
    /// 链表尾节点
    /// </summary>
    private LinkedListNode<T>? _tail;

    /// <summary>
    /// 链表元素数量
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// 获取链表头节点
    /// </summary>
    public LinkedListNode<T>? First => _head;

    /// <summary>
    /// 获取链表尾节点
    /// </summary>
    public LinkedListNode<T>? Last => _tail;

    /// <summary>
    /// 是否为只读集合
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// 索引器 - 按索引访问元素（O(n)复杂度）
    /// </summary>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var node = GetNodeAt(index);
            return node!.Value;
        }
        set
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var node = GetNodeAt(index);
            node!.Value = value;
        }
    }

    /// <summary>
    /// 在链表头部添加值
    /// </summary>
    public LinkedListNode<T> AddFirst(T value)
    {
        var newNode = new LinkedListNode<T>(value) { List = this };

        if (_head == null)
        {
            _head = _tail = newNode;
        }
        else
        {
            newNode.Next = _head;
            _head.Previous = newNode;
            _head = newNode;
        }

        Count++;
        return newNode;
    }

    /// <summary>
    /// 在链表尾部添加值
    /// </summary>
    public LinkedListNode<T> AddLast(T value)
    {
        var newNode = new LinkedListNode<T>(value) { List = this };

        if (_tail == null)
        {
            _head = _tail = newNode;
        }
        else
        {
            newNode.Previous = _tail;
            _tail.Next = newNode;
            _tail = newNode;
        }

        Count++;
        return newNode;
    }

    /// <summary>
    /// 在指定节点之前添加值
    /// </summary>
    public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));
        if (node.List != this)
            throw new InvalidOperationException("节点不属于此链表");

        var newNode = new LinkedListNode<T>(value) { List = this };

        newNode.Next = node;
        newNode.Previous = node.Previous;

        if (node.Previous != null)
        {
            node.Previous.Next = newNode;
        }

        node.Previous = newNode;

        if (_head == node)
        {
            _head = newNode;
        }

        Count++;
        return newNode;
    }

    /// <summary>
    /// 在指定节点之后添加值
    /// </summary>
    public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));
        if (node.List != this)
            throw new InvalidOperationException("节点不属于此链表");

        var newNode = new LinkedListNode<T>(value) { List = this };

        newNode.Previous = node;
        newNode.Next = node.Next;

        if (node.Next != null)
        {
            node.Next.Previous = newNode;
        }

        node.Next = newNode;

        if (_tail == node)
        {
            _tail = newNode;
        }

        Count++;
        return newNode;
    }

    /// <summary>
    /// 移除头部节点
    /// </summary>
    public bool RemoveFirst()
    {
        if (_head == null)
            return false;

        Remove(_head);
        return true;
    }

    /// <summary>
    /// 移除尾部节点
    /// </summary>
    public bool RemoveLast()
    {
        if (_tail == null)
            return false;

        Remove(_tail);
        return true;
    }

    /// <summary>
    /// 移除指定节点
    /// </summary>
    public void Remove(LinkedListNode<T> node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));
        if (node.List != this)
            throw new InvalidOperationException("节点不属于此链表");

        if (node.Previous != null)
        {
            node.Previous.Next = node.Next;
        }
        else
        {
            _head = node.Next;
        }

        if (node.Next != null)
        {
            node.Next.Previous = node.Previous;
        }
        else
        {
            _tail = node.Previous;
        }

        node.List = null;
        Count--;
    }

    /// <summary>
    /// 移除指定值的节点
    /// </summary>
    public bool Remove(T value)
    {
        var node = Find(value);
        if (node != null)
        {
            Remove(node);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 移除指定索引的节点
    /// </summary>
    public void RemoveAt(int index)
    {
        var node = GetNodeAt(index);
        if (node != null)
        {
            Remove(node);
        }
    }

    /// <summary>
    /// 查找包含指定值的节点
    /// </summary>
    public LinkedListNode<T>? Find(T value)
    {
        for (var node = _head; node != null; node = node.Next)
        {
            if (EqualityComparer<T>.Default.Equals(node.Value, value))
                return node;
        }
        return null;
    }

    /// <summary>
    /// 从后往前查找包含指定值的节点
    /// </summary>
    public LinkedListNode<T>? FindLast(T value)
    {
        for (var node = _tail; node != null; node = node.Previous)
        {
            if (EqualityComparer<T>.Default.Equals(node.Value, value))
                return node;
        }
        return null;
    }

    /// <summary>
    /// 检查是否包含指定值
    /// </summary>
    public bool Contains(T value)
    {
        return Find(value) != null;
    }

    /// <summary>
    /// 清空链表
    /// </summary>
    public void Clear()
    {
        var current = _head;
        while (current != null)
        {
            var next = current.Next;
            current.List = null;
            current.Next = null;
            current.Previous = null;
            current = next;
        }

        _head = null;
        _tail = null;
        Count = 0;
    }

    /// <summary>
    /// 复制到数组
    /// </summary>
    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0 || arrayIndex >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        var index = arrayIndex;
        for (var node = _head; node != null && index < array.Length; node = node.Next)
        {
            array[index++] = node.Value;
        }
    }

    /// <summary>
    /// 获取指定索引位置的节点
    /// </summary>
    private LinkedListNode<T>? GetNodeAt(int index)
    {
        if (index < 0 || index >= Count)
            return null;

        // 从较近的一端开始遍历
        LinkedListNode<T>? node;
        if (index < Count / 2)
        {
            node = _head;
            for (int i = 0; i < index && node != null; i++)
            {
                node = node.Next;
            }
        }
        else
        {
            node = _tail;
            for (int i = Count - 1; i > index && node != null; i--)
            {
                node = node.Previous;
            }
        }

        return node;
    }

    /// <summary>
    /// 获取枚举器
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        for (var node = _head; node != null; node = node.Next)
        {
            yield return node.Value;
        }
    }

    /// <summary>
    /// 获取枚举器
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 添加元素到尾部
    /// </summary>
    void ICollection<T>.Add(T value)
    {
        AddLast(value);
    }
}
