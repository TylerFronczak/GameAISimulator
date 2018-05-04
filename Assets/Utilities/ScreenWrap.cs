//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;

public class ScreenWrap : MonoBehaviour
{
    [SerializeField] float minBoundX;
    [SerializeField] float maxBoundX;

    [SerializeField] float minBoundZ;
    [SerializeField] float maxBoundZ;

    private void FixedUpdate()
    {
        Vector3 position = transform.position;

        if (position.x < minBoundX) {
            position.x = maxBoundX;
        } else if (position.x > maxBoundX) {
            position.x = minBoundX;
        }

        if (position.z < minBoundZ) {
            position.z = maxBoundZ;
        } else if (position.z > maxBoundZ) {
            position.z = minBoundZ;
        }

        transform.position = position;
    }
}
