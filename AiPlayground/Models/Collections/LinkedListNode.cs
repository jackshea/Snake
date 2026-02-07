using System;

namespace AiPlayground.Models.Collections;

/// <summary>
/// 双向链表节点
/// </summary>
/// <typeparam name="T">节点数据类型</typeparam>
public class LinkedListNode<T>
{
    /// <summary>
    /// 节点的值
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// 前一个节点
    /// </summary>
    public LinkedListNode<T>? Previous { get; internal set; }

    /// <summary>
    /// 下一个节点
    /// </summary>
    public LinkedListNode<T>? Next { get; internal set; }

    /// <summary>
    /// 所属的链表
    /// </summary>
    public LinkedList<T>? List { get; internal set; }

    public LinkedListNode(T value)
    {
        Value = value;
    }

    public override string? ToString()
    {
        return Value?.ToString();
    }
}
