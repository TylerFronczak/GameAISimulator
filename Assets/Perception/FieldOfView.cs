//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. https://www.youtube.com/watch?v=rQG9aUWarwE
//  2. https://www.youtube.com/watch?v=73Dc5JTCmKI
//
// Notes: 
//  1. The angles of a circle are represented diffently in Unity and trigonometry.
//     In trig, 0 degrees starts at the right side and increases in a counter-clockwise manner.
//     In Unity, 0 degrees start at the top and increases in a clockwise manner.
//     To convert, Unity angle = 90-trigAngle
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FieldOfView : MonoBehaviour
{
    public float viewRadius;
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> VisibleTargets;// { get; private set; }

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        meshFOV = new Mesh();
        meshFOV.name = "FOV";

        meshFilterFOV = GetComponent<MeshFilter>();
        meshFilterFOV.mesh = meshFOV;

        VisibleTargets = new List<Transform>();
    }

    private void FixedUpdate()
    {
        FindVisibleTargets();

        if (isVisualizingFOV)
        {
            DrawFOV();
        }
    }

    Collider[] targetsInViewRadius = new Collider[10];

    public bool IsInView(int layer)
    {
        foreach (Transform potentialTransform in VisibleTargets)
        {
            if (potentialTransform.gameObject.layer == layer)
            {
                return true;
            }
        }

        return false;
    }
    public bool IsInView(string tag)
    {
        foreach (Transform potentialTransform in VisibleTargets)
        {
            if (potentialTransform.CompareTag(tag))
            {
                return true;
            }
        }

        return false;
    }

    public Transform GetClosestTransformInView(int layer)
    {
        Transform closestTransform = null;
        float shortestSqrMagnitude = Mathf.Infinity;

        foreach (Transform potentialTransform in VisibleTargets)
        {
            if (potentialTransform.gameObject.layer == layer)
            {
                float sqrMagnitude = Vector3.SqrMagnitude(potentialTransform.position - transform.position);
                if (sqrMagnitude < shortestSqrMagnitude)
                {
                    shortestSqrMagnitude = sqrMagnitude;
                    closestTransform = potentialTransform;
                }
            }
        }

        return closestTransform;
    }
    public Transform GetClosestTransformInView(string tag)
    {
        Transform closestTransform = null;
        float shortestSqrMagnitude = Mathf.Infinity;

        foreach (Transform potentialTarget in VisibleTargets)
        {
            if (potentialTarget == null)
            {
                break;
            }

            if (potentialTarget.CompareTag(tag))
            {
                float sqrMagnitude = Vector3.SqrMagnitude(potentialTarget.position - potentialTarget.position);
                if (sqrMagnitude < shortestSqrMagnitude)
                {
                    shortestSqrMagnitude = sqrMagnitude;
                    closestTransform = potentialTarget;
                }
            }
        }

        return closestTransform;
    }
    public Transform GetClosestTransformInView(List<string> tags)
    {
        Transform closestTransform = null;
        float shortestSqrMagnitude = Mathf.Infinity;

        foreach (Transform potentialTarget in VisibleTargets)
        {
            if (potentialTarget == null)
            {
                break;
            }

            for (int i = 0; i < tags.Count; i++)
            {
                if (potentialTarget.CompareTag(tags[i]))
                {
                    float sqrMagnitude = Vector3.SqrMagnitude(potentialTarget.position - potentialTarget.position);
                    if (sqrMagnitude < shortestSqrMagnitude)
                    {
                        shortestSqrMagnitude = sqrMagnitude;
                        closestTransform = potentialTarget;
                    }
                }
            }
        }

        return closestTransform;
    }

    public int GetColliderCountInView(int layer)
    {
        int colliderCount = 0;

        foreach (Collider collider in targetsInViewRadius)
        {
            if (collider.gameObject.layer == layer)
            {
                colliderCount++;
            }
        }

        return colliderCount;
    }
    public int GetColliderCountInView(string tag)
    {
        int colliderCount = 0;

        foreach (Collider collider in targetsInViewRadius)
        {
            if (collider.CompareTag(tag))
            {
                colliderCount++;
            }
        }

        return colliderCount;
    }

    void FindVisibleTargets()
    {
        VisibleTargets.Clear();

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            targetsInViewRadius[i] = null;
        }

        int numTargets = Physics.OverlapSphereNonAlloc(transform.position, viewRadius, targetsInViewRadius, targetMask);
        if (numTargets > 10)
        {
            Debug.Log("Consider increasing the size of the collider array because the number of targets being detected by an agent exceeds the array.");
        }

        for (int i = 0; i < numTargets; i++)
        {
            // Likely detected yourself if the transforms positions are the same
            if (targetsInViewRadius[i].transform.position == transform.position)
            {
                continue;
            }

            Transform target = targetsInViewRadius[i].transform;
            Vector3 vectorToTarget = target.position - transform.position;

            if (Vector3.Angle(transform.forward, vectorToTarget.normalized) < viewAngle * 0.5f)
            {
                VisibleTargets.Add(target);
                if (!Physics.Raycast(transform.position, vectorToTarget.normalized, vectorToTarget.magnitude, obstacleMask, QueryTriggerInteraction.Collide))
                {
                    VisibleTargets.Add(target);
                }
            }
        }
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool isAngleGlobal)
    {
        if (!isAngleGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    #region VisualizationFOV
    bool isVisualizingFOV;
    public bool IsVisualizingFOV
    {
        get { return isVisualizingFOV; }
        set
        {
            isVisualizingFOV = value;
            if (!isVisualizingFOV) { meshFOV.Clear(); }
        }
    }

    MeshFilter meshFilterFOV;
    Mesh meshFOV;

    [SerializeField] float raysPerDegree = 1f;
    [SerializeField] int edgeIterations = 2;
    [SerializeField] float edgeDistanceThreshold = 0.5f;

    void DrawFOV()
    {
        int numRays = Mathf.RoundToInt(viewAngle * raysPerDegree);
        float angleBetweenRays = viewAngle / numRays;

        List<Vector3> viewPoints = new List<Vector3>();

        ViewCastInfo oldViewCast = new ViewCastInfo();

        // Angle along the left edge of the FOV.
        float angleStart = transform.eulerAngles.y - viewAngle / 2;

        for (int i = 0; i < numRays; i++)
        {
            float angle = angleStart + angleBetweenRays * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool isEdgeDistanceThresholdExceeded = Mathf.Abs(oldViewCast.length - newViewCast.length) > edgeDistanceThreshold;
                if (oldViewCast.isHit != newViewCast.isHit || (oldViewCast.isHit && newViewCast.isHit && isEdgeDistanceThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.endPoint);
            //Debug.DrawLine(transform.position, transform.position + DirectionFromAngle(angle, true) * viewRadius, Color.red);
            oldViewCast = newViewCast;
        }

        // The viewPoints array sould likely just double as the vertex array, by first inserting the origin point.

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            else
            {
                break;
            }
        }

        meshFOV.Clear();
        meshFOV.vertices = vertices;
        meshFOV.triangles = triangles;
        meshFOV.RecalculateNormals();
    }

    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 direction = DirectionFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, direction, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + direction * viewRadius, viewRadius, globalAngle);
        }
    }
    public struct ViewCastInfo
    {
        public bool isHit;
        public Vector3 endPoint;
        public float length;
        public float angle;

        public ViewCastInfo(bool isHit, Vector3 endPoint, float length, float angle)
        {
            this.isHit = isHit;
            this.endPoint = endPoint;
            this.length = length;
            this.angle = angle;
        }
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeIterations; i++)
        {
            float angle = (minAngle + maxAngle) * 0.5f;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool isEdgeDistanceThresholdExceeded = Mathf.Abs(minViewCast.length - newViewCast.length) > edgeDistanceThreshold;
            if (newViewCast.isHit == minViewCast.isHit && !isEdgeDistanceThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.endPoint;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.endPoint;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }
    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }

    public void ToggleFOV()
    {
        IsVisualizingFOV = !IsVisualizingFOV;
    }

    public void ModifyRadius(float value)
    {
        viewRadius = value;
    }

    public void ModifyAngle(float value)
    {
        viewAngle = value;
    }

    [SerializeField] Agent agent;
    #endregion
}
