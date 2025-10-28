using System;
using System.Collections.Generic;

public class PriorityQueue<TItem, TPriority> where TPriority : IComparable<TPriority>
{
    private List<(TItem item, TPriority priority)> heap = new List<(TItem, TPriority)>();

    public int Count => heap.Count;

    public void Enqueue(TItem item, TPriority priority)
    {
        heap.Add((item, priority));
        HeapifyUp(heap.Count - 1);
    }

    public TItem Dequeue()
    {
        if (heap.Count == 0) throw new InvalidOperationException("Queue is empty.");
        var root = heap[0].item;
        var last = heap[heap.Count - 1];
        heap[0] = last;
        heap.RemoveAt(heap.Count - 1);
        HeapifyDown(0);
        return root;
    }

    public void UpdatePriority(TItem item, TPriority newPriority)
    {
        int index = heap.FindIndex(x => EqualityComparer<TItem>.Default.Equals(x.item, item));
        if (index == -1) throw new InvalidOperationException("Item not found.");
        heap[index] = (item, newPriority);
        HeapifyUp(index);
        HeapifyDown(index);
    }

    private void HeapifyUp(int index)
    {
        var child = heap[index];
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            var parent = heap[parentIndex];
            if (child.priority.CompareTo(parent.priority) >= 0) break;
            heap[index] = parent;
            index = parentIndex;
        }
        heap[index] = child;
    }

    private void HeapifyDown(int index)
    {
        var item = heap[index];
        int childIndex;
        while ((childIndex = GetLeftChildIndex(index)) < heap.Count)
        {
            int rightChildIndex = GetRightChildIndex(index);
            if (rightChildIndex < heap.Count &&
                heap[rightChildIndex].priority.CompareTo(heap[childIndex].priority) < 0)
            {
                childIndex = rightChildIndex;
            }

            if (item.priority.CompareTo(heap[childIndex].priority) <= 0) break;

            heap[index] = heap[childIndex];
            index = childIndex;
        }
        heap[index] = item;
    }

    private int GetLeftChildIndex(int parentIndex) => 2 * parentIndex + 1;
    private int GetRightChildIndex(int parentIndex) => 2 * parentIndex + 2;
}
