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

    [Range(0, 0.5f)] public float sameThreshold = 0.2f;
    [Range(0, 0.5f)] public float oppositeThreshold = 0.2f;
    [Range(0, 0.5f)] public float perpendicularThreshold = 0.2f;
    public UnityEvent<Vector2> onDrag;
    public UnityEvent<float> onPinch;
    public UnityEvent<float> onRotate;

    bool fingerContact1;
    bool fingerContact2;
    Vector2 newFingerPos1;
    Vector2 newFingerPos2;
    Vector2 oldFingerPos1;
    Vector2 oldFingerPos2;

    Vector2 oldDragPosition;
    float oldPinchDistance;
    float oldRotationAngle;

    public bool registeringInput => fingerContact1 && fingerContact2;



    public float screenScale => Mathf.Min(Screen.width, Screen.height);
    

    public void OnFingerContact1(InputValue input) => CheckStartInput(input.isPressed, fingerContact2);
    public void OnFingerContact2(InputValue input) => CheckStartInput(fingerContact1, input.isPressed);
    void CheckStartInput(bool one, bool two)
    {
        bool isNowRegistering = one && two;

        if (isNowRegistering == true && registeringInput == false)
        {
            oldDragPosition = newFingerPos1 + newFingerPos2 / 2;
            oldPinchDistance = Vector2.Distance(newFingerPos1, newFingerPos2);
            oldRotationAngle = Vector2.SignedAngle(Vector2.up, newFingerPos2 - newFingerPos1);
        }
        fingerContact1 = one;
        fingerContact2 = two;
    }

    public void OnFingerPosition1(InputValue input) => CheckInput(input.Get<Vector2>(), newFingerPos2);
    public void OnFingerPosition2(InputValue input) => CheckInput(newFingerPos1, input.Get<Vector2>());
    public void CheckInput(Vector2 one, Vector2 two)
    {
        // Sets most recent finger positions
        newFingerPos1 = one;
        newFingerPos2 = two;

        bool bothFingersMoved = newFingerPos1 != oldFingerPos1 && newFingerPos2 != oldFingerPos2;
        if (bothFingersMoved == false)
        {
            // If one of the fingers has not moved, do not update old positions until both have moved
            return;
        }
        else if (registeringInput == false)
        {
            // Unless only one finger is pressed, because this means the player has not started a two-finger action yet
            oldFingerPos1 = newFingerPos1;
            oldFingerPos2 = newFingerPos2;
            return;
        }

        RegisterNewInput(); // If player is moving both fingers, register new input
    }

    void RegisterNewInput()
    {
        bool dragging = true;//sameDirection; // If fingers are moving in the same direction, player is dragging
        bool pinching = true;//oppositeDirection && !perp1 && !perp2; // If opposite directions but not perpendicular, player is pinching/stretching
        bool rotating = true;//oppositeDirection;// && perp1 && perp2; // If opposite and perpendicular, player is performing a rotation

        #region Invoke position change
        Vector2 newDragPosition = newFingerPos1 + newFingerPos2 / 2;
        if (newDragPosition != oldDragPosition && dragging)
        {
            Vector2 scaledPositionDifference = (newDragPosition - oldDragPosition) / screenScale;
            onDrag.Invoke(scaledPositionDifference);

            oldDragPosition = newDragPosition;
        }
        #endregion

        #region Invoke zoom change
        float newPinchDistance = Vector2.Distance(newFingerPos1, newFingerPos2);
        if (newPinchDistance != oldPinchDistance && pinching)
        {
            float scaledPinchDifference = (newPinchDistance - oldPinchDistance) / screenScale;
            onPinch.Invoke(scaledPinchDifference);

            oldPinchDistance = newPinchDistance;
        }
        #endregion

        #region Invoke angle change
        float newRotationAngle = Vector2.SignedAngle(Vector2.up, newFingerPos2 - newFingerPos1);
        if (newRotationAngle != oldRotationAngle && rotating)
        {
            float scaledRotationDifference = newRotationAngle - oldRotationAngle;
            onRotate.Invoke(scaledRotationDifference);

            oldRotationAngle = newRotationAngle;
        }
        #endregion

        // Overwrite old inputs to match the current ones, so differences can be accurately checked next time
        oldFingerPos1 = newFingerPos1;
        oldFingerPos2 = newFingerPos2;
    }
}