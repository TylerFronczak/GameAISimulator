//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. Catlike Coding http://catlikecoding.com/unity/tutorials/
//  2. https://unity3d.com/learn/tutorials/topics/mobile-touch/pinch-zoom
//  3. https://forum.unity3d.com/threads/click-drag-camera-movement.39513/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomCamera : MonoBehaviour
{
    Transform swivel, stick;

    float zoom = 1f; // 0=ZoomedOut 1=ZoomedIn
    public float zoomDivisor = 300f;
    public float stickPositionZoomedOut = -100f, stickPositionZoomedIn = -5f;
    public float swivelAngleZoomedOut = 85f, swivelAngleZoomedIn = 45f;
    public float moveSpeedZoomedOut = 5f, moveSpeedZoomedIn = 2.5f;

    public float rotationDegreesPerSecond = 180f;
    float rotationAngle;
    public float maxRotationAngleClockwise = 315f;
    public float maxRotationAngleCounterClockwise = 45f;

    bool isSnapped;

    float dragSpeed = 60f;
    Vector2 dragStart;

    bool isCameraControlsEnabled = true;


    public void Initialize(CellGrid grid, bool isStartingOffset)
    {
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);

        rotationAngle = 0f;
        AdjustRotation(0f);

        zoom = 0f;
        AdjustZoom(0f);

        Vector3 startPosition;
        if (isStartingOffset)
        {
            startPosition = new Vector3(
            5f,
            this.transform.position.y,
            (grid.Rows * CellMetrics.offset) / 2f
            );
        }
        else // Center
        {
            startPosition = new Vector3(
            (grid.Columns * CellMetrics.offset) / 2f,
            this.transform.position.y,
            (grid.Rows * CellMetrics.offset) / 2f
            );

        }

        this.transform.position = startPosition;
    }

    void Update()
    {
        if (!isCameraControlsEnabled)
        {
            return;
        }

    #if UNITY_EDITOR
        // Zoom
        float zoomDelta = Input.GetAxis("Zoom");
        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        //Movement
        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f)
        {
            AdjustPosition(xDelta, zDelta);
        }
    #endif

    #if UNITY_ANDROID
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            Vector2 inputVector = new Vector2(touch.position.x, touch.position.y);
            if (!IsVectorOverUIObject(inputVector))
            {
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        dragStart = touch.position;
                        break;
                    case TouchPhase.Moved:
                        Vector2 position = Camera.main.ScreenToViewportPoint(touch.position - dragStart);
                        AdjustPosition(-position.x * dragSpeed, -position.y * dragSpeed);
                        dragStart = touch.position;
                        break;
                }
            }
        }

        if (Input.touchCount == 2)
        {
            Touch touchOne = Input.GetTouch(0);
            Touch touchTwo = Input.GetTouch(1);

            Vector2 touchOnePreviousPosition = touchOne.position - touchOne.deltaPosition;
            Vector2 touchTwoPreviousPosition = touchTwo.position - touchTwo.deltaPosition;

            float previousTouchDeltaMagnitude = (touchOnePreviousPosition - touchTwoPreviousPosition).magnitude;
            float touchDeltaMagnitude = (touchOne.position - touchTwo.position).magnitude;

            float deltaMagnitudeDifference = previousTouchDeltaMagnitude - touchDeltaMagnitude;

            AdjustZoom(deltaMagnitudeDifference / zoomDivisor);
        }
    #endif

        // Rotation
        float rotationDelta = Input.GetAxis("Rotation");
        if (rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }
    }

    public void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float moveSpeed = Mathf.Lerp(moveSpeedZoomedOut, moveSpeedZoomedIn, zoom);
        float distance = moveSpeed * damping * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;

        transform.localPosition = ClampPosition(position);
    }

    Vector3 ClampPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, xMin, xMax);
        position.z = Mathf.Clamp(position.z, zMin, zMax);

        return position;
    }

    private bool isClamping;
    private float xMin;
    private float xMax;
    private float zMin;
    private float zMax;
    public void SetClamp(float xMin, float xMax, float zMin, float zMax)
    {
        this.xMin = xMin;
        this.xMax = xMax;
        this.zMin = zMin;
        this.zMax = zMax;
    }

    public void AdjustRotationExactly(float rotationDelta)
    {
        rotationAngle += rotationDelta;

        if (rotationAngle >= 360f)
        {
            rotationAngle -= 360f;
        }
        else if (rotationAngle < 0f)
        {
            rotationAngle += 360f;
        }

        if (rotationAngle > maxRotationAngleCounterClockwise && rotationAngle <= 180f)
        {
            rotationAngle = maxRotationAngleCounterClockwise;
        }
        else if (rotationAngle < maxRotationAngleClockwise && rotationAngle >= 180)
        {
            rotationAngle = maxRotationAngleClockwise;
        }
        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }

    public void AdjustRotation(float rotationDelta)
    {
        rotationAngle += rotationDelta * rotationDegreesPerSecond * Time.deltaTime;

        if (rotationAngle >= 360f)
        {
            rotationAngle -= 360f;
        }
        else if (rotationAngle < 0f)
        {
            rotationAngle += 360f;
        }

        if (rotationAngle > maxRotationAngleCounterClockwise && rotationAngle <= 180f)
        {
            rotationAngle = maxRotationAngleCounterClockwise;
        }
        else if (rotationAngle < maxRotationAngleClockwise && rotationAngle >= 180)
        {
            rotationAngle = maxRotationAngleClockwise;
        }
        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }

    public void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom - delta);

        // Adjusts stick
        float distance = Mathf.Lerp(stickPositionZoomedOut, stickPositionZoomedIn, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        // Adjusts swivel
        float angle = Mathf.Lerp(swivelAngleZoomedOut, swivelAngleZoomedIn, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    public Vector3 GetStickPosition()
    {
        return stick.position;
    }

    public Quaternion GetSwivelRotation()
    {
        return swivel.rotation;
    }

    public void ToggleCameraControls()
    {
        isCameraControlsEnabled = !isCameraControlsEnabled;
    }
    public void ToggleCameraControls(bool isEnabled)
    {
        isCameraControlsEnabled = isEnabled;
    }

    bool IsVectorOverUIObject(Vector2 inputPosition)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(inputPosition.x, inputPosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
