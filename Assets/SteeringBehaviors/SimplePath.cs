//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References:
//  1. https://www.youtube.com/watch?v=2qGsBClh3hE&index=6&list=PLRqwX-V7Uu6YHt0dtyf4uiw8tKOxQLvlW
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;

public class SimplePath : MonoBehaviour
{
    public float radius;

    [SerializeField] LineRenderer pathCenter;
    [SerializeField] LineRenderer pathZone;

    public List<Vector3> points;

    [SerializeField] bool displayOnAwake;

    private void Awake()
    {
        if (displayOnAwake)
        {
            DisplayPath();
        }
        else
        {
            HidePath();
        }
    }

    public void HidePath()
    {
        pathCenter.gameObject.SetActive(false);
        pathZone.gameObject.SetActive(false);
    }

    public void DisplayPath()
    {
        pathCenter.gameObject.SetActive(true);
        pathZone.gameObject.SetActive(true);

        Vector3 offsetY = new Vector3(0, 0.01f, 0);

        pathCenter.positionCount = points.Count;
        pathCenter.startWidth = 0.1f;
        pathCenter.endWidth = 0.1f;

        pathZone.positionCount = points.Count;
        pathZone.startWidth = radius * 2;
        pathZone.endWidth = radius * 2;

        for (int i = 0; i < points.Count; i++)
        {
            pathCenter.SetPosition(i, points[i] + offsetY);
            pathZone.SetPosition(i, points[i]);
        }
    }
}
