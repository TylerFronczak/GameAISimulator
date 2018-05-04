//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;

public class InputMarker : MonoBehaviour
{
    [SerializeField] float timeTillDestroy;

    private void Update()
    {
        timeTillDestroy -= Time.deltaTime;
        if (timeTillDestroy <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
