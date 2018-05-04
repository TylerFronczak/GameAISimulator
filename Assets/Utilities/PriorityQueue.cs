//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

public class PriorityQueue<T> where T : class
{
    public BinaryMinHeap<T> jobs = new BinaryMinHeap<T>();

    public void Enqueue(T dataClass, float priority)
    {
        jobs.Insert(new HeapNode<T>(priority, dataClass));
    }

    public T Dequeue()
    {
        return jobs.ExtractRoot().dataClass;
    }

    public bool IsEmpty()
    {
        return jobs.IsEmpty();
    }

    public void Clear()
    {
        jobs.Clear();
    }
}
