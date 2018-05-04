//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. https://www.youtube.com/watch?v=WCm3TqScBM8
//  2. https://en.wikipedia.org/wiki/Binary_heap
//  3. https://www.youtube.com/watch?v=-WEku8ZnynU&t=66s
//  4. https://en.wikipedia.org/wiki/Heap_(data_structure)
//
// Notes: 
//  1. If indexing from 0, left index = 2i + 1 , right index = 2i + 2, parent index = [(i-1) / 2] 
//  2. If indexing from 1, left index = 2i , right index = 2i + 1, parent index = [i/2]
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;

public class BinaryMinHeap<T> where T : class
{
    public List<HeapNode<T>> nodes = new List<HeapNode<T>>();

    // O(log n)
    public void Insert(HeapNode<T> heapNode)
    {
        nodes.Add(heapNode);

        int insertedNodeIndex = nodes.Count - 1;

        // As long as the index is not zero, inserted node has parent.
        // Swaps up if inserted node's value is lower than its parent.
        while (insertedNodeIndex > 0 && (nodes[insertedNodeIndex].key < nodes[(insertedNodeIndex - 1) / 2].key)) 
        {
            insertedNodeIndex = Swap(insertedNodeIndex, (insertedNodeIndex - 1) / 2);
        }
    }

    // O(log n)
    public HeapNode<T> ExtractRoot()
    {
        int lastElementIndex = nodes.Count - 1;

        Swap(0, lastElementIndex);

        HeapNode<T> nodeToRemove = nodes[lastElementIndex];
        nodes.RemoveAt(lastElementIndex);

        int parentIndex = 0;

        // While there is at least a left child
        while (2 * parentIndex + 1 < nodes.Count)
        {
            // If the left child has a lower key value than its parent
            if (nodes[2 * parentIndex + 1].key < nodes[parentIndex].key)
            {
                // If there is a right child and it has a lower value than the left child
                if (2 * parentIndex + 2 < nodes.Count && nodes[2 * parentIndex + 2].key < nodes[2 * parentIndex + 1].key)
                {
                    // Swap parent with right child and reassign parent index
                    parentIndex = Swap(parentIndex, 2 * parentIndex + 2);
                }
                else
                {
                    // Swap parent with left child and reassign parent index
                    parentIndex = Swap(parentIndex, 2 * parentIndex + 1);
                }
            }
            // If there is a right child and it has a lower value than its parent
            else if (2 * parentIndex + 2 < nodes.Count && nodes[2 * parentIndex + 2].key < nodes[parentIndex].key)
            {
                // Swap parent with right child and reassign parent index
                parentIndex = Swap(parentIndex, 2 * parentIndex + 2);
            }
            else
            {
                // Heap rules are maintained, so no change is required.
                break;
            }
        }

        return nodeToRemove;
    }
    
    // O(n log n) This could be improved by breaking abstraction.
    public void ChangeKey(T dataClass, float newKey)
    {
        int indexOfUpdatedNode = -1;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].dataClass == dataClass)
            {
                indexOfUpdatedNode = i;
                break;
            }
        }

        if (indexOfUpdatedNode == -1)
        {
            Debug.LogWarning("FAIL: ChangeKey could not locate the heapNode containing the specified <T> dataClass");
            return;
        }

        float oldKey = nodes[indexOfUpdatedNode].key;

        // If new key is greater, check for downwards percolation
        if (newKey > oldKey)
        {
            nodes[indexOfUpdatedNode].key = newKey;

            // While there is at least a left child
            while (2 * indexOfUpdatedNode + 1 < nodes.Count)
            {
                // If the new key value is greater than the left child
                if (newKey > nodes[2 * indexOfUpdatedNode + 1].key)
                {
                    // If there is a right child and it has a lower value than the left child
                    if (2 * indexOfUpdatedNode + 2 < nodes.Count && nodes[2 * indexOfUpdatedNode + 2].key < nodes[2 * indexOfUpdatedNode + 1].key)
                    {
                        // Swap parent with right child and reassign parent index
                        indexOfUpdatedNode = Swap(indexOfUpdatedNode, 2 * indexOfUpdatedNode + 2);
                    }
                    else
                    {
                        // Swap parent with left child and reassign parent index
                        indexOfUpdatedNode = Swap(indexOfUpdatedNode, 2 * indexOfUpdatedNode + 1);
                    }
                }
                // If there is a right child and the new key value is greater than the right child
                else if (2 * indexOfUpdatedNode + 2 < nodes.Count && newKey > nodes[2 * indexOfUpdatedNode + 2].key)
                {
                    // Swap parent with right child and reassign parent index
                    indexOfUpdatedNode = Swap(indexOfUpdatedNode, 2 * indexOfUpdatedNode + 2);
                }
                else
                {
                    // Heap rules are maintained, so no change is required.
                    break;
                }
            }
        }
        else if (newKey < oldKey)
        {
            nodes[indexOfUpdatedNode].key = newKey;

            // Swaps up if new key is lower than its parent.
            while (indexOfUpdatedNode > 0 && (newKey < nodes[(indexOfUpdatedNode - 1) / 2].key))
            {
                indexOfUpdatedNode = Swap(indexOfUpdatedNode, (indexOfUpdatedNode - 1) / 2);
            }
        }
        else
        {
            // Nothing has changed
        }
    }

    int Swap(int nodeOneIndex, int nodeTwoIndex)
    {
        HeapNode<T> tempNode = nodes[nodeOneIndex];
        nodes[nodeOneIndex] = nodes[nodeTwoIndex];
        nodes[nodeTwoIndex] = tempNode;

        return nodeTwoIndex;
    }

    public bool IsEmpty()
    {
        if (nodes.Count == 0) {
            return true;
        } else {
            return false;
        }
    }

    public void Clear()
    {
        nodes.Clear();
    }
}

public class HeapNode<T>
{
    public float key;

    public T dataClass;

    public HeapNode(float key, T dataClass)
    {
        this.key = key;
        this.dataClass = dataClass;
    }
}
