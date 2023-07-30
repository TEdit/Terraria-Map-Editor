using System;

namespace TEdit.Common.Geometry;

/// <summary>A queue of FloodFillRanges.</summary>
public class FloodFillRangeQueue
{
    FloodFillRange[] array;
    int size;
    int head;

    /// <summary>
    /// Returns the number of items currently in the queue.
    /// </summary>
    public int Count
    {
        get { return size; }
    }

    public FloodFillRangeQueue()
        : this(10000)
    {

    }

    public FloodFillRangeQueue(int initialSize)
    {
        array = new FloodFillRange[initialSize];
        head = 0;
        size = 0;
    }

    /// <summary>Gets the <see cref="FloodFillRange"/> at the beginning of the queue.</summary>
    public FloodFillRange First
    {
        get { return array[head]; }
    }

    /// <summary>Adds a <see cref="FloodFillRange"/> to the end of the queue.</summary>
    public void Enqueue(ref FloodFillRange r)
    {
        if (size + head == array.Length)
        {
            FloodFillRange[] newArray = new FloodFillRange[2 * array.Length];
            Array.Copy(array, head, newArray, 0, size);
            array = newArray;
            head = 0;
        }
        array[head + size++] = r;
    }

    /// <summary>Removes and returns the <see cref="FloodFillRange"/> at the beginning of the queue.</summary>
    public FloodFillRange Dequeue()
    {
        FloodFillRange range = new FloodFillRange();
        if (size > 0)
        {
            range = array[head];
            array[head] = new FloodFillRange();
            head++;//advance head position
            size--;//update size to exclude dequeued item
        }
        return range;
    }

    /// <summary>Remove all FloodFillRanges from the queue.</summary>
    /*public void Clear() 
    {
        if (size > 0)
            Array.Clear(array, 0, size);
        size = 0;
    }*/

}
