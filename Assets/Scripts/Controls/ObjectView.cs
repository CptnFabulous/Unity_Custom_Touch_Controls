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
    private void Awake()
    {
        twoFingerControls?.onDrag.AddListener(RotateOnPerpendicularAxes);
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
}
