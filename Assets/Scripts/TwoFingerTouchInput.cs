using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TwoFingerTouchInput : AdvancedTouchInput
{
    [Header("Values")]
    [Range(0, 0.5f)] public float sameThreshold = 0.2f;
    [Range(0, 0.5f)] public float oppositeThreshold = 0.2f;
    [Range(0, 0.5f)] public float perpendicularThreshold = 0.2f;
    public UnityEvent<Vector2> onDrag;
    public UnityEvent<float> onPinch;
    public UnityEvent<float> onRotate;

    protected override int requiredNumberOfTouches => 2;
    protected override bool inputsHaveChanged => newFingerPos1 != oldFingerPos1 && newFingerPos2 != oldFingerPos2; // Makes sure both fingers have moved after being pressed down
    public Vector2 newFingerPos1 => inputHandler.positions[0];
    public Vector2 newFingerPos2 => inputHandler.positions[1];
    public Vector2 oldFingerPos1 { get; private set; }
    public Vector2 oldFingerPos2 { get; private set; }

    protected override void ProcessInputs()
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

        #region Determine rotation and perpendicular movement
        // To calculate rotation
        // Get 'direction' between original positions
        Vector2 oldRelativeDirection = (oldFingerPos2 - oldFingerPos1).normalized;
        // Calculate the dot products of the new finger directions to the 'relative' direction
        float dotProductRelative1 = Vector2.Dot(oldRelativeDirection, fingerDirection1);
        float dotProductRelative2 = Vector2.Dot(oldRelativeDirection, fingerDirection2);
        // If the new finger directions are opposite each other but perpendicular to the relative direction, it's a rotation. If they're perpendicular but the same, it's a drag. If they're opposite but not perpendicular, it's a pinch.
        bool perp1 = Mathf.Abs(dotProductRelative1) < perpendicularThreshold;
        bool perp2 = Mathf.Abs(dotProductRelative2) < perpendicularThreshold;
        #endregion

        #region Determine if dragging, pinching or rotation
        bool dragging = sameDirection;//sameDirection; // If fingers are moving in the same direction, player is dragging
        bool pinching = oppositeDirection && !perp1 && !perp2;//oppositeDirection && !perp1 && !perp2; // If opposite directions but not perpendicular, player is pinching/stretching
        bool rotating = oppositeDirection && (perp1 || perp2);//oppositeDirection;// && perp1 && perp2; // If opposite and perpendicular, player is performing a rotation
        #endregion

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
    }
    protected override void ResetOldInputs()
    {
        oldFingerPos1 = newFingerPos1;
        oldFingerPos2 = newFingerPos2;
    }
}
