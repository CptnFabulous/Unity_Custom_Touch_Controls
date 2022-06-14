using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectView : MonoBehaviour
{
    public Transform viewedObject;
    public Camera viewingCamera;

    [Header("Touch control info")]
    public TwoFingerTouchControls twoFingerControls;
    public float rotationSensitivity = 90;
    public float zoomSensitivity = 10;
    public float minZoomDistance = 2;
    public float maxZoomDistance = 20;

    private void Awake()
    {
        twoFingerControls?.onDrag.AddListener(RotateOnPerpendicularAxes);
        twoFingerControls?.onPinch.AddListener(Zoom);
        twoFingerControls?.onRotate.AddListener(RotateOnCameraAxis);
    }
    public void RotateOnPerpendicularAxes(Vector2 input)
    {
        input = rotationSensitivity * input;
        viewedObject.RotateAround(viewedObject.transform.position, viewingCamera.transform.up, -input.x);
        viewedObject.RotateAround(viewedObject.transform.position, viewingCamera.transform.right, input.y);
    }
    public void RotateOnCameraAxis(float angle)
    {
        viewedObject.RotateAround(viewedObject.transform.position, viewedObject.position - viewingCamera.transform.position, angle);
    }
    public void Zoom(float distance)
    {
        float extraDistance = -distance * zoomSensitivity;

        viewedObject.transform.position += extraDistance * viewingCamera.transform.forward;

        DistanceSanityCheck();
    }

    void DistanceSanityCheck()
    {
        Vector3 offsetFromCamera = viewedObject.transform.position - viewingCamera.transform.position;
        offsetFromCamera = Mathf.Clamp(offsetFromCamera.magnitude, minZoomDistance, maxZoomDistance) * offsetFromCamera.normalized;
        viewedObject.transform.position = viewingCamera.transform.position + offsetFromCamera;
    }
}
