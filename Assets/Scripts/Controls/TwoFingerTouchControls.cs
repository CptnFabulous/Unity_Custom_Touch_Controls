using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

// https://www.youtube.com/watch?v=5LEVj3PLufE

public class TwoFingerTouchControls : ComplexTouch
{
    [Range(0, 0.5f)] public float sameThreshold = 0.2f;
    [Range(0, 0.5f)] public float oppositeThreshold = 0.2f;
    [Range(0, 0.5f)] public float perpendicularThreshold = 0.2f;
    public UnityEvent<Vector2> onDrag;
    public UnityEvent<float> onPinch;
    public UnityEvent<float> onRotate;

    Vector2 newFingerPos1;
    Vector2 newFingerPos2;
    Vector2 oldFingerPos1;
    Vector2 oldFingerPos2;

    public void OnFingerPosition1(InputValue input) => CheckInput(input.Get<Vector2>(), newFingerPos2);
    public void OnFingerPosition2(InputValue input) => CheckInput(newFingerPos1, input.Get<Vector2>());
    public void CheckInput(Vector2 one, Vector2 two)
    {
        // Sets most recent finger positions
        newFingerPos1 = one;
        newFingerPos2 = two;

        // Check if the player has pressed and moved both fingers
        bool bothFingersPressed = fingerContact1 && fingerContact2;
        bool bothFingersMoved = newFingerPos1 != oldFingerPos1 && newFingerPos2 != oldFingerPos2;

        if (bothFingersMoved == false)
        {
            // If one of the fingers has not moved, do not update old positions until both have moved
            return;
        }
        else if (bothFingersPressed == false)
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
        
        #region Calculate directions and dot product
        // Directions fingers have moved in
        Vector2 fingerDirection1 = (newFingerPos1 - oldFingerPos1).normalized;
        Vector2 fingerDirection2 = (newFingerPos2 - oldFingerPos2).normalized;

        // Dot product of finger directions. Are they moving in the same or opposite directions?
        float fingerDotProduct = Vector2.Dot(fingerDirection1, fingerDirection2);
        bool sameDirection = fingerDotProduct > (1 - sameThreshold);
        bool oppositeDirection = fingerDotProduct < (-1 + oppositeThreshold);
        #endregion

        // To calculate rotation
        // Get 'direction' between original positions
        Vector2 oldRelativeDirection = (oldFingerPos2 - oldFingerPos1).normalized;
        // Calculate the dot products of the new finger directions to the 'relative' direction
        float dotProductRelative1 = Vector2.Dot(oldRelativeDirection, fingerDirection1);
        float dotProductRelative2 = Vector2.Dot(oldRelativeDirection, fingerDirection2);
        // If the new finger directions are opposite each other but perpendicular to the relative direction, it's a rotation. If they're perpendicular but the same, it's a drag. If they're opposite but not perpendicular, it's a pinch.
        bool perp1 = Mathf.Abs(dotProductRelative1) < perpendicularThreshold;
        bool perp2 = Mathf.Abs(dotProductRelative2) < perpendicularThreshold;

        Debug.Log("Rotation check. Dot product 1 = " + dotProductRelative1 + ", dot product 2 = " + dotProductRelative2 + ",  1 is perpendicular = " + perp1 + ", 2 is perpendicular = " + perp2);
        /*
        #region What directions are the fingers moving relative to their original positions?
        // Direction of finger 1 to finger 2, based off old positions
        Vector2 oldRelativeDirection = oldFingerPos2 - oldFingerPos1;
        // Dot products of finger directions relative to the old relative direction
        float dotProductRelative1 = Vector2.Dot(oldRelativeDirection, fingerDirection1);
        float dotProductRelative2 = Vector2.Dot(oldRelativeDirection, fingerDirection2);
        // If the dot products are close to zero (specified by perpendicularThreshold), they are perpendicular to original direction
        
        #endregion
        */
        //Debug.Log("Same direction = " + sameDirection + ", opposite direction = " + oppositeDirection + ", perp1 = " + perp1 + ", perp2 = " + perp2);

        // Code accurately detects dragging and pinching, but rotation detection is off


        bool dragging = sameDirection;//sameDirection; // If fingers are moving in the same direction, player is dragging
        bool pinching = oppositeDirection && !perp1 && !perp2;//oppositeDirection && !perp1 && !perp2; // If opposite directions but not perpendicular, player is pinching/stretching
        bool rotating = oppositeDirection && (perp1 || perp2);//oppositeDirection;// && perp1 && perp2; // If opposite and perpendicular, player is performing a rotation

        #region Invoke position change
        Vector2 oldDragPosition = oldFingerPos1 + oldFingerPos2 / 2;
        Vector2 newDragPosition = newFingerPos1 + newFingerPos2 / 2;
        if (newDragPosition != oldDragPosition && dragging)
        {
            Vector2 scaledPositionDifference = (newDragPosition - oldDragPosition) / screenScale;
            onDrag.Invoke(scaledPositionDifference);
        }
        #endregion

        #region Invoke zoom change
        float oldPinchDistance = Vector2.Distance(oldFingerPos1, oldFingerPos2);
        float newPinchDistance = Vector2.Distance(newFingerPos1, newFingerPos2);
        if (newPinchDistance != oldPinchDistance && pinching)
        {
            float scaledPinchDifference = (newPinchDistance - oldPinchDistance) / screenScale;
            onPinch.Invoke(scaledPinchDifference);
        }
        #endregion

        #region Invoke angle change
        float oldRotationAngle = Vector2.SignedAngle(Vector2.up, oldFingerPos2 - oldFingerPos1);
        float newRotationAngle = Vector2.SignedAngle(Vector2.up, newFingerPos2 - newFingerPos1);
        if (newRotationAngle != oldRotationAngle && rotating)
        {
            float scaledRotationDifference = newRotationAngle - oldRotationAngle;
            onRotate.Invoke(scaledRotationDifference);
        }
        #endregion

        // Overwrite old inputs to match the current ones, so differences can be accurately checked next time
        oldFingerPos1 = newFingerPos1;
        oldFingerPos2 = newFingerPos2;
    }
}