using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

// https://www.youtube.com/watch?v=5LEVj3PLufE

public class TwoFingerTouchControls : MonoBehaviour
{
    public float pinchSensitivity;
    public float dragSensitivity;

    [Header("Drag")]
    [Range(0, 1)] public float thresholdForSameDirection = 0.8f;
    public UnityEvent<Vector2> onDrag;

    [Header("Pinch")]
    [Range(-1, 0)] public float thresholdForOppositeDirection = -0.8f;
    public UnityEvent<float> onPinch;

    [Header("Rotate")]
    public UnityEvent<float> onRotate;

    [Header("Debug")]
    public LineRenderer debugDragLine;
    public LineRenderer debugPinchLine;
    public LineRenderer debugRotateLine;
    public Graphic finger2Pressed;
    public Text dragInfo;
    public Text rotationInfo;
    public Text pinchInfo;
    float lineDistance = 1;

    public Vector2 fingerPosition1 { get; private set; }
    public Vector2 fingerPosition2 { get; private set; }
    public bool fingerContact1 { get; private set; }
    public bool fingerContact2 { get; private set; }
    public bool registeringInput => fingerContact1 && fingerContact2;


    public float currentPinchDistance { get; private set; }
    public float currentRotationAngle { get; private set; }
    public Vector2 currentDragPosition { get; private set; }

    public float screenScale => Mathf.Min(Screen.width, Screen.height);
    

    public void OnFingerContact1(InputValue input) => CheckStartInput(input.isPressed, fingerContact2);
    public void OnFingerContact2(InputValue input) => CheckStartInput(fingerContact1, input.isPressed);
    void CheckStartInput(bool one, bool two)
    {
        bool isNowRegistering = one && two;

        if (isNowRegistering == true && registeringInput == false)
        {
            currentDragPosition = fingerPosition1 + fingerPosition2 / 2;
            currentPinchDistance = Vector2.Distance(fingerPosition1, fingerPosition2);
            currentRotationAngle = Vector2.SignedAngle(Vector2.up, fingerPosition2 - fingerPosition1);
        }
        fingerContact1 = one;
        fingerContact2 = two;

        finger2Pressed.gameObject.SetActive(registeringInput);
        if (registeringInput == false)
        {
            pinchInfo.text = "";
            rotationInfo.text = "";
        }
    }

    public void OnFingerPosition1(InputValue input) => UpdateInput(input.Get<Vector2>(), fingerPosition2);
    public void OnFingerPosition2(InputValue input) => UpdateInput(fingerPosition1, input.Get<Vector2>());
    public void UpdateInput(Vector2 newFingerPosition1, Vector2 newFingerPosition2)
    {
        if (registeringInput)
        {
            Vector2 fingerDirection1 = newFingerPosition1 - fingerPosition1;
            Vector2 fingerDirection2 = newFingerPosition2 - fingerPosition2;
            float dotProduct = Vector2.Dot(fingerDirection1, fingerDirection2);
            bool sameDirection = true;//dotProduct > thresholdForSameDirection;
            bool oppositeDirection = true;//dotProduct < thresholdForOppositeDirection;

            #region Invoke position change
            Vector2 newDragPosition = newFingerPosition1 + newFingerPosition2 / 2;
            if (newDragPosition != currentDragPosition && sameDirection)
            {
                Vector2 scaledPositionDifference = (newDragPosition - currentDragPosition) / screenScale;
                onDrag.Invoke(scaledPositionDifference);
                dragInfo.text = scaledPositionDifference.ToString();

                currentDragPosition = newDragPosition;
            }
            #endregion

            #region Invoke zoom change
            float newPinchDistance = Vector2.Distance(newFingerPosition1, newFingerPosition2);
            if (newPinchDistance != currentPinchDistance && oppositeDirection)
            {
                float scaledPinchDifference = (newPinchDistance - currentPinchDistance) / screenScale;
                pinchInfo.text = scaledPinchDifference.ToString();
                onPinch.Invoke(scaledPinchDifference);

                currentPinchDistance = newPinchDistance;
            }
            #endregion

            #region Invoke angle change
            float newRotationAngle = Vector2.SignedAngle(Vector2.up, newFingerPosition2 - newFingerPosition1);
            if (newRotationAngle != currentRotationAngle)
            {
                float scaledRotationDifference = newRotationAngle - currentRotationAngle;
                onRotate.Invoke(scaledRotationDifference);
                rotationInfo.text = scaledRotationDifference.ToString();

                currentRotationAngle = newRotationAngle;
            }
            #endregion
        }

        fingerPosition1 = newFingerPosition1;
        fingerPosition2 = newFingerPosition2;
    }

}
