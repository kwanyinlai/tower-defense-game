using System;
using System.Collections.Generic;

/// <summary>
/// Simple PriorityQueue due to lack of support for PQ for current C# version supported in Unity
/// </summary>
/// <typeparam name="T"></typeparam>
public class PriorityQueue<T>
{
    private readonly List<(T Item, float Priority)> _heap = new();

    public int Count => _heap.Count;

    public void Enqueue(T item, float priority)
    {
        _heap.Add((item, priority));
        HeapifyUp(_heap.Count - 1);
    }

    public T Dequeue()
    {
        if (_heap.Count == 0)
            throw new InvalidOperationException("Queue is empty");

        T result = _heap[0].Item;

        _heap[0] = _heap[^1];
        _heap.RemoveAt(_heap.Count - 1);

        HeapifyDown(0);

        return result;
    }

    public T Peek()
    {
        if (_heap.Count == 0)
            throw new InvalidOperationException("Queue is empty");

        return _heap[0].Item;
    }


    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;

            if (_heap[index].Priority >= _heap[parent].Priority)
                break;

            Swap(index, parent);
            index = parent;
        }
    }

    private void HeapifyDown(int index)
    {
        while (true)
        {
            int left = index * 2 + 1;
            int right = index * 2 + 2;
            int smallest = index;

            if (left < _heap.Count &&
                _heap[left].Priority < _heap[smallest].Priority)
                smallest = left;

            if (right < _heap.Count &&
                _heap[right].Priority < _heap[smallest].Priority)
                smallest = right;

            if (smallest == index)
                break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int a, int b)
    {
        (_heap[a], _heap[b]) = (_heap[b], _heap[a]);
    }
}
