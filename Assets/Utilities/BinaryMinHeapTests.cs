//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;

public class BinaryMinHeapTests : MonoBehaviour
{
	void Start ()
    {
        BinaryMinHeap<TestClass> minHeap = new BinaryMinHeap<TestClass>();

        TestClass nodeToChangeKey = new TestClass();
        minHeap.Insert(new HeapNode<TestClass>(6, nodeToChangeKey));
        minHeap.Insert(new HeapNode<TestClass>(3, new TestClass()));
        minHeap.Insert(new HeapNode<TestClass>(5, new TestClass()));
        minHeap.Insert(new HeapNode<TestClass>(1, new TestClass()));
        minHeap.Insert(new HeapNode<TestClass>(7, new TestClass()));
        minHeap.Insert(new HeapNode<TestClass>(8, new TestClass()));

        string debugString = "After insertion: ";

        for (int i = 0; i < minHeap.nodes.Count; i++)
        {
            debugString += minHeap.nodes[i].key + ", ";
        }

        Debug.Log(debugString);

        Debug.Log("Extracted value: " + minHeap.ExtractRoot().key);


        debugString = "After extraction: ";
        for (int i = 0; i < minHeap.nodes.Count; i++)
        {
            debugString += minHeap.nodes[i].key + ", ";
        }
        Debug.Log(debugString);

        minHeap.ChangeKey(nodeToChangeKey, 2);
        debugString = "After changing 6 to 2: ";
        for (int i = 0; i < minHeap.nodes.Count; i++)
        {
            debugString += minHeap.nodes[i].key + ", ";
        }
        Debug.Log(debugString);

        minHeap.ChangeKey(nodeToChangeKey, 11);
        debugString = "After changing 2 to 11: ";
        for (int i = 0; i < minHeap.nodes.Count; i++)
        {
            debugString += minHeap.nodes[i].key + ", ";
        }
        Debug.Log(debugString);
    }

    public class TestClass
    {

    }
}
