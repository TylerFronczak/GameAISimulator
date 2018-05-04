//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. https://www.youtube.com/watch?v=rQG9aUWarwE
//*************************************************************************************************

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    void OnSceneGUI()
    {
        FieldOfView fieldOfView = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fieldOfView.transform.position, Vector3.up, Vector3.forward, 360f, fieldOfView.viewRadius);
        Vector3 viewAngleA = fieldOfView.DirectionFromAngle(-fieldOfView.viewAngle * 0.5f, false);
        Vector3 viewAngleB = fieldOfView.DirectionFromAngle(fieldOfView.viewAngle * 0.5f, false);

        Handles.DrawLine(fieldOfView.transform.position, fieldOfView.transform.position + viewAngleA * fieldOfView.viewRadius);
        Handles.DrawLine(fieldOfView.transform.position, fieldOfView.transform.position + viewAngleB * fieldOfView.viewRadius);

        Handles.color = Color.red;
        for (int i = 0; i < fieldOfView.VisibleTargets.Count; i++)
        {
            Handles.DrawLine(fieldOfView.transform.position, fieldOfView.VisibleTargets[i].position);
        }
    }
}
