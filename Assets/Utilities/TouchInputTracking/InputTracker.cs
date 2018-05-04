//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;

public class InputTracker : MonoBehaviour
{
    [SerializeField] GameObject touchIndicatorPrefab;
    [SerializeField] Canvas screenSpaceCanvas;

    [SerializeField] bool isEnabled;

    void Update()
    {
        if (isEnabled)
        {
            for (var i = 0; i < Input.touchCount; ++i)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    CreateIndicator(touch.position);
                }
            }
        }
    }

    void CreateIndicator(Vector2 pixelPosition)
    {
        GameObject indicator = Instantiate(touchIndicatorPrefab);
        indicator.transform.parent = screenSpaceCanvas.transform;
        indicator.transform.position = new Vector3(pixelPosition.x, pixelPosition.y, 0);
    }
}
