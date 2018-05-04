//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System;
using System.Collections.Generic;

public class ObjectPool<T> where T : IPoolObject
{
    public List<T> pool = new List<T>();
    private Func<T> delegateNewObject;

    public void SetDelegate(Func<T> delegateNewObject)
    {
        this.delegateNewObject = delegateNewObject;
    }

    public T GetPoolObject()
    {
        T poolObject;
        if (pool.Count > 0)
        {
            int lastIndex = pool.Count - 1;
            poolObject =  pool[lastIndex];
            pool.RemoveAt(lastIndex);
            poolObject.OnPoolRemoval();
        }
        else
        {
            poolObject = delegateNewObject();
        }
        return poolObject;
    }

    public void Add(T poolObject)
    {
        poolObject.ResetForPool();
        pool.Add(poolObject);
    }
    public void Add(T[] poolObjects)
    {
        for (int i = 0; i < poolObjects.Length; i++)
        {
            poolObjects[i].ResetForPool();
        }
        pool.AddRange(poolObjects);
    }
}
