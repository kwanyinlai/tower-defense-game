using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    private readonly List<(T Item, float Priority)> _heap = new();

    public float Count => _heap.Count;

    // Add an item with a priority
    public void Enqueue(T item, float priority)
    {
        _heap.Add((item, priority));
        HeapifyUp(_heap.Count - 1);
    }

    // Remove and return the item with the smallest priority
    public T Dequeue()
    {
        if (_heap.Count == 0)
            throw new InvalidOperationException("Queue is empty");

        T result = _heap[0].Item;

        // Move last item to root
        _heap[0] = _heap[^1];
        _heap.RemoveAt(_heap.Count - 1);

        HeapifyDown(0);

        return result;
    }

    // Look at the next item without removing it
    public T Peek()
    {
        if (_heap.Count == 0)
            throw new InvalidOperationException("Queue is empty");

        return _heap[0].Item;
    }

    // --- Heap helpers ---

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
